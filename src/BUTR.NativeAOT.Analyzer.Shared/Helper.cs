using Microsoft.CodeAnalysis;

using System.Linq;

namespace BUTR.NativeAOT.Analyzer.Shared;

public class Helper
{
    public static bool CompareAttributeName(ISymbol? typeSymbol, string expected) => typeSymbol is not null && typeSymbol.MetadataName.Replace("Attribute", string.Empty) == expected;

    private static ConstFlags GetConstRootMetadata(INamedTypeSymbol typeSymbol)
    {
        if (CompareAttributeName(typeSymbol, "IsPtrConst"))
        {
            return new ConstFlags(true, null);
        }

        return new ConstFlags(false, null);
    }

    public static bool TryGetMethodMetadata(IMethodSymbol methodSymbol, out ConstMetadata constMetadata)
    {
        var constMethodAttribute = methodSymbol.GetAttributes().FirstOrDefault(x => CompareAttributeName(x.AttributeClass, "IsConst"));
        var constReturnTypeAttribute = methodSymbol.ReturnType.GetAttributes().FirstOrDefault(x => x.AttributeClass?.MetadataName.StartsWith("IsConst") == true);
        var constAttribute = constReturnTypeAttribute ?? constMethodAttribute;

        var constMethodGenericAttribute = methodSymbol.GetAttributes().FirstOrDefault(x => CompareAttributeName(x.AttributeClass, "IsConst`1"));
        var constReturnTypeGenericAttribute = methodSymbol.ReturnType.GetAttributes().FirstOrDefault(x => x.AttributeClass?.MetadataName.StartsWith("IsConst`1") == true);
        var constGenericAttribute = constReturnTypeGenericAttribute ?? constMethodGenericAttribute;

        var notConstMethodGenericAttribute = methodSymbol.GetAttributes().FirstOrDefault(x => CompareAttributeName(x.AttributeClass, "IsNotConst`1"));
        var notConstReturnTypeGenericAttribute = methodSymbol.ReturnType.GetAttributes().FirstOrDefault(x => x.AttributeClass?.MetadataName.StartsWith("IsNotConst`1") == true);
        var notConstGenericAttribute = notConstReturnTypeGenericAttribute ?? notConstMethodGenericAttribute;

        if (constAttribute is null && constGenericAttribute is null && notConstGenericAttribute is null)
        {
            constMetadata = null;
            return false;
        }

        var hasConst = (constAttribute is not null || constGenericAttribute is not null) && notConstGenericAttribute is null;
        var hasPointsToConst = false;
        if (constAttribute?.NamedArguments.FirstOrDefault(x => x.Key == "PointsToConstant") is { Value: { IsNull: false, Value: bool pointsToConstVal1 } })
        {
            hasPointsToConst = pointsToConstVal1;
        }
        if (constGenericAttribute?.AttributeClass?.TypeArguments[0] is INamedTypeSymbol innerConstFlag)
        {
            var (isPtrConst, _) = GetConstRootMetadata(innerConstFlag);
            hasPointsToConst = isPtrConst;
        }
        if (notConstGenericAttribute?.AttributeClass?.TypeArguments[0] is INamedTypeSymbol innerNonConstFlag)
        {
            var (isPtrConst, _) = GetConstRootMetadata(innerNonConstFlag);
            hasPointsToConst = isPtrConst;
        }

        constMetadata = new ConstMetadata(constAttribute ?? constGenericAttribute ?? notConstGenericAttribute!, hasConst, hasPointsToConst);
        return true;
    }

    public static bool TryGetFunctionalPointerParameterMetadata(IParameterSymbol parent, IFunctionPointerTypeSymbol functionPointerTypeSymbol, int idx, out ConstMetadata constMetadata)
    {
        var count = functionPointerTypeSymbol.Signature.Parameters.Length + 1;
        var constAttribute = parent.GetAttributes().FirstOrDefault(x => CompareAttributeName(x.AttributeClass, $"ConstMeta`{count}"));
        if (constAttribute?.AttributeClass?.Arity != count) constAttribute = null;

        if (constAttribute?.AttributeClass is not { } rootRoot)
        {
            constMetadata = null;
            return false;
        }

        if (idx < rootRoot.TypeArguments.Length && rootRoot.TypeArguments[idx] is INamedTypeSymbol root)
        {
            if (CompareAttributeName(root, "IsConst"))
            {
                constMetadata = new ConstMetadata(constAttribute, true, false);
                return true;
            }
            if (CompareAttributeName(root, "IsNotConst"))
            {
                constMetadata = new ConstMetadata(constAttribute, false, false);
                return true;
            }
            if (CompareAttributeName(root, "IsConst`1") && root.TypeArguments[0] is INamedTypeSymbol innerConstFlag)
            {
                var (isPtrConst, _) = GetConstRootMetadata(innerConstFlag);
                constMetadata = new ConstMetadata(constAttribute, true, isPtrConst);
                return true;
            }
            if (CompareAttributeName(root, "IsNotConst`1") && root.TypeArguments[0] is INamedTypeSymbol innerNotConstFlag)
            {
                var (isPtrConst, _) = GetConstRootMetadata(innerNotConstFlag);
                constMetadata = new ConstMetadata(constAttribute, false, isPtrConst);
                return true;
            }
        }

        constMetadata = new ConstMetadata(constAttribute, false, false);
        return true;
    }
    public static bool TryGetParameterMetadata(IMethodSymbol parent, IParameterSymbol parameter, out ConstMetadata constMetadata)
    {
        var parentConstAttribute = parent.GetAttributes().FirstOrDefault(x => CompareAttributeName(x.AttributeClass, "IsConst"));
        var parameterConstAttribute = parameter.GetAttributes().FirstOrDefault(x => CompareAttributeName(x.AttributeClass, "IsConst") ||
                                                                                    CompareAttributeName(x.AttributeClass, "IsNotConst") ||
                                                                                    CompareAttributeName(x.AttributeClass, "IsConst`1") ||
                                                                                    CompareAttributeName(x.AttributeClass, "IsNotConst`1"));
        var constAttribute = parameterConstAttribute ?? parentConstAttribute;

        if (constAttribute?.AttributeClass is not { } root)
        {
            constMetadata = null;
            return false;
        }

        if (root.Arity == 1 && root.TypeArguments[0] is INamedTypeSymbol flag)
        {
            if (CompareAttributeName(root, "IsConst`1"))
            {
                var (isPtrConst, _) = GetConstRootMetadata(flag);
                constMetadata = new ConstMetadata(constAttribute, true, isPtrConst);
                return true;
            }
            if (CompareAttributeName(root, "IsNotConst`1"))
            {
                var (isPtrConst, _) = GetConstRootMetadata(flag);
                constMetadata = new ConstMetadata(constAttribute, false, isPtrConst);
                return true;
            }
        }

        if (constAttribute.NamedArguments.FirstOrDefault(x => x.Key == "PointsToConstant") is { Value: { IsNull: false, Value: bool pointsToConstVal } })
        {
            constMetadata = new ConstMetadata(constAttribute, true, pointsToConstVal);
            return true;
        }

        constMetadata = new ConstMetadata(constAttribute, true, false);
        return true;
    }
}