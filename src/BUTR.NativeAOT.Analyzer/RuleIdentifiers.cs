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

        
        internal static Diagnostic ReportUnnecessaryIsConst(GenericContext context, string typeName) =>
            DiagnosticUtils.CreateDiagnostic(UnnecessaryIsConstRule, context, typeName);
        
        internal static Diagnostic ReportUnnecessaryIsPtrConst(GenericContext context, string typeName) =>
            DiagnosticUtils.CreateDiagnostic(UnnecessaryIsPtrConstRule, context, typeName);
    }
}