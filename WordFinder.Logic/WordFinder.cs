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
            if(wordstream == null || wordstream.Any(word => string.IsNullOrEmpty(word) || string.IsNullOrWhiteSpace(word)))
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
                        if (wordMatrix[i][j] == word[0] && FindRecursive(i, j, 0, word))
                            topRepWords.Add(word);
                    }
                }
            }

            return topRepWords;
        }

        public bool FindRecursive(int col, int row, int count, string word) {


            return false;
        }
    }
}
