﻿using System;
using EfEagerLoad.Engine;

namespace EfEagerLoad.IncludeStrategies
{
    public abstract class IncludeStrategy : IIncludeStrategy
    {
        public abstract bool ShouldIncludeNavigation(EagerLoadContext context);
        public bool ShouldIgnoreNavigationPath(EagerLoadContext context, string path)
        {
            return context.NavigationPathsToIgnore.Contains(path);
        }

        public void PreBuildExecute(EagerLoadContext context)
        {
        }
    }
}