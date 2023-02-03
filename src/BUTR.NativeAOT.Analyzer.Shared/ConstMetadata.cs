using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BUTR.NativeAOT.Analyzer.Shared;

public record ConstMetadata(AttributeData AttributeData, bool IsConst, bool IsPointingToConst)
{
    public static readonly ConstMetadata Empty = new(default, false, false);
}