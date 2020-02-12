using System;
using BenchmarkDotNet.Running;
using EfEagerLoad.Benchmarks.Miscellaneous;

namespace EfEagerLoad.Benchmarks
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<IncludeFinderBenchmarks>();
        }
    }
}
