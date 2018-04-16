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
  public class GetSequential
  {
    [Params(100, 1000, 10000)]
    public int N;

    private ImmutableList<int> immutableList;
    private ImmutableArray<int> immutableArray;
    private ImmutableTrieList<int> trieList;
    private List<int> list;


    [GlobalSetup]
    public void Setup()
    {
      var range = Enumerable.Range(0, N);

      immutableList = ImmutableList<int>.Empty;
      immutableList = immutableList.AddRange(range);

      immutableArray = ImmutableArray<int>.Empty;
      immutableArray = immutableArray.AddRange(range);

      trieList = ImmutableTrieList<int>.Empty;
      trieList = trieList.AddRange(range);

      list = new List<int>();
      list.AddRange(range);
    }

    [Benchmark]
    public void ImmutableList()
    {
      for (int i = 0; i < N; i++)
      {
        var stored = immutableList[i];
      }
    }

    [Benchmark]
    public void ImmutableArray()
    {
      for (int i = 0; i < N; i++)
      {
        var stored = immutableArray[i];
      }
    }

    [Benchmark]
    public void ImmutableTrieList()
    {
      for (int i = 0; i < N; i++)
      {
        var stored = trieList[i];
      }
    }

    [Benchmark]
    public void List()
    {
      for (int i = 0; i < N; i++)
      {
        var stored = list[i];
      }
    }
  }
}

