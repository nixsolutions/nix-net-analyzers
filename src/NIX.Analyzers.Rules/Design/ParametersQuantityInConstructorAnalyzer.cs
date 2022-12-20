using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NIX.Analyzers.Analyzers.Infrastructure.Rules;
using NIX.Analyzers.Analyzers.Infrastructure.Settings;

namespace NIX.Analyzers.Analyzers.Design
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ParametersQuantityInConstructorAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule;

        static ParametersQuantityInConstructorAnalyzer()
        {
            var factory = new RulesFactory();
            Rule = factory.CreateRule(nameof(ParametersQuantityInConstructorAnalyzer));
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private Settings settings;

        public ParametersQuantityInConstructorAnalyzer()
        {
        }

        public ParametersQuantityInConstructorAnalyzer(Settings settings)
        {
            this.settings = settings;
        }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(this.HandleParametersCount, SyntaxKind.ConstructorDeclaration);
        }

        private void HandleParametersCount(SyntaxNodeAnalysisContext context)
        {
            var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
            this.settings ??= ReadSettings(options);

            var constructor = context.Node as ConstructorDeclarationSyntax;

            var parametersCount = constructor.ParameterList.Parameters.Count;
            if (parametersCount > this.settings.MaxParametersInConstructor)
            {
                var location = constructor.ParameterList.GetLocation();
                var text = constructor.Identifier.Text;
                var diagnostic = Diagnostic.Create(Rule, location, parametersCount, text, this.settings.MaxParametersInConstructor);

                context.ReportDiagnostic(diagnostic);
            }
        }

        private Settings ReadSettings(AnalyzerConfigOptions opt)
        {
            int maxParams = opt.GetValueOr(Rule.Id, "max_params_in_ctor", Settings.Default);

            return new Settings { MaxParametersInConstructor = maxParams };
        }

        public class Settings
        {
            public const int Default = 5;

            public virtual int MaxParametersInConstructor { get; init; }
        }
    }
}