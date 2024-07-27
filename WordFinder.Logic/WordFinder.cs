using System.Linq;
using System.Threading.Tasks;

namespace WordFinder.Logic
{
    public class WordFinder
    {

        public char[][] wordMatrix;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="matrix">List of strings with same size</param>
        /// <exception cref="ArgumentException">Matrix input cannot be empty or exceed config size.</exception>
        /// <exception cref="ArgumentNullException">Matrix input cannot be null</exception>
        public WordFinder(IEnumerable<string> matrix)
        {
            //sanity checks
            if (matrix == null)
                throw new ArgumentNullException("word matrix can´t be null");
            if (matrix.Count() == 0)
                throw new ArgumentException("word matrix can´t be empty");

            //TODO: agregar parametro de aplicacion para el tamaño de la matriz
            //matrix size check. It could be squeare or rectangular but no more than X cols and rows
            if (matrix.Count() > 64 || matrix.Any(word => word.Length > 64))
                throw new ArgumentException("word matrix size invalid. Should be less than 64 cols and rows");

            //matrix entries should have the same size
            var firstWordLength = matrix.First().Length;
            if (matrix.Any(word => word.Length != firstWordLength))
                throw new ArgumentException("every word from the matrix must have the same size");

            //build jaggedArray matrix from list<string> for further processing.
            BuildWordMatrix(matrix);
        }

        /// <summary>
        /// Build an char jaggedArray from a list of strings
        /// </summary>
        /// <param name="matrix">list of words</param>
        public void BuildWordMatrix(IEnumerable<string> matrix)
        {
            this.wordMatrix = new char[matrix.Count()][];
            for (int i = 0; i < matrix.Count(); i++) {
                this.wordMatrix[i] = matrix.ElementAt(i).ToArray();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wordstream"></param>
        /// <returns></returns>
        public IEnumerable<string> Find(IEnumerable<string> wordstream)
        {
            List<string> topRepWords = new List<string>();

            //sanity check for wordsStream
            if (wordstream == null || wordstream.Any(word => string.IsNullOrEmpty(word) || string.IsNullOrWhiteSpace(word)))
                throw new ArgumentException("wordStream can´t be null or have empty space");

            //remove repeated words to avoid re-check matrix
            var uniqueWordStream = wordstream.GroupBy(word => word);

            //for every word in wordStream
            foreach (var word in uniqueWordStream.Select(w => w.Key))
            {
                //first step is to look recursively inside the matrix the first character of the word. If found, then look right and down.
                for (int i = 0; i < wordMatrix.Length; i++)
                {
                    for (int j = 0; j < wordMatrix[i].Length; j++)
                    {
                        //if (wordMatrix[i][j] == word[0] && FindRecursive(i, j, word, 0))
                        //topRepWords.Add(word);

                        if (wordMatrix[i][j] == word[0])
                        {
                            var result = FindByRange(i, j, word);
                            if (result.found)
                                topRepWords.AddRange(Enumerable.Repeat(word, result.count));
                        }
                    }
                }
            }

            return topRepWords.GroupBy(w => w).OrderByDescending(o => o.Count()).Select(x => x.Key).Take(10);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wordstream"></param>
        /// <returns></returns>
        public IEnumerable<string> FindParalell(IEnumerable<string> wordstream)
        {
            List<string> topRepWords = new List<string>();

            //sanity check for wordsStream
            if (wordstream == null || wordstream.Any(word => string.IsNullOrEmpty(word) || string.IsNullOrWhiteSpace(word)))
                throw new ArgumentException("wordStream can´t be null or have empty space");

            //remove repeated words to avoid re-check matrix
            var uniqueWordStream = wordstream.GroupBy(word => word);

            //for every word in wordStream
            Parallel.ForEach(uniqueWordStream.Select(w => w.Key), (word) =>
            {
                //first step is to look recursively inside the matrix the first character of the word. If found, then look right and down.
                for (int i = 0; i < wordMatrix.Length; i++)
                {
                    for (int j = 0; j < wordMatrix[i].Length; j++)
                    {
                        if (wordMatrix[i][j] == word[0])
                        {
                            var result = FindByRange(i, j, word);
                            if (result.found)
                                topRepWords.AddRange(Enumerable.Repeat(word, result.count));
                        }
                    }
                }
            });

            return topRepWords.GroupBy(w => w).OrderByDescending(o => o.Count()).Select(x => x.Key).Take(10);
        }

        /// <summary>
        /// This method will look recursively every char of the word horizontally and vertically.
        /// </summary>
        /// <param name="col">wordMatrix col number</param>
        /// <param name="row">wordMatrix row number</param>
        /// <param name="word">word to be found</param>
        /// <param name="wordCount">amount of letters of the word found</param>
        /// <returns></returns>
        public bool FindRecursive(int col, int row, string word, int wordCount) {

            //if last word has been found return true
            if (wordCount == word.Length)
                return true;

            //discard if next letter doesnt match and check matrix bounds. 
            if (col >= wordMatrix.Length || row >= wordMatrix[col].Length || wordMatrix[col][row] != word[wordCount])
                return false;

            //TODO: Check remaining letters with the size of the matrix

            //recursive call to look horizontally and vertically. Incrementing col and row number
            return FindRecursive(col, row + 1, word, wordCount + 1) || FindRecursive(col + 1, row, word, wordCount + 1);
        }


        public bool FindRecursive2(int col, int row, string word, int wordCount, bool initial = true)
        {

            //if last word has been found return true
            if (wordCount == word.Length)
                return true;

            //discard if next letter doesnt match and check matrix bounds. 
            if (col >= wordMatrix.Length || row >= wordMatrix[col].Length)
                return false;

            //dont re-check first letter.
            if (!initial || wordMatrix[col][row] != word[wordCount])
                return false;

            //recursive call to look horizontally and vertically. Incrementing col and row number
            return FindRecursive2(col, row + 1, word, wordCount + 1, false) || FindRecursive2(col + 1, row, word, wordCount + 1, false);
        }


        public (bool found, int count) FindByRange(int row, int col, string word)
        {
            bool hResult = false;
            bool vResult = false;
            int count = 0;

            //check if horizontally the word to be search fit and if word match
            hResult = (wordMatrix[row].Length >= col + word.Length) && string.Join("", wordMatrix[row][col..(col + word.Length)]).Equals(word);
            
            //check if vertically the word to be search fit and match
            //first get column as array
            var wordMatrixColumn = wordMatrix.GetColumn(col).ToArray();
            //check word
            vResult = (wordMatrixColumn.Length >= row + word.Length ) && string.Join("",wordMatrixColumn[row..(row + word.Length)]).Equals(word);

            return (hResult || vResult, Convert.ToInt32(hResult)+ Convert.ToInt32(vResult));
            
        }

        
    }

    
}
