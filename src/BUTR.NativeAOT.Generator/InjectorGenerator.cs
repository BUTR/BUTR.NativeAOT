using BUTR.NativeAOT.Analyzer.Shared;

using Microsoft.CodeAnalysis;

using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace BUTR.NativeAOT.Generator;

[Generator]
public class InjectorGenerator : ISourceGenerator
{
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

    private static string GetTypeName(ITypeSymbol type, ConstMetadata? constMetadataRaw = null)
    {
        var constMetadata = constMetadataRaw ?? ConstMetadata.Empty;

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

    private static string GetReturnType(IMethodSymbol methodSymbol, ConstMetadata? constMetadataOverride)
    {
        constMetadataOverride ??= Helper.TryGetMethodMetadata(methodSymbol, out var val) ? val : null;
        return $"{GetTypeName(methodSymbol.ReturnType, constMetadataOverride)}";
    }

    private static string GetFunctionPointerParameterType(IParameterSymbol parent, IFunctionPointerTypeSymbol functionPointerTypeSymbol, IParameterSymbol parameterSymbol, int idx)
    {
        var constMetadata = Helper.TryGetFunctionalPointerParameterMetadata(parent, functionPointerTypeSymbol, idx, out var val) ? val : null;
        return $"{GetTypeName(parameterSymbol.Type, constMetadata)}";
    }

    private static (string Type, string Name) GetFunctionPointerParameterType(IParameterSymbol parent, IFunctionPointerTypeSymbol functionPointerTypeSymbol)
    {
        var methodSymbol = functionPointerTypeSymbol.Signature;
        var callingConvention = GetCallingConvention(methodSymbol);
        var name = parent.MetadataName;
        var returnMetadata = Helper.TryGetFunctionalPointerParameterMetadata(parent, functionPointerTypeSymbol, methodSymbol.Parameters.Length, out var val) ? val : null;
        var returnType = GetReturnType(methodSymbol, returnMetadata);
        var parameters = string.Join(", ", methodSymbol.Parameters.Select((x, i) => GetFunctionPointerParameterType(parent, functionPointerTypeSymbol, x, i)));

        return ($"{returnType} ({callingConvention} {name})({parameters})", string.Empty);
    }

    private static (string Type, string Name) GetParameterType(IMethodSymbol parent, IParameterSymbol parameterSymbol)
    {
        if (parameterSymbol.Type is IFunctionPointerTypeSymbol functionPointerTypeSymbol)
            return GetFunctionPointerParameterType(parameterSymbol, functionPointerTypeSymbol);

        var constMetadata = Helper.TryGetParameterMetadata(parent, parameterSymbol, out var val) ? val : null;
        return ($"{GetTypeName(parameterSymbol.Type, constMetadata)}", parameterSymbol.MetadataName);
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
        foreach (var methodSymbol in receiver.Methods.Select(x => context.Compilation.GetSemanticModel(x.SyntaxTree).GetDeclaredSymbol(x)).OfType<IMethodSymbol>().OrderBy(x => x.Name))
        {
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