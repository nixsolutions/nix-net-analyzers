using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NIX.Analyzers.Analyzers.Analyzers.Design;
using Nix.StyleCop.Tests.Infrastructure.Results;
using Nix.StyleCop.Tests.Infrastructure.Verifiers;

namespace Nix.StyleCop.Tests.Analyzers.Design
{
    [TestClass]
    public class ParametersQuantityInConstructorTests : DiagnosticVerifier
    {
        private readonly Mock<ParametersQuantityInConstructorAnalyzer.Settings> settings;

        public ParametersQuantityInConstructorTests()
        {
            this.settings = new Mock<ParametersQuantityInConstructorAnalyzer.Settings>();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new ParametersQuantityInConstructorAnalyzer(this.settings.Object);

        [TestMethod]
        public void NoCode_NoDiagnosticIsShown()
        {
            this.settings.Setup(x => x.MaxParametersInConstructor).Returns(0);

            var code = string.Empty;

            this.VerifyCSharpDiagnostic(code);
        }

        [TestMethod]
        public void ParametersQuantityIsLessOrEqualToAllowed_NoDiagnosticIsShown()
        {
            this.settings.Setup(x => x.MaxParametersInConstructor).Returns(2);

            var code =
@"namespace ConsoleApplication
{
    public class Program
    {
        public Program(string[] args)
        {
        }

        public Program(string left, string right)
        {
        }

        public Program()
        {
        }
    }
}";
            this.VerifyCSharpDiagnostic(code);
        }

        [TestMethod]
        public void ParametersQuantityIsMoreThanToAllowed_OneDiagnosticIsShown()
        {
            this.settings.Setup(x => x.MaxParametersInConstructor).Returns(2);

            var code =
@"namespace ConsoleApplication
{
    public class Program
    {
        public Program(string[] args, string left, string right)
        {
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = "Nix05",
                Message = @"There are 3 parameters in ""Program"" constructor but the maximum allowed quantity is 2.",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 5, 23) }
            };

            this.VerifyCSharpDiagnostic(code, expected);
        }
    }
}