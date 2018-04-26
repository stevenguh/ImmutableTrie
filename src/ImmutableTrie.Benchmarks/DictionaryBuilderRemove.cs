using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Attributes.Exporters;
using BenchmarkDotNet.Attributes.Jobs;

namespace ImmutableTrie.Benchmarks
{
  [ClrJob(isBaseline: true), CoreJob, MonoJob]
  [RPlotExporter, RankColumn]
  [MemoryDiagnoser]
  public class DictionaryBuilderRemove
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
      for (int i = 0; i < N; i++)
      {
        var key = $"{i},{i}";
        immutableDictionary[key] = i;
         immutableTrieDictionary[key] = i;
        dictionary[key] = i;
      }
    }

    [Benchmark]
    public void ImmutableDictRemove()
    {
      for (int i = 0; i < N; i++)
      {
        immutableDictionary.Remove($"{i},{i}");
      }
    }

    [Benchmark]
    public void ImmutableTrieDictRemove()
    {
      for (int i = 0; i < N; i++)
      {
        immutableTrieDictionary.Remove($"{i},{i}");
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