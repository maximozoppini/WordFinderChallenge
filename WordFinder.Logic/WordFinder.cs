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
        /// <param name="configuration">IConfiguration object injected to the class</param>
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
                throw new ArgumentException($"word matrix size invalid. Should be less than {maxWordMatrixSize} cols and rows");

            //matrix entries should have the same size
            var firstWordLength = matrix.First().Length;
            if (matrix.Any(word => word.Length != firstWordLength))
                throw new ArgumentException("every word from the matrix must have the same size");

            //build jaggedArray matrix from list<string> for further processing.
            BuildWordMatrix(matrix);
        }

        /// <summary>
        /// Find method that will search words inside the wordstream parameter inside the matrix. 
        /// The type of search will be determine by the SearchStrategy enum. 
        /// The function will return the top 10 most ocurring words from the stream. 
        /// </summary>
        /// <param name="wordstream">List of words to be find inside the word matrix</param>
        /// <returns>List<string> containing the top 10 words from the stream found in the matrix</string></returns>
        /// <exception cref="ArgumentException">wordstream should not be null or empty</exception>
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
                ? FindByIndex(topRepWords, uniqueWordStream)
                : FindSecuential(topRepWords, uniqueWordStream, strategy);


        }

        /// <summary>
        /// The method will return the top 10 words found inside the matrix from the uniqueWordStream
        /// It will create a dictionary from every first letter or the uniqueWordStream and a list of coordinates of those letters.
        /// Finally it will loop the dictionary and check if the words are found either horizontally or vertically depending on those 
        /// coordinates. 
        /// </summary>
        /// <param name="topRepWords">list to be return with words</param>
        /// <param name="uniqueWordStream">Disctint list of words from the stream to be looked for</param>
        /// <returns></returns>
        private IEnumerable<string> FindByIndex(ConcurrentBag<string> topRepWords, IEnumerable<IGrouping<string, string>> uniqueWordStream)
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
                    //for each position it will check by range either horizontally or vertically if word is there.
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


        /// <summary>
        /// The method will return the top 10 words from the wordsStream that have been found in the matrix
        /// Depending on the strategy (Range or Recursive) it will loop using Paralell function every unique word if it is found
        /// inside the matrix. 
        /// It will loop the matrix until the first letter of the word is found and then it will look recursivly or by range the word. 
        /// </summary>
        /// <param name="topRepWords">list to be return</param>
        /// <param name="uniqueWordStream">Disctint list of words from the stream to be looked for</param>
        /// <param name="strategy">Enum for the looping strategy</param>
        /// <returns></returns>
        private IEnumerable<string> FindSecuential(ConcurrentBag<string> topRepWords, IEnumerable<IGrouping<string, string>> uniqueWordStream, SearchStrategyEnum strategy) 
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

        /// <summary>
        /// This method is the recursive strategy that if the first letter of the word has been found, It will check if 
        /// right and down for the next letter of the word in a recursive way. Until the count reaches the size of the word. 
        /// Checking if array indexes are not outside the wordMatrix
        /// </summary>
        /// <param name="col">x index number</param>
        /// <param name="row">y index number</param>
        /// <param name="word">word to be looked for</param>
        /// <param name="wordCount">number of chars that have been found of the word</param>
        /// <returns>true of it has been found, false if not</returns>
        private bool SearchRecursive(int col, int row, string word, int wordCount)
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

        /// <summary>
        /// This method is the range strategy that if the first letter of the word has been foun, It will check right and down 
        /// the entire word against a horizontal and vertical slice of the matrix. 
        /// It will also check if the word size fits either vertically or horizontally to avoid looking outside the matrix
        /// </summary>
        /// <param name="row">x index number</param>
        /// <param name="col">y indiex number</param>
        /// <param name="word">word to be looked for</param>
        /// <returns>Named tuple with True or false if word has been found and if so, the count of times the word has been found</returns>
        private (bool found, int count) SearchByRange(int row, int col, string word)
        {
            bool hResult = false;
            bool vResult = false;

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

        /// <summary>
        /// This method builds the dictionary of key: first letter of word, value: list of coordinates tuples. 
        /// It will look paralell wise the wordMatrix and check with the unique set of first letters of the wordstream to be found if matches.
        /// If so it will add or update the dictionary on that key with the coordinate. 
        /// </summary>
        /// <param name="wordStream">list of words to be found</param>
        /// <returns>dictionary that contains for each first letter of input, the coordintes of the ocurrencies inside the matrix</returns>
        private ConcurrentDictionary<char, ConcurrentBag<(int row, int col)>> buildWordIndex(IEnumerable<IGrouping<string, string>> wordStream)
        {
            //use concurrent dictionary to search paralell
            var index = new ConcurrentDictionary<char, ConcurrentBag<(int row, int col)>>();

            //get only unique first letters of the wordstream. This input has already been sanitaze
            HashSet<char> uniqueFirstChars = new HashSet<char>();
            wordStream.Select(w => w.Key).ToList().ForEach(w => uniqueFirstChars.Add(w[0]));

            // Parallel.For loop to process each row of the jagged array
            Parallel.For(0, _wordMatrix.Length,  row =>
            {
                // Process each character in the current row
                Parallel.For(0, _wordMatrix[row].Length, col =>
                {
                    char letter = _wordMatrix[row][col];
                    if (uniqueFirstChars.Contains(letter))
                    {
                        //if first letter exists, create an bag of the position under the index key 
                        index.AddOrUpdate(
                            key: letter,
                            _ =>
                            {
                                // Create a new ConcurrentBag<int> if key doesn't exist
                                var newBag = new ConcurrentBag<(int row, int col)>() { (row, col) };
                                return newBag;
                            },
                            (key, existingBag) =>
                            {
                                // Add the value to the existing bag
                                existingBag.Add((row, col));
                                return existingBag;
                            }
                        );
                    }
                });
            });

            return index;
        }


    }


}
