using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using EchoTrace.CodeAnalyzer.Bases;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

namespace EchoTrace.CodeAnalyzer.ContractAnalyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ContractNamingFixProvider)), Shared]
public class ContractNamingFixProvider : CodeFixProvider
{
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(ContractNamingAnalyzer.ContractNamingDiagnosticId);

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var root = await document.GetSyntaxRootAsync(context.CancellationToken);
        foreach (var diagnostic in context.Diagnostics)
        {
            if (diagnostic.Id == ContractNamingAnalyzer.ContractNamingDiagnosticId)
            {
                var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: "重命名为Contract结尾",
                        c => FixContractName(document, token, c),
                        equivalenceKey: ContractNamingAnalyzer.ContractNamingDiagnosticId),
                    diagnostic);
            }
        }
    }

    public async Task<Document> FixContractName(Document document, SyntaxToken token,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken);
        root = root.ReplaceToken(token, SyntaxFactory.Identifier(token.ValueText + "Contract"));
        return document.WithSyntaxRoot(root);
    }
}