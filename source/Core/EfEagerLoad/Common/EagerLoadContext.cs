﻿using System;
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

        private readonly Stack<INavigation> _navigationPath = new Stack<INavigation>();
        private char[] _currentIncludePath = string.Empty.ToCharArray();

        public EagerLoadContext(DbContext dbContext, IIncludeStrategy includeStrategy, IList<string> includePathsToIgnore = null,
                                IncludeExecution includeExecution = IncludeExecution.Cached, Type rooType = null)
        {
            Guard.IsNotNull(nameof(dbContext), dbContext);
            Guard.IsNotNull(nameof(includeStrategy), includeStrategy);

            RootType = rooType;
            DbContext = dbContext;
            IncludePathsToIgnore =  new List<string>(includePathsToIgnore ?? new string[0]);
            IncludeStrategy = includeStrategy;
            IncludeExecution = includeExecution;
        }

        public Type RootType { get; internal set; }

        public INavigation CurrentNavigation => (_navigationPath.Any()) ? _navigationPath.Peek() : null;

        public string CurrentIncludePath => new string(_currentIncludePath);

        public ReadOnlySpan<char> CurrentIncludePathSpan => _currentIncludePath;

        public string ParentIncludePath => CurrentIncludePathSpan.GetParentIncludePathSpan().ToString();

        public ReadOnlySpan<char> ParentIncludePathSpan => CurrentIncludePathSpan.GetParentIncludePathSpan();


        public IEnumerable<INavigation> NavigationPath => _navigationPath;

        public DbContext DbContext { get; }

        public IList<string> IncludePathsToIgnore { get; }

        public IList<Type> TypesVisited => new List<Type>();

        public IIncludeStrategy IncludeStrategy { get; set; }

        public IncludeExecution IncludeExecution { get; }

        public IList<string> IncludePathsToInclude { get; internal set; } = new List<string>();

        public IServiceProvider ServiceProvider
        {
            get => ThreadLocalServiceProvider.Value;
            set => ThreadLocalServiceProvider.Value = value;
        }

        internal void SetCurrentNavigation(INavigation navigation)
        {
            if(navigation == null) { return; }

            _navigationPath.Push(navigation);
            _currentIncludePath = (NavigationPath.Skip(1).Any()) ? $"{CurrentIncludePath}.{CurrentNavigation.Name}".ToCharArray() :
                                                                    CurrentNavigation.Name.ToCharArray();
        }
        
        internal void RemoveCurrentNavigation()
        {
            if (_navigationPath.Count == 0) { return; }

            _navigationPath.Pop();
            _currentIncludePath = (!NavigationPath.Skip(1).Any()) ? 
                                    ((CurrentNavigation?.Name != null) ? CurrentNavigation?.Name.ToCharArray() : Array.Empty<char>()) :
                                    ParentIncludePathSpan.ToArray();
        }

        public static void InitializeServiceProvider(IServiceProvider serviceProvider)
        {
            ThreadLocalServiceProvider.Value = serviceProvider;
        }

    }
}
