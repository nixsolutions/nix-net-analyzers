using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using NIX.Analyzers.Analyzers.Infrastructure.Rules;
using NIX.Analyzers.Analyzers.Infrastructure.Settings;

namespace NIX.Analyzers.Analyzers.Analyzers.Design
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LineIsTooLongAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule;

        static LineIsTooLongAnalyzer()
        {
            var factory = new RulesFactory();
            Rule = factory.CreateRule(nameof(LineIsTooLongAnalyzer));
        }

        private Settings settings;

        public LineIsTooLongAnalyzer()
        {
        }

        public LineIsTooLongAnalyzer(Settings settings)
        {
            this.settings = settings;
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxTreeAction(this.HandleSyntaxTree);
        }

        private void HandleSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Tree);
            this.settings ??= ReadSettings(options);

            SourceText text;
            if (context.Tree.TryGetText(out text))
            {
                foreach (TextLine line in text.Lines)
                {
                    TextSpan lineSpan = line.Span;
                    if (line.Span.Length > this.settings.MaxLineLength)
                    {
                        TextSpan excess = TextSpan.FromBounds(line.Span.End - this.settings.MaxLineLength, lineSpan.End);
                        Location location = Location.Create(context.Tree, excess);
                        context.ReportDiagnostic(Diagnostic.Create(Rule, location, this.settings.MaxLineLength, line.Span.Length));
                    }
                }
            }
        }

        private Settings ReadSettings(AnalyzerConfigOptions opt)
        {
            int maxLineLength = opt.GetValueOr(Rule.Id, "max_line_length", Settings.Default);

            return new Settings { MaxLineLength = maxLineLength };
        }

        public class Settings
        {
            public const int Default = 200;

            public virtual int MaxLineLength { get; init; }
        }
    }
}