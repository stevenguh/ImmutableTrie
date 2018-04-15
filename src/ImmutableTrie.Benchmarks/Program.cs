using System;
using BenchmarkDotNet.Running;

namespace ImmutableTrie.Benchmarks
{
  public class Program
  {
    static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<GetSequential>();
        var summary2 = BenchmarkRunner.Run<GetReverse>();

    }
  }
}
