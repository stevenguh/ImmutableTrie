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
  public class InsertAtZero
  {
    [Params(1000)]
    public int N;

    [Benchmark]
    public void ImmutableList()
    {
      var list = ImmutableList<int>.Empty;
      for(int i = 0; i < N; i++)
      {
        list = list.Insert(0, 0);
      }
    }

    [Benchmark]
    public void ImmutableTrieList()
    {
      var list = ImmutableTrieList<int>.Empty;
      for(int i = 0; i < N; i++)
      {
        list = list.Insert(0, 0);
      }
    }

        [Benchmark]
    public void List()
    {
      var list = new List<int>();
      for(int i = 0; i < N; i++)
      {
        list.Insert(0, 0);
      }
    }
  }
}
