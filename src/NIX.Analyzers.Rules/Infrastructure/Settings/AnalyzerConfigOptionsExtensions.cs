using Microsoft.CodeAnalysis.Diagnostics;

namespace NIX.Analyzers.Analyzers.Infrastructure.Settings
{
    internal static class AnalyzerConfigOptionsExtensions
    {
        public static int GetValueOr(this AnalyzerConfigOptions opt, string ruleId, string key, int defaultValue)
        {
            int parsedValue = 0;
            bool parseSuccess =
                opt.TryGetValue($"dotnet_diagnostic.{ruleId}.{key}", out var value)
                && int.TryParse(value, out parsedValue);

            return parseSuccess ? parsedValue : defaultValue;
        }
    }
}