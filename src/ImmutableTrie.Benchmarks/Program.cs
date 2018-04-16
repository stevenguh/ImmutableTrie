using System;
using BenchmarkDotNet.Running;

namespace ImmutableTrie.Benchmarks
{
  public class Program
  {
    static void Main(string[] args)
    {
        var getSequential = BenchmarkRunner.Run<GetSequential>();
        var getReverse = BenchmarkRunner.Run<GetReverse>();
        var add = BenchmarkRunner.Run<Add>();
        var insertAtZero = BenchmarkRunner.Run<InsertAtZero>();
        var getRange = BenchmarkRunner.Run<GetRange>();

    }
  }
}
