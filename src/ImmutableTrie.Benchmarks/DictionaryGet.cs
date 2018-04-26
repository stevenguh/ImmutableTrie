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
  public class DictionaryGet
  {
    [Params(1000, 10000)]
    public int N;

    public ImmutableDictionary<string, int> immutableDictionary;
    public ImmutableTrieDictionary<string, int> immutableTrieDictionary;
    public Dictionary<string, int> dictionary;


    [GlobalSetup]
    public void Setup()
    {
      var content = Enumerable.Range(0, N).Select(i => new KeyValuePair<string, int>(i.ToString(), i));

      immutableDictionary = ImmutableDictionary.CreateRange<string, int>(content);
      immutableTrieDictionary = ImmutableTrieDictionary.CreateRange<string, int>(content);
      dictionary = new Dictionary<string, int>(immutableTrieDictionary);

    }

    [Benchmark]
    public void ImmutableDictGet()
    {
      var temp = immutableDictionary;
      for (int i = 0; i < N; i++)
      {
        var value = immutableDictionary[i.ToString()];
      }
    }

    [Benchmark]
    public void ImmutableTrieDictGet()
    {
      var temp = immutableTrieDictionary;
      for (int i = 0; i < N; i++)
      {
        var value = immutableDictionary[i.ToString()];
      }
    }

    [Benchmark]
    public void DictGet()
    {
      for (int i = 0; i < N; i++)
      {
        var value = dictionary[i.ToString()];
      }
    }
  }
}