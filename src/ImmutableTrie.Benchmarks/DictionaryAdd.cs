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
  public class DictionaryAdd
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
    }

    [Benchmark]
    public void ImmutableDictAdd()
    {
      var temp = immutableDictionary;
      for (int i = 0; i < N; i++)
      {
        temp = temp.Add($"{i},{i}", i);
      }
    }

    [Benchmark]
    public void ImmutableTrieDictAdd()
    {
      var temp = immutableTrieDictionary;
      for (int i = 0; i < N; i++)
      {
        temp = temp.Add($"{i},{i}", i);
      }
    }

    [Benchmark]
    public void DictAdd()
    {
      dictionary.Clear();
      for (int i = 0; i < N; i++)
      {
        dictionary.Add($"{i},{i}", i);
      }
    }
  }
}