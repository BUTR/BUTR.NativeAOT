using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.CodeAnalysis;

namespace BUTR.NativeAOT.Generator;

[Generator]
public class InjectorGenerator : ISourceGenerator
{
    private record ConstMetadata(bool IsConst, bool IsPointingToConst);
    private record ConstFlags(bool IsPointingToConst, object? Ignore);

    private static bool CompareAttributeName(ISymbol typeSymbol, string expected) => typeSymbol.MetadataName.Replace("Attribute", string.Empty) == expected;
    
    private static string GetTypeName(ITypeSymbol type, ConstMetadata? constMetadataRaw = null)
    {
        var constMetadata = constMetadataRaw ?? new ConstMetadata(false, false);
        
        if (type is IPointerTypeSymbol pointerTypeSymbol)
            return $"{(constMetadata.IsConst ? "const " : string.Empty)}{GetTypeName(pointerTypeSymbol.PointedAtType)}*{(constMetadata.IsPointingToConst ? " const" : string.Empty)}";
        
        switch (type.SpecialType)
        {
            case SpecialType.System_Boolean:
                return "uint8_t";
            case SpecialType.System_SByte:
                return "int8_t";
            case SpecialType.System_Int16:
                return "int16_t";
            case SpecialType.System_Int32:
                return "int32_t";
            case SpecialType.System_Int64:
                return "int64_t";
            case SpecialType.System_Byte:
                return "uint8_t";
            case SpecialType.System_UInt16:
                return "uint16_t";
            case SpecialType.System_UInt32:
                return "uint32_t";
            case SpecialType.System_UInt64:
                return "uint64_t";
            case SpecialType.System_Single:
                return "float";
            case SpecialType.System_Double:
                return "double";
            case SpecialType.System_Char:
                return "char16_t";
            case SpecialType.System_IntPtr:
            case SpecialType.System_UIntPtr:
                return "size_t";
            case SpecialType.System_Void:
                return "void";
        }
        
        return type.MetadataName;
    }

    private static ConstFlags GetConstRootMetadata(INamedTypeSymbol typeSymbol)
    {
        if (CompareAttributeName(typeSymbol, "IsPtrConst"))
        {
            return new ConstFlags(true, null);
        }

        return new ConstFlags(false, null);
    }

    private static ConstMetadata IsConst(AttributeData? constAttribute, IFunctionPointerTypeSymbol functionPointerTypeSymbol, int idx)
    {
        if (constAttribute?.AttributeClass is null) return new ConstMetadata(false, false);
        
        if (constAttribute.AttributeClass?.TypeArguments[idx] is INamedTypeSymbol root)
        {
            if (CompareAttributeName(root, "IsConst"))
            {
                return new ConstMetadata(true, false);
            }
            if (CompareAttributeName(root, "IsNotConst"))
            {
                return new ConstMetadata(false, false);
            }
            if (CompareAttributeName(root, "IsConst`1") && root.TypeArguments[0] is INamedTypeSymbol innerConstFlag)
            {
                var (isPtrConst, _) = GetConstRootMetadata(innerConstFlag);
                return new ConstMetadata(true, isPtrConst);
            }
            if (CompareAttributeName(root, "IsNotConst`1") && root.TypeArguments[0] is INamedTypeSymbol innerNotConstFlag)
            {
                var (isPtrConst, _) = GetConstRootMetadata(innerNotConstFlag);
                return new ConstMetadata(false, isPtrConst);
            }
        }
        
        return new ConstMetadata(false, false);
    }
    private static ConstMetadata IsConst(AttributeData? parentConstAttribute, IParameterSymbol parameter)
    {
        var parameterConstAttribute = parameter.GetAttributes().FirstOrDefault(x => x.AttributeClass is not null && CompareAttributeName(x.AttributeClass, "IsConst"));

        var constAttribute = parameterConstAttribute ?? parentConstAttribute;
        if (constAttribute?.AttributeClass is null) return new ConstMetadata(false, false);

        if (constAttribute.AttributeClass.Arity == 1)
        {
            var root = constAttribute.AttributeClass;
            if (root is not null && CompareAttributeName(root, "IsConst`1") && root.TypeArguments[0] is INamedTypeSymbol innerConstFlag)
            {
                var (isPtrConst, _) = GetConstRootMetadata(innerConstFlag);
                return new ConstMetadata(true, isPtrConst);
            }
            if (root is not null && CompareAttributeName(root, "IsNotConst`1") && root.TypeArguments[0] is INamedTypeSymbol innerNotConstFlag)
            {
                var (isPtrConst, _) = GetConstRootMetadata(innerNotConstFlag);
                return new ConstMetadata(false, isPtrConst);
            }
        }
        
        if (constAttribute.NamedArguments.FirstOrDefault(x => x.Key == "PointsToConstant") is { Value: { IsNull: false, Value: bool pointsToConstVal } })
        {
            return new ConstMetadata(true, pointsToConstVal);
        }
        
        return new ConstMetadata(true, false);
    }

