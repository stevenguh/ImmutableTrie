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
    public class DictionaryBuilderAdd
    {
        [Params(1000, 10000)]
        public int N;

        public ImmutableDictionary<string, int>.Builder immutableDictionary;
        public ImmutableTrieDictionary<string, int>.Builder immutableTrieDictionary;
        public Dictionary<string, int> dictionary;

        [GlobalSetup]
        public void Setup()
        {
            immutableDictionary = ImmutableDictionary.CreateBuilder<string, int>();
            immutableTrieDictionary = ImmutableTrieDictionary.CreateBuilder<string, int>();
            dictionary = new Dictionary<string, int>();
        }

        [Benchmark]
        public void ImmutableDictAdd()
        {
            immutableDictionary.Clear();
            for (int i = 0; i < N; i++)
            {
                immutableDictionary.Add($"{i},{i}", i);
            }
        }

        [Benchmark]
        public void ImmutableTrieDictAdd()
        {
            immutableTrieDictionary.Clear();
            for (int i = 0; i < N; i++)
            {
                immutableTrieDictionary.Add($"{i},{i}", i);
            }
        }

        [Benchmark]
        public void DictionaryAdd()
        {
            dictionary.Clear();
            for (int i = 0; i < N; i++)
            {
                dictionary.Add($"{i},{i}", i);
            }
        }
    }
}
