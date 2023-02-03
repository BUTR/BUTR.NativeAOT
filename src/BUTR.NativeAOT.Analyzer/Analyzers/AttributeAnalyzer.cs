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
    public class AttributeAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            RuleIdentifiers.UnnecessaryIsConstRule,
            RuleIdentifiers.UnnecessaryIsPtrConstRule
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
            
            if (Helper.TryGetMethodMetadata(methodSymbol, out var methodConstMetadata))
            {
                CheckReturnType(context, methodSymbol, methodConstMetadata);
            }

            foreach (var parameterSyntax in methodDeclarationSyntax.ParameterList.Parameters)
            {
                if (context.SemanticModel.GetDeclaredSymbol(parameterSyntax) is not { } parameterSymbol) continue;

                if (parameterSymbol.Type is IFunctionPointerTypeSymbol functionPointerTypeSymbol)
                {
                    CheckFunctionPointerParameter(context, methodSymbol, parameterSymbol, functionPointerTypeSymbol);
                }
                else
                {
                    if (Helper.TryGetParameterMetadata(methodSymbol, parameterSymbol, out var parameterConstMetadata))
                    {
                        CheckParameter(context, parameterSyntax, parameterSymbol, parameterConstMetadata);
                    }
                }
            }
        }

        private static void CheckReturnType(SyntaxNodeAnalysisContext context, IMethodSymbol methodSymbol, ConstMetadata constMetadata)
        {
            if (methodSymbol.ReturnType is not IPointerTypeSymbol && constMetadata.IsPointingToConst)
            {
                var nodeRoot = (AttributeSyntax) constMetadata.AttributeData.ApplicationSyntaxReference!.GetSyntax();
                var nodePtr = (nodeRoot.Name as GenericNameSyntax).TypeArgumentList.Arguments[0];
                var ctx = new GenericContext(context.Compilation, () => nodePtr.GetLocation(), context.ReportDiagnostic);
                context.ReportDiagnostic(RuleIdentifiers.ReportUnnecessaryIsPtrConst(ctx, NameFormatter.ReflectionName(methodSymbol.ReturnType)));
            }
            if (methodSymbol.ReturnType is not IPointerTypeSymbol && constMetadata.IsConst)
            {
                var nodeRoot = (AttributeSyntax) constMetadata.AttributeData.ApplicationSyntaxReference!.GetSyntax();
                var ctx = new GenericContext(context.Compilation, () => nodeRoot.GetLocation(), context.ReportDiagnostic);
                context.ReportDiagnostic(RuleIdentifiers.ReportUnnecessaryIsConst(ctx, NameFormatter.ReflectionName(methodSymbol.ReturnType)));
            }
        }
        
        private static void CheckParameter(SyntaxNodeAnalysisContext context, ParameterSyntax parameterSyntax, IParameterSymbol parameterSymbol, ConstMetadata constMetadata)
        {
            if (parameterSymbol.Type is not IPointerTypeSymbol && constMetadata.IsPointingToConst)
            {
                var nodeRoot = (AttributeSyntax) constMetadata.AttributeData.ApplicationSyntaxReference!.GetSyntax();
                var nodePtr = (nodeRoot.Name as GenericNameSyntax).TypeArgumentList.Arguments[0];
                var ctx = new GenericContext(context.Compilation, () => nodePtr.GetLocation(), context.ReportDiagnostic);
                context.ReportDiagnostic(RuleIdentifiers.ReportUnnecessaryIsPtrConst(ctx, NameFormatter.ReflectionName(parameterSymbol.Type)));
            }
            if (parameterSymbol.Type is not IPointerTypeSymbol && constMetadata.IsConst)
            {
                var nodeRoot = (AttributeSyntax) constMetadata.AttributeData.ApplicationSyntaxReference!.GetSyntax();
                var ctx = new GenericContext(context.Compilation, () => nodeRoot.GetLocation(), context.ReportDiagnostic);
                context.ReportDiagnostic(RuleIdentifiers.ReportUnnecessaryIsConst(ctx, NameFormatter.ReflectionName(parameterSymbol.Type)));
            }
        }
        
        private static void CheckFunctionPointerParameter(SyntaxNodeAnalysisContext context, IMethodSymbol methodSymbol, IParameterSymbol parameterSymbol, IFunctionPointerTypeSymbol functionPointerTypeSymbol)
        {
            var functionalPointerParameterReturnMetadata = Helper.TryGetFunctionalPointerParameterMetadata(parameterSymbol, functionPointerTypeSymbol, methodSymbol.Parameters.Length, out var val) ? val : ConstMetadata.Empty;
            var functionalPointerParameterParameters = methodSymbol.Parameters.Select((x, i) => Helper.TryGetFunctionalPointerParameterMetadata(x, functionPointerTypeSymbol, i, out var val) ? val : ConstMetadata.Empty).ToImmutableArray();

            if (functionPointerTypeSymbol.Signature.ReturnType is not IPointerTypeSymbol && functionalPointerParameterReturnMetadata.IsPointingToConst)
            {
                var nodeRootRoot = (AttributeSyntax) functionalPointerParameterReturnMetadata.AttributeData.ApplicationSyntaxReference!.GetSyntax();
                var nodeRoot = (nodeRootRoot.Name as GenericNameSyntax).TypeArgumentList.Arguments.Last();
                var nodePtr = (nodeRoot as GenericNameSyntax).TypeArgumentList.Arguments.Last();
                var ctx = new GenericContext(context.Compilation, () => nodePtr.GetLocation(), context.ReportDiagnostic);
                context.ReportDiagnostic(RuleIdentifiers.ReportUnnecessaryIsPtrConst(ctx, NameFormatter.ReflectionName(methodSymbol.ReturnType)));
            }
            if (functionPointerTypeSymbol.Signature.ReturnType is not IPointerTypeSymbol && functionalPointerParameterReturnMetadata.IsConst)
            {
                var nodeRootRoot = (AttributeSyntax) functionalPointerParameterReturnMetadata.AttributeData.ApplicationSyntaxReference!.GetSyntax();
                var nodeRoot = (nodeRootRoot.Name as GenericNameSyntax).TypeArgumentList.Arguments.Last();
                var ctx = new GenericContext(context.Compilation, () => nodeRoot.GetLocation(), context.ReportDiagnostic);
                context.ReportDiagnostic(RuleIdentifiers.ReportUnnecessaryIsConst(ctx, NameFormatter.ReflectionName(methodSymbol.ReturnType)));
            }

            for (var i = 0; i < functionalPointerParameterParameters.Length; i++)
            {
                var functionPointerParameterParameterMetadata = functionalPointerParameterParameters[i];
                var functionPointerParameterSymbol = functionPointerTypeSymbol.Signature.Parameters[i];

                if (functionPointerParameterSymbol.Type is not IPointerTypeSymbol && functionPointerParameterParameterMetadata.IsPointingToConst)
                {
                    var nodeRootRoot = (AttributeSyntax) functionalPointerParameterReturnMetadata.AttributeData.ApplicationSyntaxReference!.GetSyntax();
                    var nodeRoot = (nodeRootRoot.Name as GenericNameSyntax).TypeArgumentList.Arguments[i];
                    var nodePtr = (nodeRoot as GenericNameSyntax).TypeArgumentList.Arguments.Last();
                    var ctx = new GenericContext(context.Compilation, () => nodePtr.GetLocation(), context.ReportDiagnostic);
                    context.ReportDiagnostic(RuleIdentifiers.ReportUnnecessaryIsPtrConst(ctx, NameFormatter.ReflectionName(methodSymbol.ReturnType)));
                }

                if (functionPointerParameterSymbol.Type is not IPointerTypeSymbol && functionPointerParameterParameterMetadata.IsConst)
                {
                    var nodeRootRoot = (AttributeSyntax) functionalPointerParameterReturnMetadata.AttributeData.ApplicationSyntaxReference!.GetSyntax();
                    var nodeRoot = (nodeRootRoot.Name as GenericNameSyntax).TypeArgumentList.Arguments[i];
                    var ctx = new GenericContext(context.Compilation, () => nodeRoot.GetLocation(), context.ReportDiagnostic);
                    context.ReportDiagnostic(RuleIdentifiers.ReportUnnecessaryIsConst(ctx, NameFormatter.ReflectionName(methodSymbol.ReturnType)));
                }
            }
        }
    }
}