    private static string GetFunctionPointerParameterType(AttributeData? constAttribute, IFunctionPointerTypeSymbol functionPointerTypeSymbol, IParameterSymbol parameterSymbol, int idx)
    {
        var constMetadata = IsConst(constAttribute, functionPointerTypeSymbol, idx);
        return $"{GetTypeName(parameterSymbol.Type, constMetadata)}";
    }
    private static (string Type, string Name) GetFunctionPointerParameterType(IParameterSymbol parent, IFunctionPointerTypeSymbol functionPointerTypeSymbol)
    {
        var count = functionPointerTypeSymbol.Signature.Parameters.Length + 1;
        var constAttribute = parent.GetAttributes().FirstOrDefault(x => x.AttributeClass is not null && CompareAttributeName(x.AttributeClass, $"ConstMeta`{count}"));
        if (constAttribute?.AttributeClass?.Arity != count) constAttribute = null;

        var methodSymbol = functionPointerTypeSymbol.Signature;
        var callingConvention = GetCallingConvention(methodSymbol);
        var name = parent.MetadataName;
        var returnMetadata = IsConst(constAttribute, functionPointerTypeSymbol, methodSymbol.Parameters.Length);
        var returnType = GetReturnType(methodSymbol, returnMetadata);
        var parameters = string.Join(", ", methodSymbol.Parameters.Select((x, i) => GetFunctionPointerParameterType(constAttribute, functionPointerTypeSymbol, x, i)));
    
        return ($"{returnType} ({callingConvention} {name})({parameters})", string.Empty);
    }
    private static (string Type, string Name) GetParameterType(IMethodSymbol parent, IParameterSymbol parameterSymbol)
    {
        if (parameterSymbol.Type is IFunctionPointerTypeSymbol functionPointerTypeSymbol)
            return GetFunctionPointerParameterType(parameterSymbol, functionPointerTypeSymbol);

        var parentConstAttribute = parent.GetAttributes().FirstOrDefault(x => x.AttributeClass is not null && CompareAttributeName(x.AttributeClass, "IsConst"));
        var constMetadata = IsConst(parentConstAttribute, parameterSymbol);
        return ($"{GetTypeName(parameterSymbol.Type, constMetadata)}", parameterSymbol.MetadataName);
    }
    
    private static string GetReturnType(IMethodSymbol methodSymbol, ConstMetadata? constMetadataOverride)
    {
        if (constMetadataOverride is not null)
            return $"{GetTypeName(methodSymbol.ReturnType, constMetadataOverride)}";

        var constMethodAttribute = methodSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass is not null && CompareAttributeName(x.AttributeClass, "IsConst"));
        var constReturnTypeAttribute = methodSymbol.ReturnType.GetAttributes().FirstOrDefault(x => x.AttributeClass?.MetadataName.StartsWith("IsConst") == true);
        var constAttribute = constReturnTypeAttribute ?? constMethodAttribute;

