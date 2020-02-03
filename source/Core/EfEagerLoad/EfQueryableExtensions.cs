﻿using System;
using System.Linq;
using EfEagerLoad.Common;
using EfEagerLoad.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EfEagerLoad
{
    public static class EfQueryableExtensions
    {
        public static IQueryable<TEntity> EagerLoad<TEntity>(this IQueryable<TEntity> originalQuery, DbContext dbContext) where TEntity : class
        {
            return originalQuery.EagerLoad(dbContext, true);
        }

        public static IQueryable<TEntity> EagerLoadMatchingAttribute<TEntity, TAttribute>(this IQueryable<TEntity> originalQuery, 
                                                                DbContext dbContext) where TEntity : class
                                                                                    where TAttribute : Attribute
        {
            return originalQuery.EagerLoadMatchingAttribute<TEntity, TAttribute>(dbContext, true);
        }

        public static IQueryable<TEntity> EagerLoadAll<TEntity>(this IQueryable<TEntity> originalQuery, DbContext dbContext) where TEntity : class
        {
            return originalQuery.EagerLoadAll(dbContext, true);
        }

        public static IQueryable<TEntity> EagerLoadMatching<TEntity>(this IQueryable<TEntity> originalQuery, DbContext dbContext,
                                        Func<EfEagerLoadContext, INavigation, bool> navigationMatchPredicate) where TEntity : class
        {
            return originalQuery.EagerLoadMatching(dbContext, true, navigationMatchPredicate);
        }

        public static IQueryable<TEntity> EagerLoad<TEntity>(this IQueryable<TEntity> originalQuery, DbContext dbContext, 
                                                            params string[] ignoredNavigationProperties) where TEntity : class
        {
            return originalQuery.EagerLoad(dbContext, true, ignoredNavigationProperties);
        }

        public static IQueryable<TEntity> EagerLoadMatchingAttribute<TEntity, TAttribute>(this IQueryable<TEntity> originalQuery, 
                                                DbContext dbContext, params string[] ignoredNavigationProperties) where TEntity : class
                                                                                                                where TAttribute : Attribute
        {
            return originalQuery.EagerLoadMatchingAttribute<TEntity, TAttribute>(dbContext, true, ignoredNavigationProperties);
        }

        public static IQueryable<TEntity> EagerLoadAll<TEntity>(this IQueryable<TEntity> originalQuery, DbContext dbContext,
            params string[] ignoredNavigationProperties) where TEntity : class
        {
            return originalQuery.EagerLoadAll(dbContext, true, ignoredNavigationProperties);
        }

        public static IQueryable<TEntity> EagerLoadMatching<TEntity>(this IQueryable<TEntity> originalQuery, DbContext dbContext,
                            Func<EfEagerLoadContext, INavigation, bool> shouldIncludeNavigationPredicate, params string[] ignoredNavigationProperties) 
                            where TEntity : class
        {
            return originalQuery.EagerLoadMatching(dbContext, true, shouldIncludeNavigationPredicate, ignoredNavigationProperties);
        }

        public static IQueryable<TEntity> EagerLoad<TEntity>(this IQueryable<TEntity> originalQuery, DbContext dbContext, bool eagerLoad,
                                                            params string[] navigationPropertiesToIgnore) where TEntity : class
        {
            var shouldIncludeNavigationPredicate = EagerLoadAttributeFunctions.PredicateForEagerLoadAttribute;
            return originalQuery.EagerLoadMatching(dbContext, eagerLoad, shouldIncludeNavigationPredicate, navigationPropertiesToIgnore);
        }

        public static IQueryable<TEntity> EagerLoadMatchingAttribute<TEntity, TAttribute>(this IQueryable<TEntity> originalQuery, 
                                                        DbContext dbContext, bool eagerLoad, params string[] navigationPropertiesToIgnore) 
                                                        where TEntity : class
                                                        where TAttribute : Attribute
        {
            return originalQuery.EagerLoadMatching(dbContext, eagerLoad, 
                                    (context, nav) => Attribute.IsDefined(nav.PropertyInfo, typeof(TAttribute)), navigationPropertiesToIgnore);
        }

        public static IQueryable<TEntity> EagerLoadAll<TEntity>(this IQueryable<TEntity> originalQuery, DbContext dbContext, bool eagerLoad, 
                                                                params string[] navigationPropertiesToIgnore) where TEntity : class
        {
            return originalQuery.EagerLoadMatching(dbContext, eagerLoad, (context, nav) => true, navigationPropertiesToIgnore);
        }



        public static IQueryable<TEntity> EagerLoadMatching<TEntity>(this IQueryable<TEntity> originalQuery, DbContext dbContext, bool eagerLoad,
                                Func<EfEagerLoadContext, INavigation, bool> shouldIncludeNavigationPredicate, params string[] navigationPropertiesToIgnore) 
                                where TEntity : class
        {
            if (!eagerLoad) { return originalQuery; }

            Guard.IsNotNull(nameof(originalQuery), originalQuery);

            var eagerLoadContext = new EfEagerLoadContext(typeof(TEntity), dbContext, (navigationPropertiesToIgnore ?? new string[0]).ToList(),
                                                            shouldIncludeNavigationPredicate);

            var includeFunction = EfQueryableInternalFunctionBuilder.GetIncludeFunction<TEntity>(eagerLoadContext);
            return includeFunction(originalQuery);
        }

    }
}
