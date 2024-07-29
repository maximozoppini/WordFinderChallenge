using BenchmarkDotNet.Running;
using WordFinder.Benchmark;

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<WordFInderBenchmarks>();
    }

}