using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NIX.Analyzers.Analyzers.Infrastructure.Rules;
using NIX.Analyzers.Analyzers.Infrastructure.Settings;

namespace NIX.Analyzers.Analyzers.Design
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ParametersQuantityInMethodAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule;

        static ParametersQuantityInMethodAnalyzer()
        {
            var factory = new RulesFactory();
            Rule = factory.CreateRule(nameof(ParametersQuantityInMethodAnalyzer));
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private Settings settings;

        public ParametersQuantityInMethodAnalyzer()
        {
        }

        public ParametersQuantityInMethodAnalyzer(Settings settings)
        {
            this.settings = settings;
        }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(this.HandleParametersCount, SyntaxKind.MethodDeclaration);
        }

        private void HandleParametersCount(SyntaxNodeAnalysisContext context)
        {
            int maxParametersInMethod = this.GetMaxParametersInMethod(context);

            var method = context.Node as MethodDeclarationSyntax;
            var parametersCount = method.ParameterList.Parameters.Count;
            if (parametersCount > maxParametersInMethod)
            {
                var location = method.ParameterList.GetLocation();
                var text = method.Identifier.Text;
                var diagnostic = Diagnostic.Create(Rule, location, parametersCount, text, maxParametersInMethod);

                context.ReportDiagnostic(diagnostic);
            }
        }

        private int GetMaxParametersInMethod(SyntaxNodeAnalysisContext context)
        {
            var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
            this.settings ??= ReadSettings(options);
            var maxParametersInMethod = this.settings.MaxParametersInMethod;

            var methodSymbol = (IMethodSymbol)context.ContainingSymbol;
            if (methodSymbol.IsExtensionMethod)
            {
                maxParametersInMethod++;
            }

            var cancellationToken = methodSymbol.Parameters
                                                .FirstOrDefault(x => x.Type.ToString() == "System.Threading.CancellationToken");
            if (methodSymbol.IsAsync && cancellationToken != null)
            {
                maxParametersInMethod++;
            }

            return maxParametersInMethod;
        }

        private Settings ReadSettings(AnalyzerConfigOptions opt)
        {
            int maxParams = opt.GetValueOr(Rule.Id, "max_params_in_method", Settings.Default);

            return new Settings { MaxParametersInMethod = maxParams };
        }

        public class Settings
        {
            public const int Default = 5;

            public virtual int MaxParametersInMethod { get; init; }
        }
    }
}