using System;
using System.Collections.Generic;
using System.Linq;
using EfEagerLoad.Common;

namespace EfEagerLoad.Engine
{
    public class IncludeFinder
    {
        private static readonly NavigationFinder CachedNavigationFinder = new NavigationFinder();

        private readonly NavigationFinder _navigationFinder;

        public IncludeFinder() : this(CachedNavigationFinder) { }

        public IncludeFinder(NavigationFinder navigationFinder)
        {
            _navigationFinder = navigationFinder;
        }

        public IList<ReadOnlyMemory<char>> BuildIncludePathsForRootType2(EagerLoadContext context)
        {
            foreach (var _ in BuildIncludesForEagerLoadContext()) { }

            return context.IncludePathsToInclude;

            IEnumerable<bool> BuildIncludesForEagerLoadContext()
            {
                var navigationsToConsider = _navigationFinder.GetNavigationsForType(context, context.CurrentType ?? context.RootType);
                foreach (var navigation in navigationsToConsider)
                {
                    context.SetCurrentNavigation(navigation);

                    if (context.IncludeStrategy.ShouldIncludeCurrentNavigation(context))
                    {
                        context.IncludePathsToInclude.Add(context.CurrentIncludePath);

                        foreach (var _ in BuildIncludesForEagerLoadContext()) { yield return default; }
                    }

                    context.RemoveCurrentNavigation();
                }
            }
        }

        public IList<ReadOnlyMemory<char>> BuildIncludePathsForRootType(EagerLoadContext context)
        {
            BuildIncludesForEagerLoadContext(context);
            return context.IncludePathsToInclude;
        }

        private void BuildIncludesForEagerLoadContext(EagerLoadContext context)
        {
            BuildIncludesForEagerLoadContext();

            void BuildIncludesForEagerLoadContext()
            {
                var navigationsToConsider = _navigationFinder.GetNavigationsForType(context, context.CurrentType ?? context.RootType);

                foreach (var navigation in navigationsToConsider)
                {
                    context.SetCurrentNavigation(navigation);

                    if (context.IncludeStrategy.ShouldIncludeCurrentNavigation(context))
                    {
                        context.IncludePathsToInclude.Add(context.CurrentIncludePath);
                        BuildIncludesForEagerLoadContext();
                    }

                    context.RemoveCurrentNavigation();
                }
            }
        }

    }
}
