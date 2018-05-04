using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Attributes.Exporters;
using BenchmarkDotNet.Attributes.Jobs;

namespace ImmutableTrie.Benchmarks
{
    [ClrJob(isBaseline: true), CoreJob, MonoJob]
    [RPlotExporter, RankColumn]
    public class PopCount
    {
        [Params(1000, 10000, 100000)]
        public int N;

        private int[] range;

        [GlobalSetup]
        public void Setup()
        {
            Random r = new Random();
            range = Enumerable.Range(0, N).Select(e => r.Next()).ToArray();
        }

        [Benchmark]
        public void PopCountWithNoMultiplication()
        {
            foreach (int bit in range)
            {
                var a = PopCountA(bit);
            }
        }

        [Benchmark]
        public void PopCountWithMultiplication()
        {
            foreach (int bit in range)
            {
                var b = PopCountB(bit);
            }
        }

        /// <summary>
        /// Popcount with 15 arithmetic operations. This is ideal for computer with slow multiplication.
        /// </summary>
        /// <param name="x">The number to get the pop count.</param>
        /// <returns>The pop count of <paramref name="x"/>.</returns>
        private static int PopCountA(int x)
        {
            x -= (x >> 1) & 0x55555555;
            x = (x & 0x33333333) + ((x >> 2) & 0x33333333);
            x = (x + (x >> 4)) & 0x0f0f0f0f;
            x += x >> 8;
            x += x >> 16;
            return x & 0x7f;
        }

        /// <summary>
        /// Popcount with 13 arithmetic operations, one of which is multiplication.
        /// </summary>
        /// <param name="x">The number to get the pop count.</param>
        /// <returns>The pop count of <paramref name="x"/>.</returns>
        private static int PopCountB(int x)
        {
            x = x - ((x >> 1) & 0x55555555);
            x = (x & 0x33333333) + ((x >> 2) & 0x33333333);
            return (((x + (x >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }
    }
}
