using Microsoft.CodeAnalysis;
using NIX.Analyzers.Analyzers.Properties;

namespace NIX.Analyzers.Analyzers.Infrastructure.Rules
{
    public class RulesFactory
    {
        public DiagnosticDescriptor CreateRule(string analyzerName)
        {
            var info = RulesInfo.Instance[analyzerName];

            var title = new LocalizableResourceString($"{info.Number}Title", Resources.ResourceManager, typeof(Resources));
            var messageFormat = new LocalizableResourceString($"{info.Number}MessageFormat", Resources.ResourceManager, typeof(Resources));
            var description = new LocalizableResourceString($"{info.Number}Description", Resources.ResourceManager, typeof(Resources));

            return new DiagnosticDescriptor(info.Number, title, messageFormat, info.Type, DiagnosticSeverity.Warning, true, description);
        }
    }
}