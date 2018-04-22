using System;
using BenchmarkDotNet.Running;

namespace ImmutableTrie.Benchmarks
{
  public class Program
  {
    static void Main(string[] args)
    {
        var getSequential = BenchmarkRunner.Run<ListGetSequential>();
        var getReverse = BenchmarkRunner.Run<ListGetReverse>();
        var add = BenchmarkRunner.Run<ListAdd>();
        var insertAtZero = BenchmarkRunner.Run<ListInsertAtZero>();
        var getRange = BenchmarkRunner.Run<ListGetRange>();

    }
  }
}
