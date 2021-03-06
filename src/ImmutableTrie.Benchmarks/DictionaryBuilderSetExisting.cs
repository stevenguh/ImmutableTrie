using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Attributes.Exporters;
using BenchmarkDotNet.Attributes.Jobs;

namespace ImmutableTrie.Benchmarks
{
    [ClrJob(isBaseline: true), CoreJob, MonoJob]
    [RPlotExporter, RankColumn]
    [MemoryDiagnoser]
    public class DictionaryBuilderSetExisting
    {
        [Params(1000, 10000, 100000)]
        public int N;

        public ImmutableDictionary<string, int>.Builder immutableDictionary;
        public ImmutableTrieDictionary<string, int>.Builder immutableTrieDictionary;
        public Dictionary<string, int> dictionary;
        public Random rnd = new Random();

        [GlobalSetup]
        public void Setup()
        {
            immutableDictionary = ImmutableDictionary.CreateBuilder<string, int>();
            immutableTrieDictionary = ImmutableTrieDictionary.CreateBuilder<string, int>();
            dictionary = new Dictionary<string, int>();
            for (int i = 0; i < N; i++)
            {
                var key = $"{i},{i}";
                immutableDictionary[key] = i;
                immutableTrieDictionary[key] = i;
                dictionary[key] = i;
            }
        }

        [Benchmark]
        public void ImmutableDictSet()
        {
            for (int i = 0; i < N; i++)
            {
                immutableDictionary[$"{i},{i}"] = rnd.Next();
            }
        }

        [Benchmark]
        public void ImmutableTrieDictSet()
        {
            for (int i = 0; i < N; i++)
            {
                immutableTrieDictionary[$"{i},{i}"] = rnd.Next();
            }
        }

        [Benchmark]
        public void DictSet()
        {
            for (int i = 0; i < N; i++)
            {
                dictionary[$"{i},{i}"] = rnd.Next();
            }
        }
    }
}
