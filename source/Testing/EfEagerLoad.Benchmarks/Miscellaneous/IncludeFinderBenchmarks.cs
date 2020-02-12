using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using EfEagerLoad.Common;
using EfEagerLoad.Engine;
using EfEagerLoad.IncludeStrategies;
using EfEagerLoad.Testing.Data;
using EfEagerLoad.Testing.Model;
using Microsoft.EntityFrameworkCore;

namespace EfEagerLoad.Benchmarks.Miscellaneous
{
    [RankColumn]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [MemoryDiagnoser]
    public class IncludeFinderBenchmarks
    {
        private TestDbContext _testDbContext;
        private EagerLoadContext _context;
        private EagerLoadAttributeIncludeStrategy _strategy;
        private IncludeFinder _includeFinder;

        [Benchmark(Baseline = true)]
        public IList<ReadOnlyMemory<char>> Recurse_1()
        {
            _context.IncludePathsToInclude.Clear();
            return _includeFinder.BuildIncludePathsForRootType(_context);
        }

        [Benchmark]
        public IList<ReadOnlyMemory<char>> Generator_1()
        {
            _context.IncludePathsToInclude.Clear();
            return _includeFinder.BuildIncludePathsForRootType2(_context);
        }



        [GlobalSetup]
        public void GlobalSetup()
        {
            _testDbContext = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

            _strategy = new EagerLoadAttributeIncludeStrategy();

            _context = new EagerLoadContext(_testDbContext, _strategy, new string[0], IncludeExecution.NoCache, typeof(Book));

            _includeFinder = new IncludeFinder();
        }
    }
}