        var constMethodGenericAttribute = methodSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass is not null && CompareAttributeName(x.AttributeClass, "IsConst`1"));
        var constReturnTypeGenericAttribute = methodSymbol.ReturnType.GetAttributes().FirstOrDefault(x => x.AttributeClass?.MetadataName.StartsWith("IsConst`1") == true);
        var constGenericAttribute = constReturnTypeGenericAttribute ?? constMethodGenericAttribute;

        var notConstMethodGenericAttribute = methodSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass is not null && CompareAttributeName(x.AttributeClass, "IsNotConst`1"));
        var notConstReturnTypeGenericAttribute = methodSymbol.ReturnType.GetAttributes().FirstOrDefault(x => x.AttributeClass?.MetadataName.StartsWith("IsNotConst`1") == true);
        var notConstGenericAttribute = notConstReturnTypeGenericAttribute ?? notConstMethodGenericAttribute;
        
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

        return $"{GetTypeName(methodSymbol.ReturnType, new ConstMetadata(hasConst, hasPointsToConst))}";
    }

    private static string GetCallingConvention(IMethodSymbol method)
    {
        switch (method.CallingConvention)
        {
            case SignatureCallingConvention.CDecl: return "__cdecl";
            case SignatureCallingConvention.StdCall: return "__stdcall";
            case SignatureCallingConvention.FastCall: return "__fastcall";
            case SignatureCallingConvention.ThisCall: return "__thiscall";
        }
        
        //if (method.UnmanagedCallingConventionTypes.Length > 0)
        //{
        //}
        
        var attr = method.GetAttributes().FirstOrDefault(x => x.AttributeClass?.MetadataName.StartsWith("UnmanagedCallersOnly", StringComparison.Ordinal) == true);
        if (attr?.NamedArguments.FirstOrDefault(x => x.Key == "CallConvs") is { Value.IsNull: false } callConvs && callConvs.Value.Values.FirstOrDefault().Value is INamedTypeSymbol typeSymbol)
        {
            switch (typeSymbol.MetadataName)
            {
                case nameof(CallConvCdecl): return "__cdecl";
                case nameof(CallConvStdcall): return "__stdcall";
                case nameof(CallConvFastcall): return "__fastcall";
                case nameof(CallConvThiscall): return "__thiscall";
                default: return typeSymbol.MetadataName;
            }
        }
            
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "__stdcall" : "__cdecl";
    }
    private static string GetMethodName(IMethodSymbol method)
    {
        var attr = method.GetAttributes().FirstOrDefault(x => x.AttributeClass?.MetadataName.StartsWith("UnmanagedCallersOnly", StringComparison.Ordinal) == true);
        if (attr?.NamedArguments.FirstOrDefault(x => x.Key == "EntryPoint") is { Value.IsNull: false, Value.Value: string entryPoint })
        {
            return entryPoint;
        }
            
        return method.MetadataName;
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        /*
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            System.Diagnostics.Debugger.Launch();
        }
        */
            
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SyntaxReceiver receiver || receiver.Methods.Count == 0)
            return;
        
        if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.rootnamespace", out var rootNamespace))
        {
            return;
        }
        if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.assemblyname", out var assemblyName))
        {
            return;
        }
        
        if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.projectdir", out var projectDir))
        {
            return;
        }

        var methods = new StringBuilder();
        foreach (var method in receiver.Methods)
        {
            var semanticModel = context.Compilation.GetSemanticModel(method.SyntaxTree);
            if (semanticModel.GetDeclaredSymbol(method) is not IMethodSymbol methodSymbol)
                continue;

            var callingConvention = GetCallingConvention(methodSymbol);
            var name = GetMethodName(methodSymbol);
            var returnType = GetReturnType(methodSymbol, null);
            var parameters = string.Join(", ", methodSymbol.Parameters.Select(x => GetParameterType(methodSymbol, x)).Select(param => $"{param.Type} {param.Name}"));
            methods.AppendLine($"    {returnType} {callingConvention} {name}({parameters});");
        }

        var cppNamespace = rootNamespace.Replace(".", "::");
        var final = $@"
#ifndef SRC_BINDINGS_H_
#define SRC_BINDINGS_H_

#ifndef __cplusplus
{HeaderStrings.CImportHeaders}
#else
{HeaderStrings.CppImportHeaders}
namespace {cppNamespace}
{{
    extern ""C""
    {{
#endif
{HeaderStrings.Types}
{methods}

#ifdef __cplusplus
    }}
}}
#endif

#endif
";

        var fullPath = Path.Combine(projectDir, $"{assemblyName}.h");
        File.WriteAllText(fullPath, final);
    }
}