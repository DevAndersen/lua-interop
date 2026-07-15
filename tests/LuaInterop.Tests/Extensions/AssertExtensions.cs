namespace LuaInterop.Tests.Extensions;

public static class AssertExtensions
{
    extension(Assert)
    {
        /// <summary>
        /// Verifies that <paramref name="diagnostics"/> contains the expected <paramref name="diagnosticsIds"/> exactly (regardless of order).
        /// </summary>
        /// <param name="diagnostics"></param>
        /// <param name="diagnosticsIds"></param>
        public static void Diagnostics(ImmutableArray<Diagnostic> diagnostics, params string[] diagnosticsIds)
        {
            SortedSet<string> expectedIds = new SortedSet<string>(diagnostics.Select(x => x.Id));
            SortedSet<string> actualIds = new SortedSet<string>(diagnosticsIds);

            bool equal = expectedIds.SequenceEqual(actualIds);

            Assert.True(equal, $"Unexpected diagnostics pattern, expected [{string.Join(", ", expectedIds)}], got [{string.Join(", ", actualIds)}]");
        }
    }
}
