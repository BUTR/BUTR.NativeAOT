using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.NativeAOT.Analyzer;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class UnnecessaryCSCodeFixProvider : CodeFixProvider
{
    private const string Title = "Fix the issue";

    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RuleIdentifiers.UnnecessaryIsConst, RuleIdentifiers.UnnecessaryIsPtrConst);

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var nodeToFix = root?.FindNode(context.Span, getInnermostNodeForTie: true);
        
        if (context.Diagnostics.Any(x => x.Id == RuleIdentifiers.UnnecessaryIsConst) && nodeToFix?.Parent is AttributeSyntax attribute1)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: ct => RemoveIsConst(context.Document, attribute1, ct),
                    equivalenceKey: Title),
                context.Diagnostics);
        }
        else if (context.Diagnostics.Any(x => x.Id == RuleIdentifiers.UnnecessaryIsConst) && nodeToFix?.Parent is TypeArgumentListSyntax)
        {
            switch (nodeToFix)
            {
                case IdentifierNameSyntax identifierName:
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: Title,
                            createChangedDocument: ct => RemoveFunctionPointerParameterIsConst(context.Document, identifierName, ct),
                            equivalenceKey: Title),
                        context.Diagnostics);
                    break;
                case GenericNameSyntax genericName:
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: Title,
                            createChangedDocument: ct => RemoveFunctionPointerParameterIsConst(context.Document, genericName, ct),
                            equivalenceKey: Title),
                        context.Diagnostics);
                    break;
            }
        }
        else if (context.Diagnostics.Any(x => x.Id == RuleIdentifiers.UnnecessaryIsPtrConst) && nodeToFix is IdentifierNameSyntax identifierName2)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: ct => RemoveIsPtrConst(context.Document, identifierName2, ct),
                    equivalenceKey: Title),
                context.Diagnostics);
        }
    }

    private static async Task<Document> RemoveIsPtrConst(Document document, IdentifierNameSyntax nodeToFix, CancellationToken ct)
    {
        if (nodeToFix.Parent is not TypeArgumentListSyntax typeArgumentList) return document;
        if (typeArgumentList.Parent is not GenericNameSyntax genericName) return document;
        
        // Standard Attribute
        if (genericName.Parent is AttributeSyntax attribute)
        {
            var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);

            if (genericName.Identifier.Text == "IsConst")
                editor.ReplaceNode(attribute, SyntaxFactory.Attribute(SyntaxFactory.ParseName("IsConst")));
            if (genericName.Identifier.Text == "IsNotConst")
                editor.RemoveNode(attribute);
        
            return editor.GetChangedDocument();
        }
        // Function Pointer
        if (genericName.Parent is TypeArgumentListSyntax)
        {
            var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);

            if (genericName.Identifier.Text == "IsConst")
                editor.ReplaceNode(genericName, SyntaxFactory.IdentifierName("IsConst"));
            if (genericName.Identifier.Text == "IsNotConst")
                editor.ReplaceNode(genericName, SyntaxFactory.IdentifierName("IsNotConst"));

            return editor.GetChangedDocument();
        }
        
        return document;
    }

    private static async Task<Document> RemoveIsConst(Document document, AttributeSyntax nodeToFix, CancellationToken ct)
    {
        if (!document.SupportsSemanticModel) return document;

        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);

        editor.RemoveNode(nodeToFix);

        return editor.GetChangedDocument();
    }
    
    private static async Task<Document> RemoveFunctionPointerParameterIsConst(Document document, IdentifierNameSyntax nodeToFix, CancellationToken ct)
    {
        if (!document.SupportsSemanticModel) return document;

        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);

        editor.ReplaceNode(nodeToFix, SyntaxFactory.IdentifierName("IsNotConst"));

        return editor.GetChangedDocument();
    }
    private static async Task<Document> RemoveFunctionPointerParameterIsConst(Document document, GenericNameSyntax nodeToFix, CancellationToken ct)
    {
        if (!document.SupportsSemanticModel) return document;

        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);

        editor.ReplaceNode(nodeToFix, nodeToFix.WithIdentifier(SyntaxFactory.Identifier("IsNotConst")));

        return editor.GetChangedDocument();
    }
}