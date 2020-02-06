﻿using System;

namespace EfEagerLoad.IncludeStrategy
{
    public class AttributeExistsIncludeStrategy<TAttribute> : IncludeStrategy
    {
        public override bool ShouldIncludeNavigation(EagerLoadContext context)
        {
            return (context.CurrentNavigation?.PropertyInfo != null) && Attribute.IsDefined(context.CurrentNavigation.PropertyInfo, typeof(TAttribute));
        }
    }
}
