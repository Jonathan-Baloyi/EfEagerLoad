using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EfEagerLoad.Engine;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EfEagerLoad.Common
{
    public class EagerLoadContext
    {
        private static readonly AsyncLocal<IServiceProvider> ThreadLocalServiceProvider = new AsyncLocal<IServiceProvider>();
        private static readonly char SeparatorCharacter = char.Parse(".");
        private readonly Stack<INavigation> _navigationPath = new Stack<INavigation>();

        public EagerLoadContext(DbContext dbContext, IIncludeStrategy includeStrategy, IList<string> includePathsToIgnore = null,
                                IncludeExecution includeExecution = IncludeExecution.Cached, Type rootType = null)
        {
            Guard.IsNotNull(nameof(dbContext), dbContext);
            Guard.IsNotNull(nameof(includeStrategy), includeStrategy);

            RootType = rootType;
            DbContext = dbContext;
            IncludePathsToIgnore =  new List<string>(includePathsToIgnore ?? new string[0]);
            IncludeStrategy = includeStrategy;
            IncludeExecution = includeExecution;
        }

        public Type RootType { get; internal set; }

        public Type CurrentType => CurrentNavigation?.GetNavigationType();

        public INavigation CurrentNavigation => (_navigationPath.Any()) ? _navigationPath.Peek() : null;

        public ReadOnlyMemory<char> CurrentIncludePath { get; internal set; } = new Memory<char>();

        public IEnumerable<INavigation> NavigationPath => _navigationPath;

        public DbContext DbContext { get; }

        public IList<string> IncludePathsToIgnore { get; }
        
        public IIncludeStrategy IncludeStrategy { get; set; }

        public IncludeExecution IncludeExecution { get; }

        public IList<ReadOnlyMemory<char>> IncludePathsToInclude { get; internal set; } = new List<ReadOnlyMemory<char>>();

        public IDictionary<string, object> Bag = new Dictionary<string, object>(5);

        public IServiceProvider ServiceProvider
        {
            get => ThreadLocalServiceProvider.Value;
            set => InitializeServiceProvider(value);
        }

        internal void SetCurrentNavigation(INavigation navigation)
        {
            if(navigation == null) { return; }

            _navigationPath.Push(navigation);
            CurrentIncludePath = (NavigationPath.Skip(1).Any()) ?
                                BuildPathForNavigation() :
                                //$"{CurrentIncludePath}.{CurrentNavigation.Name}".AsMemory() :
                                CurrentNavigation.Name.AsMemory();
        }

        public ReadOnlyMemory<char> BuildPathForNavigation()
        {
            var chars = new char[CurrentIncludePath.Length + 1 + CurrentNavigation.Name.Length];
            CurrentIncludePath.CopyTo(chars);
            chars[CurrentIncludePath.Length] = SeparatorCharacter;
            CurrentNavigation.Name.AsSpan().CopyTo(chars.AsSpan().Slice(CurrentIncludePath.Length + 1));
            return new ReadOnlyMemory<char>(chars);

            //string.Create(CurrentIncludePath.Length + 1 + CurrentNavigation.Name.Length, this,
            //    (chars, state) =>
            //    {
            //        state.CurrentIncludePath.Span.CopyTo(chars);
            //        chars[state.CurrentIncludePath.Length] = SeparatorCharacter;
            //        state.CurrentNavigation.Name.AsSpan().CopyTo(chars.Slice(state.CurrentIncludePath.Length + 1));
            //    }).AsMemory() :
        }
        
        internal void RemoveCurrentNavigation()
        {
            if (_navigationPath.Count == 0) { return; }

            _navigationPath.Pop();
            CurrentIncludePath = (NavigationPath.Skip(1).Any()) ?
                                    CurrentIncludePath.GetParentIncludePathSpan() :
                                    CurrentNavigation?.Name?.AsMemory() ?? new Memory<char>();
                                    
        }

        public static void InitializeServiceProvider(IServiceProvider serviceProvider)
        {
            ThreadLocalServiceProvider.Value = serviceProvider;
        }

    }
}
