using System;
using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using NIX.Analyzers.Analyzers.Infrastructure.Rules;
using NIX.Analyzers.Analyzers.Infrastructure.Settings;

namespace NIX.Analyzers.Analyzers.Design
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FileIsTooLongAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule;

        static FileIsTooLongAnalyzer()
        {
            var factory = new RulesFactory();
            Rule = factory.CreateRule(nameof(FileIsTooLongAnalyzer));
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private Settings settings;

        public FileIsTooLongAnalyzer()
        {
        }

        public FileIsTooLongAnalyzer(Settings settings)
        {
            this.settings = settings;
        }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxTreeAction(this.HandleSyntaxTree);
        }

        private void HandleSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            SyntaxTree tree = context.Tree;
            SourceText text;
            if (tree.TryGetText(out text))
            {
                var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Tree);
                this.settings ??= ReadSettings(options);

                int maxLength = this.settings.MaxFileLines;
                int fileLength = text.Lines.Count;
                if (fileLength > maxLength)
                {
                    var fileLocation = this.GetFileLocation(tree, text, maxLength);
                    context.ReportDiagnostic(Diagnostic.Create(Rule, fileLocation.Item2, fileLocation.Item1, maxLength, fileLength));
                }
            }
        }

        private Tuple<string, Location> GetFileLocation(SyntaxTree tree, SourceText text, int startLine)
        {
            int endLine = text.Lines.Count - 1;

            Location location = Location.None;
            TextLineCollection lines = text.Lines;
            if (endLine >= startLine)
            {
                if (endLine == startLine)
                {
                    location = Location.Create(tree, lines[startLine].Span);
                }
                else
                {
                    TextSpan startSpan = lines[startLine].Span;
                    TextSpan endSpan = lines[endLine].Span;
                    location = Location.Create(tree, TextSpan.FromBounds(startSpan.Start, endSpan.End));
                }
            }

            string filePath = tree.FilePath;
            string fileName = !string.IsNullOrEmpty(filePath) ? Path.GetFileName(filePath) : string.Empty;

            return Tuple.Create(fileName, location);
        }

        private Settings ReadSettings(AnalyzerConfigOptions opt)
        {
            int maxFileLines = opt.GetValueOr(Rule.Id, "max_file_lines", Settings.Default);

            return new Settings { MaxFileLines = maxFileLines };
        }

        public class Settings
        {
            public const int Default = 1000;

            public virtual int MaxFileLines { get; init; }
        }
    }
}