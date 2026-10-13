// netstandard2.1 (Unity's API profile) predates C# 9; this marker type enables records and init-only setters.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit
    {
    }
}
