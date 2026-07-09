using Microsoft.CodeAnalysis;

namespace LuaInterop.Tests.Extensions;

internal static class SyntaxNodeExtensions
{
    extension(SyntaxNode node)
    {
        /// <summary>
        /// Returns the child nodes of <paramref name="node"/> an array, allowing direct use of pattern matching.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public SyntaxNode[] GetChildNodes()
        {
            return node.ChildNodes().ToArray();
        }
    }
}
