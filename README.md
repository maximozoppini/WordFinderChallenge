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

BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3880/23H2/2023Update/SunValley3)
13th Gen Intel Core i7-1355U, 1 CPU, 12 logical and 10 physical cores
.NET SDK 8.0.303
  [Host]     : .NET 8.0.7 (8.0.724.31311), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.7 (8.0.724.31311), X64 RyuJIT AVX2


| Method               | ConfigFile | Mean        | Error     | StdDev    | Gen0     | Exceptions | Completed Work Items | Lock Contentions | Gen1    | Allocated  |
|--------------------- |----------- |------------:|----------:|----------:|---------:|-----------:|---------------------:|-----------------:|--------:|-----------:|
| **BenchmarkFind_Normal** | **index**      |    **12.05 μs** |  **0.239 μs** |  **0.275 μs** |   **1.9531** |          **-** |               **2.0085** |           **0.0015** |  **0.9766** |   **11.95 KB** |
| BenchmarkFind_Stress | index      |   197.99 μs |  3.618 μs |  6.705 μs |   7.8125 |          - |               2.9990 |           0.0054 |  2.4414 |   49.43 KB |
| **BenchmarkFind_Normal** | **range**      |    **22.67 μs** |  **0.411 μs** |  **0.563 μs** |   **3.1128** |          **-** |               **2.4977** |           **0.0028** |  **1.4038** |   **18.88 KB** |
| BenchmarkFind_Stress | range      | 1,330.14 μs | 26.519 μs | 52.347 μs | 582.0313 |          - |               3.0000 |           0.0117 | 52.7344 | 3557.18 KB |
| **BenchmarkFind_Normal** | **secuential** |    **16.33 μs** |  **0.278 μs** |  **0.320 μs** |   **1.9531** |          **-** |               **1.9879** |           **0.0024** |  **0.9766** |   **11.87 KB** |
| BenchmarkFind_Stress | secuential |   195.03 μs |  3.827 μs |  4.094 μs |   7.8125 |          - |               3.0000 |           0.0078 |  2.4414 |   49.41 KB |
