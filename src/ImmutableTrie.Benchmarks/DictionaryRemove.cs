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
    public class DictionaryRemove
    {
        [Params(1000, 10000)]
        public int N;

        public ImmutableDictionary<string, int> immutableDictionary;
        public ImmutableTrieDictionary<string, int> immutableTrieDictionary;
        public Dictionary<string, int> dictionary;

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
        public void ImmutableDictRemove()
        {
            var temp = immutableDictionary;
            for (int i = 0; i < N; i++)
            {
                temp = temp.Remove($"{i},{i}");
            }
        }

        [Benchmark]
        public void ImmutableTrieDictRemove()
        {
            var temp = immutableTrieDictionary;
            for (int i = 0; i < N; i++)
            {
                temp = temp.Remove($"{i},{i}");
            }
        }

        [Benchmark]
        public void DictRemove()
        {
            for (int i = 0; i < N; i++)
            {
                dictionary.Remove($"{i},{i}");
            }
        }
    }
}
