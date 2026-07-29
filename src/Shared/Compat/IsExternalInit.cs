#if !NET6_0_OR_GREATER

// netstandard2.1 does not define this type, and without it the compiler refuses every
// `record`, `record struct`, and `init` accessor in the core. Modern targets use the
// BCL type instead; this copy exists only so Unity can compile against the same source.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit
    {
    }
}

#endif
