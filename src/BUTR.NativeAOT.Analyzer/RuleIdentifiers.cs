using BUTR.NativeAOT.Analyzer.Data;
using BUTR.NativeAOT.Analyzer.Utils;

using Microsoft.CodeAnalysis;

using System.Globalization;

namespace BUTR.NativeAOT.Analyzer
{
    public static class RuleIdentifiers
    {
        public const string UnnecessaryIsConst = "BNA0001";
        public const string UnnecessaryIsPtrConst = "BNA0002";
        public const string RequiredIsConst = "BNA0003";
        public const string RequiredIsPtrConst = "BNA0004";

        private static string GetHelpUri(string idenfifier) =>
            string.Format(CultureInfo.InvariantCulture, "https://github.com/BUTR/BUTR.NativeAOT.Analyzer/blob/master/docs/Rules/{0}.md", idenfifier);
        
        internal static readonly DiagnosticDescriptor UnnecessaryIsConstRule = new(
            UnnecessaryIsConst,
            title: "Unnecessary IsConst",
            messageFormat: "Unnecessary IsConst for type '{0}'",
            RuleCategories.Usage,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "",
            helpLinkUri: GetHelpUri(UnnecessaryIsConst));
        
        internal static readonly DiagnosticDescriptor UnnecessaryIsPtrConstRule = new(
            UnnecessaryIsPtrConst,
            title: "Unnecessary IsPtrConst",
            messageFormat: "Unnecessary IsPtrConst for type '{0}'",
            RuleCategories.Usage,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "",
            helpLinkUri: GetHelpUri(UnnecessaryIsPtrConst));
        internal static readonly DiagnosticDescriptor RequiredIsConstRule = new(
            RequiredIsConst,
            title: "Required IsConst",
            messageFormat: "Required IsConst for type '{0}'",
            RuleCategories.Usage,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "",
            helpLinkUri: GetHelpUri(RequiredIsConst));
        
        internal static readonly DiagnosticDescriptor RequiredIsPtrConstRule = new(
            RequiredIsPtrConst,
            title: "Required IsPtrConst",
            messageFormat: "Required IsPtrConst for type '{0}'",
            RuleCategories.Usage,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "",
            helpLinkUri: GetHelpUri(RequiredIsPtrConst));

        
        internal static Diagnostic ReportUnnecessaryIsConst(GenericContext context, string typeName) =>
            DiagnosticUtils.CreateDiagnostic(UnnecessaryIsConstRule, context, typeName);
        internal static Diagnostic ReportUnnecessaryIsPtrConst(GenericContext context, string typeName) =>
            DiagnosticUtils.CreateDiagnostic(UnnecessaryIsPtrConstRule, context, typeName);
        internal static Diagnostic ReportRequiredIsConstRule(GenericContext context, string typeName) =>
            DiagnosticUtils.CreateDiagnostic(RequiredIsConstRule, context, typeName);
        internal static Diagnostic ReportRequiredIsPtrConstRule(GenericContext context, string typeName) =>
            DiagnosticUtils.CreateDiagnostic(RequiredIsPtrConstRule, context, typeName);
    }
}