using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Configuration;

namespace WordFinder.Logic
{
    public class WordFinder
    {
        private static ConcurrentDictionary<string, ConcurrentBag<(int row, int col)>> _indexes;

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

            return strategy == SearchStrategyEnum.Index
                ? FindByIndex(topRepWords, uniqueWordStream, strategy)
                : FindSecuential(topRepWords, uniqueWordStream, strategy);


        }

        public IEnumerable<string> FindByIndex(ConcurrentBag<string> topRepWords, IEnumerable<IGrouping<string, string>> uniqueWordStream, SearchStrategyEnum strategy)
        {
            //create letter index dictionary
            var indexes = buildWordIndex(uniqueWordStream);

            ParallelOptions po = new ParallelOptions() { MaxDegreeOfParallelism = 4 };
            //for every unique word in wordStream
            Parallel.ForEach(uniqueWordStream.Select(w => w.Key), po, (word) =>
            {
                //search if first letter of the word exists in the index dicc
                if (indexes.ContainsKey(word[0]))
                {
                    //positions at this point cannot be null
                    var positions = indexes.GetValueOrDefault(word[0]);
                    Parallel.ForEach(positions, po, (position) =>
                    {
                        var result = SearchByRange(position.row, position.col, word);
                        if (result.found)
                            for (int k = 0; k < result.count; k++)
                            {
                                topRepWords.Add(word);
                            }
                    });
                }
            });

            return topRepWords.GroupBy(w => w).OrderByDescending(o => o.Count()).Select(x => x.Key).Take(10);
        }


        public IEnumerable<string> FindSecuential(ConcurrentBag<string> topRepWords, IEnumerable<IGrouping<string, string>> uniqueWordStream, SearchStrategyEnum strategy) 
        {
            ParallelOptions po = new ParallelOptions() { MaxDegreeOfParallelism = 4 };
            //for every unique word in wordStream
            Parallel.ForEach(uniqueWordStream.Select(w => w.Key), po, (word) =>
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
                                var result = SearchByRange(i, j, word);
                                if (result.found)
                                    for (int k = 0; k < result.count; k++)
                                    {
                                        topRepWords.Add(word);
                                    }
                            }
                            else
                            {
                                if (_wordMatrix[i][j] == word[0] && SearchRecursive(i, j, word, 0))
                                    topRepWords.Add(word);
                            }
                        }
                    }
                }
            });

            return topRepWords.GroupBy(w => w).OrderByDescending(o => o.Count()).Select(x => x.Key).Take(10);
        }

        public bool SearchRecursive(int col, int row, string word, int wordCount)
        {
            //if last word has been found return true
            if (wordCount == word.Length)
                return true;

            //discard if next letter doesnt match and check matrix bounds. 
            if (col >= _wordMatrix.Length || row >= _wordMatrix[col].Length || _wordMatrix[col][row] != word[wordCount])
                return false;

            //recursive call to look horizontally and vertically. Incrementing col and row number
            return SearchRecursive(col, row + 1, word, wordCount + 1) || SearchRecursive(col + 1, row, word, wordCount + 1);
        }


        public (bool found, int count) SearchByRange(int row, int col, string word)
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
            vResult = (wordMatrixColumn.Length >= row + word.Length) && string.Join("", wordMatrixColumn[row..(row + word.Length)]).Equals(word);

            return (hResult || vResult, Convert.ToInt32(hResult) + Convert.ToInt32(vResult));

        }

        /// <summary>
        /// Build an char jaggedArray from a list of strings
        /// </summary>
        /// <param name="matrix">list of words</param>
        private void BuildWordMatrix(IEnumerable<string> matrix)
        {
            _wordMatrix = new char[matrix.Count()][];
            for (int i = 0; i < matrix.Count(); i++)
            {
                _wordMatrix[i] = matrix.ElementAt(i).ToArray();
            }
        }

        private ConcurrentDictionary<char, ConcurrentBag<(int row, int col)>> buildWordIndex(IEnumerable<IGrouping<string, string>> wordStream)
        {
            //use concurrent dictionary to search paralell
            var index = new ConcurrentDictionary<char, ConcurrentBag<(int row, int col)>>();

            //get only unique first letters of the wordstream. This input has already been sanitaze
            HashSet<char> uniqueFirstChars = new HashSet<char>();
            wordStream.Select(w => w.Key).ToList().ForEach(w => uniqueFirstChars.Add(w[0]));

            ParallelOptions po = new ParallelOptions() { MaxDegreeOfParallelism = 4 };
            //look secuentially wordMatrix to build letters indexes based on words to be looked for
            Parallel.ForEach(uniqueFirstChars, po, (w) =>
            {
                for (int i = 0; i < _wordMatrix.Length; i++)
                {
                    for (int j = 0; j < _wordMatrix[i].Length; j++)
                    {
                        if (_wordMatrix[i][j] == w)
                        {
                            //if first letter exists, create an bag of the position under the index key 
                            index.AddOrUpdate(
                                key: w,
                                _ =>
                                {
                                    // Create a new ConcurrentBag<int> if key doesn't exist
                                    var newBag = new ConcurrentBag<(int row, int col)>() { (i, j) };
                                    return newBag;
                                },
                                (key, existingBag) =>
                                {
                                    // Add the value to the existing bag
                                    existingBag.Add((i, j));
                                    return existingBag;
                                }
                            );
                        }
                    }
                }
            });
            return index;
        }


    }


}
