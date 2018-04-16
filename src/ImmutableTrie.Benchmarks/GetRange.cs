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
  public class GetRange
  {
    [Params(100, 1000, 10000)]
    public int N;

    private ImmutableList<int> immutableList;
    private ImmutableTrieList<int> trieList;
    private List<int> list;


    [GlobalSetup]
    public void Setup()
    {
      var range = Enumerable.Range(0, N);

      immutableList = ImmutableList<int>.Empty;
      immutableList = immutableList.AddRange(range);

      trieList = ImmutableTrieList<int>.Empty;
      trieList = trieList.AddRange(range);

      list = new List<int>();
      list.AddRange(range);
    }

    [Benchmark]
    public void ImmutableList()
    {
      int count = immutableList.Count;
      for(int i = 0; i < immutableList.Count / 2; i++)
      {
        var range = immutableList.GetRange(i, count);
        count -= 2;
      }
    }

    [Benchmark]
    public void ImmutableTrieList()
    {
      int count = trieList.Count;
      for(int i = 0; i < trieList.Count / 2; i++)
      {
        var range = trieList.GetRange(i, count);
        count -= 2;
      }
    }

    [Benchmark]
    public void List()
    {
      int count = list.Count;
      for(int i = 0; i < list.Count / 2; i++)
      {
        var range = list.GetRange(i, count);
        count -= 2;
      }
    }
  }
}

