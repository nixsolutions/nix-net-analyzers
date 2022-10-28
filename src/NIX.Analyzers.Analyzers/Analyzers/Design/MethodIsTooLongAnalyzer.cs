using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using NIX.Analyzers.Analyzers.Infrastructure.Rules;
using NIX.Analyzers.Analyzers.Infrastructure.Settings;

namespace NIX.Analyzers.Analyzers.Analyzers.Design
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MethodIsTooLongAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule;

        static MethodIsTooLongAnalyzer()
        {
            var factory = new RulesFactory();
            Rule = factory.CreateRule(nameof(MethodIsTooLongAnalyzer));
        }

        private static readonly HashSet<SyntaxKind> SupportedSyntaxKinds = new HashSet<SyntaxKind>
        {
            SyntaxKind.MethodDeclaration,
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.DestructorDeclaration,
            SyntaxKind.ConversionOperatorDeclaration,
            SyntaxKind.OperatorDeclaration,
            SyntaxKind.GetAccessorDeclaration,
            SyntaxKind.SetAccessorDeclaration,
            SyntaxKind.AddAccessorDeclaration,
            SyntaxKind.RemoveAccessorDeclaration,
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private Settings settings;

        public MethodIsTooLongAnalyzer()
        {
        }

        public MethodIsTooLongAnalyzer(Settings settings)
        {
            this.settings = settings;
        }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCodeBlockAction(this.HandleCodeBlock);
        }

        private void HandleCodeBlock(CodeBlockAnalysisContext context)
        {
            var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.CodeBlock.SyntaxTree);
            this.settings ??= this.ReadSettings(options);

            var diagnostic = this.GetDiagnosticIfBlockTooLong(context, Rule, this.settings.MaxMethodLines);
            if (diagnostic != null)
            {
                context.ReportDiagnostic(diagnostic);
            }
        }

        private Diagnostic GetDiagnosticIfBlockTooLong(
            CodeBlockAnalysisContext context,
            DiagnosticDescriptor descriptor,
            int maxLineCount)
        {
            Diagnostic diagnostic = null;

            if (context.OwningSymbol.Kind == SymbolKind.Method)
            {
                SyntaxNode block = context.CodeBlock;
                SyntaxKind blockKind = block.Kind();

                if (!this.IsTestMethod(context) && SupportedSyntaxKinds.Contains(blockKind))
                {
                    SyntaxTree tree = block.SyntaxTree;
                    SourceText treeText;
                    if (tree != null && tree.TryGetText(out treeText))
                    {
                        SourceText blockText = treeText.GetSubText(block.Span);
                        int blockLineCount = blockText.Lines.Count;
                        if (blockLineCount > maxLineCount)
                        {
                            Location location = this.GetFirstLineLocation(block);
                            string blockName = context.OwningSymbol.Name;
                            string blockDescription = this.GetBlockDescription(blockName, blockKind, context.OwningSymbol?.ContainingType?.Name);
                            diagnostic = Diagnostic.Create(descriptor, location, blockDescription, maxLineCount, blockLineCount);
                        }
                    }
                }
            }

            return diagnostic;
        }

        private bool IsTestMethod(CodeBlockAnalysisContext context)
        {
            bool result = false;

            var method = context.CodeBlock as MethodDeclarationSyntax;
            if (method != null)
            {
                var identifier = method.AttributeLists
                                        .SelectMany(x => x.Attributes)
                                        .OfType<AttributeSyntax>()
                                        .Select(x => x.Name)
                                        .OfType<IdentifierNameSyntax>()
                                        .FirstOrDefault(x => x.Identifier.Text == "TestMethod");
                result = identifier != null;
            }

            return result;
        }

        private string GetBlockDescription(string blockName, SyntaxKind blockKind, string containingTypeName)
        {
            string result;
            switch (blockKind)
            {
                case SyntaxKind.ConstructorDeclaration:
                    result = (blockName == ".cctor" ? @"Static constructor """ : @"Constructor """) + (containingTypeName ?? blockName) + @"""";
                    break;

                case SyntaxKind.DestructorDeclaration:
                    result = @"Destructor """ + ('~' + containingTypeName ?? blockName) + @"""";
                    break;

                case SyntaxKind.ConversionOperatorDeclaration:
                case SyntaxKind.OperatorDeclaration:
                    // Improve: This could describe Implicit and Explicit conversion operators better (e.g., show converted type name).
                    result = @"Operator """ + this.TrimPrefix(blockName, "op_") + @"""";
                    break;

                case SyntaxKind.GetAccessorDeclaration:
                    // Improve: This could describe indexers better (e.g., report this instead of Item).
                    result = @"Get accessor for property """ + this.TrimPrefix(blockName, "get_") + @"""";
                    break;

                case SyntaxKind.SetAccessorDeclaration:
                    // Improve: This could describe indexers better (e.g., report this instead of Item).
                    result = @"Set accessor for property """ + this.TrimPrefix(blockName, "set_") + @"""";
                    break;

                case SyntaxKind.AddAccessorDeclaration:
                    result = @"Event """ + this.TrimPrefix(blockName, "add_") + @"""" + " add accessor";
                    break;

                case SyntaxKind.RemoveAccessorDeclaration:
                    result = @"Event """ + this.TrimPrefix(blockName, "remove_") + @"""" + " remove accessor";
                    break;

                case SyntaxKind.MethodDeclaration:
                    result = @"Method """ + blockName + @"""";
                    break;

                default:
                    result = this.TrimSuffix(blockKind.ToString(), @"Declaration""") + ' ' + blockName + @"""";
                    break;
            }

            return result;
        }

        private string TrimPrefix(string name, string prefix)
        {
            string result = name.StartsWith(prefix) ? name.Substring(prefix.Length) : name;
            return result;
        }

        private string TrimSuffix(string name, string suffix)
        {
            string result = name.EndsWith(suffix) ? name.Substring(0, name.Length - suffix.Length) : name;
            return result;
        }

        public Location GetFirstLineLocation(SyntaxNode node)
        {
            Location result;

            SyntaxTree tree = node.SyntaxTree;
            SourceText text;
            if (tree.TryGetText(out text))
            {
                // If the node starts with attribute list(s), then get the start of the first child that's not an attribute list.
                int nodeStart = node.SpanStart;
                ChildSyntaxList children = node.ChildNodesAndTokens();
                if (children[0].IsKind(SyntaxKind.AttributeList))
                {
                    // There can be multiple attribute lists on a node, but a node must have at least one leaf token child.
                    nodeStart = children.First(child => !child.IsKind(SyntaxKind.AttributeList)).SpanStart;
                }

                TextSpan lineSpan = text.Lines.GetLineFromPosition(nodeStart).Span;
                TextSpan blockFirstLineSpan = TextSpan.FromBounds(nodeStart, lineSpan.End);
                result = Location.Create(tree, blockFirstLineSpan);
            }
            else
            {
                // If we don't have source text, then "first line" isn't meaningful.
                result = node.GetLocation();
            }

            return result;
        }

        private Settings ReadSettings(AnalyzerConfigOptions opt)
        {
            int maxMethodLines = opt.GetValueOr(Rule.Id, "max_method_lines", Settings.Default);

            return new Settings {  MaxMethodLines = maxMethodLines };
        }

        public class Settings
        {
            public const int Default = 50;

            public virtual int MaxMethodLines { get; init; }
        }
    }
}