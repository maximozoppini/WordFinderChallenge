using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using Microsoft.Extensions.Configuration;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Diagnostics.Windows.Configs;

namespace WordFinder.Benchmark
{
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    [ConcurrencyVisualizerProfiler]
    [EtwProfiler]
    [ExceptionDiagnoser]
    public class WordFInderBenchmarks
    {
        public IConfiguration _config;
        [Params("range","index","secuential")]
        public string ConfigFile;

        [GlobalSetup]
        public void Setup() {
            _config = new ConfigurationBuilder().AddJsonFile($"appsettings.{ConfigFile}.json").Build();
        }

        [Benchmark]
        public void BenchmarkFind()
        {

            string[] list = { "enmcsolrdsgi", "bwaeqvplcoxp", "smxmaximolmq", "exiofrxbzqwu", "tnitljbyzxdl", "ytamqsagqzsl", "ctpmarianaob", "marianellaln" };
            var wordFinder = new Logic.WordFinder(_config, list);

            Console.WriteLine(wordFinder.Find(list).Count());
        }

    }

}
