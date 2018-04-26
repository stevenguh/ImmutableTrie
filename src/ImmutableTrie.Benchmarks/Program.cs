using System;
using BenchmarkDotNet.Running;

namespace ImmutableTrie.Benchmarks
{
  public class Program
  {
    static void Main(string[] args)
    {
      var listGetSequential = BenchmarkRunner.Run<ListGetSequential>();
      var listGetReverse = BenchmarkRunner.Run<ListGetReverse>();
      var listAdd = BenchmarkRunner.Run<ListAdd>();
      var listInsertAtZero = BenchmarkRunner.Run<ListInsertAtZero>();
      var listGetRange = BenchmarkRunner.Run<ListGetRange>();

      var popCount = BenchmarkRunner.Run<PopCount>();

      var dictionaryAdd = BenchmarkRunner.Run<DictionaryAdd>();
      var DictionarySetExisting = BenchmarkRunner.Run<DictionarySetExisting>();
      var dictionaryRemove = BenchmarkRunner.Run<DictionaryRemove>();

      var dictionaryBuilderAdd = BenchmarkRunner.Run<DictionaryBuilderAdd>();
      var DictionaryBuilderSetExisting = BenchmarkRunner.Run<DictionaryBuilderSetExisting>();
    }
  }
}
