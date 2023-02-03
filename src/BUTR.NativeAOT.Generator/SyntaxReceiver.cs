using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Linq;

namespace BUTR.NativeAOT.Generator;

public class SyntaxReceiver : ISyntaxReceiver
{
    public List<MethodDeclarationSyntax> Methods { get; } = new();

    /// <summary>
    /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
    /// </summary>
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        // any field with at least one attribute is a candidate for property generation
        if (syntaxNode is not MethodDeclarationSyntax { AttributeLists.Count: > 0 } methodDeclarationSyntax) return;

        foreach (var attribute in methodDeclarationSyntax.AttributeLists.SelectMany(x => x.Attributes))
        {
            if (attribute.Name is IdentifierNameSyntax { Identifier.Text: "UnmanagedCallersOnly" })
            {
                Methods.Add(methodDeclarationSyntax);
            }
            if (attribute.Name is QualifiedNameSyntax { Right.Identifier.Text: "UnmanagedCallersOnly" })
            {
                Methods.Add(methodDeclarationSyntax);
            }
        }
    }
}