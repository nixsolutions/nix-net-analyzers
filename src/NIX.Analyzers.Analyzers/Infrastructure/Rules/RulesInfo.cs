using System.Collections.Generic;
using NIX.Analyzers.Analyzers.Analyzers.Design;

namespace NIX.Analyzers.Analyzers.Infrastructure.Rules
{
    public class RulesInfo
    {
        private static RulesInfo instance;

        private readonly Dictionary<string, RuleData> info;

        private RulesInfo()
        {
            this.info = new Dictionary<string, RuleData>
            {
                { nameof(LineIsTooLongAnalyzer), new RuleData { Number = "Nix01", Type = RuleTypes.DESIGN } },
                { nameof(MethodIsTooLongAnalyzer), new RuleData { Number = "Nix02", Type = RuleTypes.DESIGN } },
                { nameof(FileIsTooLongAnalyzer), new RuleData { Number = "Nix03", Type = RuleTypes.DESIGN } },
                { nameof(ParametersQuantityInMethodAnalyzer), new RuleData { Number = "Nix04", Type = RuleTypes.DESIGN } },
                { nameof(ParametersQuantityInConstructorAnalyzer), new RuleData { Number = "Nix05", Type = RuleTypes.DESIGN } },
            };
        }

        public static RulesInfo Instance => instance ??= new RulesInfo();

        public RuleData this[string analyzerName] => this.info[analyzerName];
    }
}