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
    public class DictionarySetExisting
    {
        [Params(1000, 10000)]
        public int N;

        public ImmutableDictionary<string, int> immutableDictionary;
        public ImmutableTrieDictionary<string, int> immutableTrieDictionary;
        public Dictionary<string, int> dictionary;
        public Random rnd = new Random();

        [GlobalSetup]
        public void Setup()
        {
            immutableDictionary = ImmutableDictionary.Create<string, int>();
            immutableTrieDictionary = ImmutableTrieDictionary.Create<string, int>();
            dictionary = new Dictionary<string, int>();
            for (int i = 0; i < N; i++)
            {
                var key = $"{i},{i}";
                immutableDictionary = immutableDictionary.SetItem(key, i);
                immutableTrieDictionary = immutableTrieDictionary.SetItem(key, i);
                dictionary[key] = i;
            }
        }

        [Benchmark]
        public void ImmutableDictSet()
        {
            var temp = immutableDictionary;
            for (int i = 0; i < N; i++)
            {
                temp = temp.SetItem($"{i},{i}", rnd.Next());
            }
        }

        [Benchmark]
        public void ImmutableTrieDictSet()
        {
            var temp = immutableTrieDictionary;
            for (int i = 0; i < N; i++)
            {
                temp = temp.SetItem($"{i},{i}", rnd.Next());
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
