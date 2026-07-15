#pragma warning disable IDE0130 // Namespace does not match folder structure
#pragma warning disable IDE0290 // Use primary constructor
#pragma warning disable IDE0060 // Remove unused parameter

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Makes <c>init</c> working on .NET Standard 2.0.
    /// </summary>
    /// <remarks>
    /// This attribute is used by the compiler, it should not be access manually.
    /// </remarks>
    internal static class IsExternalInit
    {
    }

    /// <summary>
    /// Makes <c>required</c> work on .NET Standard 2.0.
    /// </summary>
    /// <remarks>
    /// This attribute is used by the compiler, it should not be access manually.
    /// </remarks>
    internal class RequiredMemberAttribute : Attribute
    {
    }

    /// <summary>
    /// Makes <c>required</c> work on .NET Standard 2.0.
    /// </summary>
    /// <remarks>
    /// This attribute is used by the compiler, it should not be access manually.
    /// </remarks>
    internal sealed class CompilerFeatureRequiredAttribute : Attribute
    {
        internal CompilerFeatureRequiredAttribute(string featureName)
        {
        }
    }
}

namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>
    /// Makes <c>[NotNull]</c> work on .NET Standard 2.0.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
    internal sealed class NotNullAttribute : Attribute
    {
    }

    /// <summary>
    /// Makes <c>[NotNullWhen]</c> work on .NET Standard 2.0.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class NotNullWhenAttribute : Attribute
    {
        public NotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

        public bool ReturnValue { get; }
    }
}

namespace System
{
    /// <summary>
    /// Makes index syntax work on .NET Standard 2.0.
    /// </summary>
    /// <remarks>
    /// Implementation copied from <seealso href="https://github.com/dotnet/dotnet/blob/main/src/runtime/src/libraries/System.Private.CoreLib/src/System/Index.cs"/>.
    /// </remarks>
    internal readonly struct Index
    {
        private readonly int _value;

        public Index(int value, bool fromEnd = false)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            if (fromEnd)
            {
                _value = ~value;
            }
            else
            {
                _value = value;
            }
        }

        public bool IsFromEnd => _value < 0;

        public int GetOffset(int length)
        {
            int offset = _value;
            if (IsFromEnd)
            {
                offset += length + 1;
            }
            return offset;
        }
    }

    /// <summary>
    /// Makes range syntax work on .NET Standard 2.0.
    /// </summary>
    internal readonly struct Range
    {
        public Index Start { get; }

        public Index End { get; }

        public Range(Index start, Index end)
        {
            Start = start;
            End = end;
        }
    }
}
