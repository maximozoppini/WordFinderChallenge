# Word Finder

Given a list of words, the project needs to build a matrix in order to let the user to search that word matrix sending a list of words to be find. The matrix size can´t be more than 64x64 and it can be square or rectangular. The list of words that the user sends to be looked for doesn´t have a limit.
Words can be found either horizontally (left to right) or vertically (top to bottom).  
The program will return the top 10 most ocurring words found inside the matrix.

## Implementation

I have implemented three ways to search the words inside the matrix. I have call them strategies and they are Range, Index and Secuential. At first I thought about the secuential way to look words but after that I realized that I was looping many times the matrix. 
Range strategy tries to avoid looping so many times the matrix but still is not performing too good. 
Index was a solution for me, looping just one time the matrix to build a dictionary to serve as a index structure helped reducing times. 

### Strategies

##### Strategy Index

This strategy will build a Dictionary<char,ConcurrentBag<(int row, int col)>> from the wordstream and the wordMatrix. The key is the first letter of the words inside the wordstream and the value is a list of coordinates from the matrix that those letters are found. 
After that it will search paralell wise the wordstream and if the Dictionary contains the first lettre of the word it gets the list of coordinates and starts looking by range horizontally or vertically the word. 
By doing this, Im looking just 1 time recursivly the matrix.
Acoording to the benchmark this is the quickest way to search words inside the matrix. 

##### Strategy Range

This strategy will loop the list of words inside the wordstream using Paralell.ForEch, so for every word it will loop the wordMatrix and check if first letter of the word matches. 
If so, it will get a slice horizontally and vertically from that coordinate + word.length. 
If the slice fits the matrix´s bounds, it will compare the word whith the slice. 
The strategy reduces the amount of times the matrix is looped.


##### Strategy Secuential

This strategy will loop the list of words inside the wordstream using Paralell.ForEch, so for every word it will loop the wordMatrix and check if first letter of the word matches. 
If so, it will check the next position (down and right) againts the next word letter. This will be executed recursivly until the count reaches the size of the word length. 
The strategy finds correctly the words inside the matrix but its looping so many times the jagged array.  

## Running UnitTest

The solution has a UnitTest project called WordFinder.Test. WordFinderTests class contains a list of test cases that validates results and validations.
The project has a appconfig.json file where the strategy can be changed.
"SearchStrategy": "Index|Secuential|Range"

## Running Benchmarks

The solution has a Benchmark project called WordFinder.Benchmark. To run benchmarks you must change from Debug to Release and Run the WordFinder.Benchmark project. This project will perform an X amount of executions to the WordFinder.Logic class using the three strategies and with a small and big set of data. 
Examples of different runs results:


