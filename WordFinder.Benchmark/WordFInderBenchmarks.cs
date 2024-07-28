using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordFinder.Benchmark
{
    [MemoryDiagnoser]
    public class WordFInderBenchmarks
    {
        public IConfiguration _config;
        [Params("appsettings.range.json", "appsettings.secuential.json")]
        public string ConfigFile;

        [GlobalSetup]
        public void Setup() { 
            _config = new ConfigurationBuilder().AddJsonFile(ConfigFile).Build();

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
