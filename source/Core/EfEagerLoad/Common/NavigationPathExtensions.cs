using System;

namespace EfEagerLoad.Common
{
    public static class NavigationPathExtensions
    {
        private static readonly char SeparatorCharacter = char.Parse(".");

        public static ReadOnlyMemory<char> GetParentIncludePathSpan(this ReadOnlyMemory<char> includePath)
        {
            var includePathSpan = includePath.Span;
            if (!includePathSpan.Contains(SeparatorCharacter)) { return string.Empty.ToCharArray(); }

            return includePath.Slice(0, includePathSpan.LastIndexOf("."));
        }
    }
}
