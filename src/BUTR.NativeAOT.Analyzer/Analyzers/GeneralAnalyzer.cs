using BUTR.NativeAOT.Analyzer.Data;
using BUTR.NativeAOT.Analyzer.Shared;
using BUTR.NativeAOT.Analyzer.Utils;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using System.Collections.Immutable;
using System.Linq;

namespace BUTR.NativeAOT.Analyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class GeneralAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            RuleIdentifiers.UnnecessaryIsConstRule,
            RuleIdentifiers.UnnecessaryIsPtrConstRule,
            RuleIdentifiers.RequiredIsConstRule,
            RuleIdentifiers.RequiredIsPtrConstRule,
            RuleIdentifiers.RequiredConstMetaRule
        );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(Action, SyntaxKind.MethodDeclaration);
        }

        private static void Action(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not MethodDeclarationSyntax methodDeclarationSyntax) return;

            if (context.ContainingSymbol is not IMethodSymbol methodSymbol) return;

            var hasUnmanagedCallersOnly = methodSymbol.GetAttributes().Any(x => Helper.CompareAttributeName(x.AttributeClass, "UnmanagedCallersOnly"));
            if (!hasUnmanagedCallersOnly) return;


            if (Helper.TryGetMethodMetadata(methodSymbol, out var methodConstMetadata))
            {
                CheckReturnType(context, methodSymbol, methodConstMetadata);
            }
            else
            {
                CheckReturnTypeNotExist(context, methodSymbol, methodDeclarationSyntax);
            }

            for (var i = 0; i < methodDeclarationSyntax.ParameterList.Parameters.Count; i++)
            {
                var parameterSyntax = methodDeclarationSyntax.ParameterList.Parameters[i];
                if (context.SemanticModel.GetDeclaredSymbol(parameterSyntax) is not { } parameterSymbol) continue;

                if (parameterSymbol.Type is IFunctionPointerTypeSymbol functionPointerTypeSymbol)
                {
                    CheckFunctionPointerParameter(context, methodSymbol, parameterSymbol, functionPointerTypeSymbol, parameterSyntax.Type as FunctionPointerTypeSyntax);
                }
                else
                {
                    if (Helper.TryGetParameterMetadata(methodSymbol, parameterSymbol, out var parameterConstMetadata))
                    {
                        CheckParameter(context, parameterSymbol, parameterConstMetadata);
                    }
                    else
                    {
                        CheckParameterNotExist(context, parameterSymbol, parameterSyntax);
                    }
                }
            }
        }

        private static void CheckReturnType(SyntaxNodeAnalysisContext context, IMethodSymbol methodSymbol, ConstMetadata constMetadata)
        {
            if (methodSymbol.ReturnType is not IPointerTypeSymbol && constMetadata.IsPointingToConst)
            {
                if (constMetadata.AttributeData.ApplicationSyntaxReference!.GetSyntax() is not AttributeSyntax nodeRoot) return;
                if (nodeRoot.Name is not GenericNameSyntax nodeRootName || nodeRootName.TypeArgumentList.Arguments[0] is not { } nodePtr) return;
                var ctx = new GenericContext(context.Compilation, () => nodePtr.GetLocation(), context.ReportDiagnostic);
                context.ReportDiagnostic(RuleIdentifiers.ReportUnnecessaryIsPtrConst(ctx, NameFormatter.ReflectionName(methodSymbol.ReturnType)));
            }
            if (methodSymbol.ReturnType is not IPointerTypeSymbol && constMetadata.IsConst)
            {
                if (constMetadata.AttributeData.ApplicationSyntaxReference!.GetSyntax() is not AttributeSyntax nodeRoot) return;
                var ctx = new GenericContext(context.Compilation, () => nodeRoot.GetLocation(), context.ReportDiagnostic);
                context.ReportDiagnostic(RuleIdentifiers.ReportUnnecessaryIsConst(ctx, NameFormatter.ReflectionName(methodSymbol.ReturnType)));
            }
        }
        private static void CheckReturnTypeNotExist(SyntaxNodeAnalysisContext context, IMethodSymbol methodSymbol, MethodDeclarationSyntax methodDeclarationSyntax)
        {
            if (methodSymbol.ReturnType is IPointerTypeSymbol)
            {
                var ctx = new GenericContext(context.Compilation, () => methodDeclarationSyntax.ReturnType.GetLocation(), context.ReportDiagnostic);
                context.ReportDiagnostic(RuleIdentifiers.ReportRequiredIsPtrConstRule(ctx, NameFormatter.ReflectionName(methodSymbol.ReturnType)));
            }
            if (methodSymbol.ReturnType is IPointerTypeSymbol)
            {
                var ctx = new GenericContext(context.Compilation, () => methodDeclarationSyntax.ReturnType.GetLocation(), context.ReportDiagnostic);
                context.ReportDiagnostic(RuleIdentifiers.ReportRequiredIsConstRule(ctx, NameFormatter.ReflectionName(methodSymbol.ReturnType)));
            }
        }

        private static void CheckParameter(SyntaxNodeAnalysisContext context, IParameterSymbol parameterSymbol, ConstMetadata constMetadata)
        {
            if (parameterSymbol.Type is not IPointerTypeSymbol && constMetadata.IsPointingToConst)
            {
                if (constMetadata.AttributeData.ApplicationSyntaxReference!.GetSyntax() is not AttributeSyntax nodeRoot) return;
                if (nodeRoot.Name is not GenericNameSyntax nodeRootName || nodeRootName.TypeArgumentList.Arguments[0] is not { } nodePtr) return;
                var ctx = new GenericContext(context.Compilation, () => nodePtr.GetLocation(), context.ReportDiagnostic);
                context.ReportDiagnostic(RuleIdentifiers.ReportUnnecessaryIsPtrConst(ctx, NameFormatter.ReflectionName(parameterSymbol.Type)));
            }
            if (parameterSymbol.Type is not IPointerTypeSymbol && constMetadata.IsConst)
            {
                if (constMetadata.AttributeData.ApplicationSyntaxReference!.GetSyntax() is not AttributeSyntax nodeRoot) return;
                var ctx = new GenericContext(context.Compilation, () => nodeRoot.GetLocation(), context.ReportDiagnostic);
                context.ReportDiagnostic(RuleIdentifiers.ReportRequiredIsPtrConstRule(ctx, NameFormatter.ReflectionName(parameterSymbol.Type)));
            }
        }
        private static void CheckParameterNotExist(SyntaxNodeAnalysisContext context, IParameterSymbol parameterSymbol, ParameterSyntax parameterSyntax)
        {
            if (parameterSymbol.Type is IPointerTypeSymbol)
            {
                var ctx = new GenericContext(context.Compilation, () => parameterSyntax.GetLocation(), context.ReportDiagnostic);
                context.ReportDiagnostic(RuleIdentifiers.ReportUnnecessaryIsPtrConst(ctx, NameFormatter.ReflectionName(parameterSymbol.Type)));
            }
            if (parameterSymbol.Type is IPointerTypeSymbol)
            {
                var ctx = new GenericContext(context.Compilation, () => parameterSyntax.GetLocation(), context.ReportDiagnostic);
                context.ReportDiagnostic(RuleIdentifiers.ReportRequiredIsConstRule(ctx, NameFormatter.ReflectionName(parameterSymbol.Type)));
            }
        }

        private static void CheckFunctionPointerParameter(SyntaxNodeAnalysisContext context, IMethodSymbol methodSymbol, IParameterSymbol parameterSymbol, IFunctionPointerTypeSymbol functionPointerTypeSymbol, FunctionPointerTypeSyntax functionPointerTypeSyntax)
        {
            var functionalPointerParameterReturnMetadata = Helper.TryGetFunctionalPointerParameterMetadata(parameterSymbol, functionPointerTypeSymbol, methodSymbol.Parameters.Length, out var val) ? val : ConstMetadata.Empty;
            var functionalPointerParameterParameters = methodSymbol.Parameters.Select((x, i) => Helper.TryGetFunctionalPointerParameterMetadata(x, functionPointerTypeSymbol, i, out var val) ? val : ConstMetadata.Empty).ToImmutableArray();

            if (functionalPointerParameterReturnMetadata.AttributeData is null)
            {
                var ctx = new GenericContext(context.Compilation, () => functionPointerTypeSyntax.GetLocation(), context.ReportDiagnostic);
                context.ReportDiagnostic(RuleIdentifiers.ReportRequiredConstMetaRule(ctx, NameFormatter.ReflectionName(functionPointerTypeSymbol)));
                return;
            }

            if (functionPointerTypeSymbol.Signature.ReturnType is not IPointerTypeSymbol && functionalPointerParameterReturnMetadata.IsPointingToConst)
            {
                if (functionalPointerParameterReturnMetadata.AttributeData.ApplicationSyntaxReference!.GetSyntax() is not AttributeSyntax nodeRootRoot) return;
                if (nodeRootRoot.Name is not GenericNameSyntax nodeRootRootName || nodeRootRootName.TypeArgumentList.Arguments.Last() is not { } nodeRoot) return;
                if (nodeRoot is not GenericNameSyntax nodeRootName || nodeRootName.TypeArgumentList.Arguments.Last() is not { } nodePtr) return;
                var ctx = new GenericContext(context.Compilation, () => nodePtr.GetLocation(), context.ReportDiagnostic);
                context.ReportDiagnostic(RuleIdentifiers.ReportUnnecessaryIsPtrConst(ctx, NameFormatter.ReflectionName(functionPointerTypeSymbol.Signature.ReturnType)));
            }
            if (functionPointerTypeSymbol.Signature.ReturnType is not IPointerTypeSymbol && functionalPointerParameterReturnMetadata.IsConst)
            {
                if (functionalPointerParameterReturnMetadata.AttributeData.ApplicationSyntaxReference!.GetSyntax() is not AttributeSyntax nodeRootRoot) return;
                if (nodeRootRoot.Name is not GenericNameSyntax nodeRootRootName || nodeRootRootName.TypeArgumentList.Arguments.Last() is not { } nodeRoot) return;
                var ctx = new GenericContext(context.Compilation, () => nodeRoot.GetLocation(), context.ReportDiagnostic);
                context.ReportDiagnostic(RuleIdentifiers.ReportUnnecessaryIsConst(ctx, NameFormatter.ReflectionName(functionPointerTypeSymbol.Signature.ReturnType)));
            }
            if (functionPointerTypeSymbol.Signature.ReturnType is IPointerTypeSymbol && !functionalPointerParameterReturnMetadata.IsPointingToConst)
            {
                var ctx = new GenericContext(context.Compilation, () => functionPointerTypeSyntax.ParameterList.Parameters.Last().GetLocation(), context.ReportDiagnostic);
                context.ReportDiagnostic(RuleIdentifiers.ReportRequiredIsPtrConstRule(ctx, NameFormatter.ReflectionName(functionPointerTypeSymbol.Signature.ReturnType)));
            }
            if (functionPointerTypeSymbol.Signature.ReturnType is IPointerTypeSymbol && !functionalPointerParameterReturnMetadata.IsConst)
            {
                var ctx = new GenericContext(context.Compilation, () => functionPointerTypeSyntax.ParameterList.Parameters.Last().GetLocation(), context.ReportDiagnostic);
                context.ReportDiagnostic(RuleIdentifiers.ReportRequiredIsConstRule(ctx, NameFormatter.ReflectionName(functionPointerTypeSymbol.Signature.ReturnType)));
            }

            for (var i = 0; i < functionalPointerParameterParameters.Length; i++)
            {
                var functionPointerParameterParameterMetadata = functionalPointerParameterParameters[i];
                var functionPointerParameterSymbol = functionPointerTypeSymbol.Signature.Parameters[i];
                var functionPointerParameterSyntax = functionPointerTypeSyntax.ParameterList.Parameters[i];

                if (functionPointerParameterSymbol.Type is not IPointerTypeSymbol && functionPointerParameterParameterMetadata.IsPointingToConst)
                {
                    if (functionalPointerParameterReturnMetadata.AttributeData.ApplicationSyntaxReference!.GetSyntax() is not AttributeSyntax nodeRootRoot) continue;
                    if (nodeRootRoot.Name is not GenericNameSyntax nodeRootRootName || nodeRootRootName.TypeArgumentList.Arguments[i] is not { } nodeRoot) continue;
                    if (nodeRoot is not GenericNameSyntax nodeRootName || nodeRootName.TypeArgumentList.Arguments.Last() is not { } nodePtr) continue;
                    var ctx = new GenericContext(context.Compilation, () => nodePtr.GetLocation(), context.ReportDiagnostic);
                    context.ReportDiagnostic(RuleIdentifiers.ReportUnnecessaryIsPtrConst(ctx, NameFormatter.ReflectionName(functionPointerParameterSymbol.Type)));
                }

                if (functionPointerParameterSymbol.Type is not IPointerTypeSymbol && functionPointerParameterParameterMetadata.IsConst)
                {
                    if (functionalPointerParameterReturnMetadata.AttributeData.ApplicationSyntaxReference!.GetSyntax() is not AttributeSyntax nodeRootRoot) continue;
                    if (nodeRootRoot.Name is not GenericNameSyntax nodeRootRootName || nodeRootRootName.TypeArgumentList.Arguments[i] is not { } nodeRoot) continue;
                    var ctx = new GenericContext(context.Compilation, () => nodeRoot.GetLocation(), context.ReportDiagnostic);
                    context.ReportDiagnostic(RuleIdentifiers.ReportUnnecessaryIsConst(ctx, NameFormatter.ReflectionName(functionPointerParameterSymbol.Type)));
                }

                if (functionPointerParameterSymbol.Type is IPointerTypeSymbol && !functionPointerParameterParameterMetadata.IsPointingToConst)
                {
                    var ctx = new GenericContext(context.Compilation, () => functionPointerParameterSyntax.GetLocation(), context.ReportDiagnostic);
                    context.ReportDiagnostic(RuleIdentifiers.ReportRequiredIsPtrConstRule(ctx, NameFormatter.ReflectionName(functionPointerParameterSymbol.Type)));
                }

                if (functionPointerParameterSymbol.Type is IPointerTypeSymbol && !functionPointerParameterParameterMetadata.IsConst)
                {
                    var ctx = new GenericContext(context.Compilation, () => functionPointerParameterSyntax.GetLocation(), context.ReportDiagnostic);
                    context.ReportDiagnostic(RuleIdentifiers.ReportRequiredIsConstRule(ctx, NameFormatter.ReflectionName(functionPointerParameterSymbol.Type)));
                }
            }
        }
    }
}