using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Configuration;

namespace WordFinder.Logic
{
    public class WordFinder
    {

        private char[][] _wordMatrix;
        public char[][] WordMatrix
        {
            get { return _wordMatrix; }
        }

        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="matrix">List of strings with same size</param>
        /// <exception cref="ArgumentException">Matrix input cannot be empty or exceed config size.</exception>
        /// <exception cref="ArgumentNullException">Matrix input cannot be null</exception>
        public WordFinder(IConfiguration configuration, IEnumerable<string> matrix)
        {
            //sanity checks
            if (matrix == null)
                throw new ArgumentNullException("word matrix can´t be null");
            if (matrix.Count() == 0)
                throw new ArgumentException("word matrix can´t be empty");

            //matrix size check. It could be squeare or rectangular but no more than X cols and rows
            _configuration = configuration;
            int maxWordMatrixSize = int.Parse(_configuration.GetSection("WordMatrixMaxSize").Value);
            if (matrix.Count() > maxWordMatrixSize || matrix.Any(word => word.Length > maxWordMatrixSize))
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
            _wordMatrix = new char[matrix.Count()][];
            for (int i = 0; i < matrix.Count(); i++) {
                _wordMatrix[i] = matrix.ElementAt(i).ToArray();
            }
        }

        //public IEnumerable<string> Find(IEnumerable<string> wordstream)
        //{
        //    List<string> topRepWords = new List<string>();

        //    //sanity check for wordsStream
        //    if (wordstream == null || wordstream.Any(word => string.IsNullOrEmpty(word) || string.IsNullOrWhiteSpace(word)))
        //        throw new ArgumentException("wordStream can´t be null or have empty space");

        //    //remove repeated words to avoid re-check matrix
        //    var uniqueWordStream = wordstream.GroupBy(word => word);

        //    //for every word in wordStream
        //    foreach (var word in uniqueWordStream.Select(w => w.Key))
        //    {
        //        //first step is to look recursively inside the matrix the first character of the word. If found, then look right and down.
        //        for (int i = 0; i < _wordMatrix.Length; i++)
        //        {
        //            for (int j = 0; j < _wordMatrix[i].Length; j++)
        //            {
                        
        //                if (_wordMatrix[i][j] == word[0])
        //                {
        //                    var result = FindByRange(i, j, word);
        //                    if (result.found)
        //                        topRepWords.AddRange(Enumerable.Repeat(word, result.count));
        //                }
        //            }
        //        }
        //    }

        //    return topRepWords.GroupBy(w => w).OrderByDescending(o => o.Count()).Select(x => x.Key).Take(10);
        //}

        public IEnumerable<string> Find(IEnumerable<string> wordstream)
        {
            //had to use concurrent bag because i was getting nulls when inserting values to list inside a paralell foreach
            ConcurrentBag<string> topRepWords = new ConcurrentBag<string>();
            SearchStrategyEnum strategy = Enum.Parse<SearchStrategyEnum>(_configuration.GetSection("SearchStrategy").Value);

            //sanity check for wordsStream
            if (wordstream == null || wordstream.Any(word => string.IsNullOrEmpty(word) || string.IsNullOrWhiteSpace(word)))
                throw new ArgumentException("wordStream can´t be null or have empty space");

            //remove repeated words to avoid re-check matrix
            var uniqueWordStream = wordstream.GroupBy(word => word);

            ParallelOptions po = new ParallelOptions() { MaxDegreeOfParallelism = 4 };
            //for every word in wordStream
            Parallel.ForEach(uniqueWordStream.Select(w => w.Key),po, (word) =>
            {
                //first step is to look recursively inside the matrix the first character of the word. If found, then look right and down.
                for (int i = 0; i < _wordMatrix.Length; i++)
                {
                    for (int j = 0; j < _wordMatrix[i].Length; j++)
                    {
                        if (_wordMatrix[i][j] == word[0])
                        {
                            if (strategy == SearchStrategyEnum.Range)
                            {
                                var result = FindByRange(i, j, word);
                                if (result.found)
                                    for (int k = 0; k < result.count; k++){
                                        topRepWords.Add(word);
                                    }
                            }
                            else {
                                if (_wordMatrix[i][j] == word[0] && FindRecursive(i, j, word, 0))
                                    topRepWords.Add(word);
                            }
                        }
                    }
                }
            });

            return topRepWords.GroupBy(w => w).OrderByDescending(o => o.Count()).Select(x => x.Key).Take(10);
        }

        public bool FindRecursive(int col, int row, string word, int wordCount)
        {
            //if last word has been found return true
            if (wordCount == word.Length)
                return true;

            //discard if next letter doesnt match and check matrix bounds. 
            if (col >= _wordMatrix.Length || row >= _wordMatrix[col].Length || _wordMatrix[col][row] != word[wordCount])
                return false;

            //recursive call to look horizontally and vertically. Incrementing col and row number
            return FindRecursive(col, row + 1, word, wordCount + 1) || FindRecursive(col + 1, row, word, wordCount + 1);
        }


        public (bool found, int count) FindByRange(int row, int col, string word)
        {
            bool hResult = false;
            bool vResult = false;
            int count = 0;

            //check if horizontally the word to be search fit and if word match
            hResult = (_wordMatrix[row].Length >= col + word.Length) && string.Join("", _wordMatrix[row][col..(col + word.Length)]).Equals(word);
            
            //check if vertically the word to be search fit and match
            //first get column as array
            var wordMatrixColumn = _wordMatrix.GetColumn(col).ToArray();
            //check word
            vResult = (wordMatrixColumn.Length >= row + word.Length ) && string.Join("",wordMatrixColumn[row..(row + word.Length)]).Equals(word);

            return (hResult || vResult, Convert.ToInt32(hResult)+ Convert.ToInt32(vResult));
            
        }

        
    }

    
}
