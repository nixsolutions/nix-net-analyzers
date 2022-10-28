using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NIX.Analyzers.Analyzers.Design;
using Nix.StyleCop.Tests.Infrastructure.Results;
using Nix.StyleCop.Tests.Infrastructure.Verifiers;

namespace Nix.StyleCop.Tests.Analyzers.Design
{
    [TestClass]
    public class ParametersQuantityInMethodTests : DiagnosticVerifier
    {
        private readonly Mock<ParametersQuantityInMethodAnalyzer.Settings> settings;

        public ParametersQuantityInMethodTests()
        {
            this.settings = new Mock<ParametersQuantityInMethodAnalyzer.Settings>();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new ParametersQuantityInMethodAnalyzer(this.settings.Object);

        [TestMethod]
        public void NoCode_NoDiagnosticIsShown()
        {
            this.settings.Setup(x => x.MaxParametersInMethod).Returns(0);

            var code = string.Empty;

            this.VerifyCSharpDiagnostic(code);
        }

        [TestMethod]
        public void ParametersQuantityIsLessOrEqualToAllowed_NoDiagnosticIsShown()
        {
            this.settings.Setup(x => x.MaxParametersInMethod).Returns(2);

            var code =
@"namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }

        public static void Compare(string left, string right)
        {
        }

        public static void Analyze()
        {
        }
    }
}";
            this.VerifyCSharpDiagnostic(code);
        }

        [TestMethod]
        public void ThisParameterIsUsed_NoDiagnosticIsShown()
        {
            this.settings.Setup(x => x.MaxParametersInMethod).Returns(2);

            var code =
@"namespace ConsoleApplication
{
    public static class Extensions
    {
        public static string DoSomeAction(this string text, int from)
        {
        }
    }
}";
            this.VerifyCSharpDiagnostic(code);
        }

        [TestMethod]
        public void ThisParameterIsUsed_OneDiagnosticIsShown()
        {
            this.settings.Setup(x => x.MaxParametersInMethod).Returns(2);

            var code =
@"namespace ConsoleApplication
{
    public static class Extensions
    {
        public static string DoSomeAction(this string text, int from, int to, int step)
        {
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = "Nix04",
                Message = @"There are 4 parameters in ""DoSomeAction"" method but the maximum allowed quantity is 3.",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 5, 42) }
            };

            this.VerifyCSharpDiagnostic(code, expected);
        }

        [TestMethod]
        public void CancellationTokenParameterIsUsed_NoDiagnosticIsShown()
        {
            this.settings.Setup(x => x.MaxParametersInMethod).Returns(2);

            var code =
@"using System.Threading;

namespace ConsoleApplication
{
    public class Program
    {
        public async Task<string> FormatStringAsync(string text, int step CancellationToken cancellationToken)
        {
            return ""Hello World"";
        }
    }
}";
            this.VerifyCSharpDiagnostic(code);
        }

        [TestMethod]
        public void CancellationTokenParameterIsUsed_OneDiagnosticIsShown()
        {
            this.settings.Setup(x => x.MaxParametersInMethod).Returns(2);

            var code =
@"using System.Threading;

namespace ConsoleApplication
{
    public class Program
    {
        public async Task<string> FormatStringAsync(string text, int from, int to, CancellationToken cancellationToken)
        {
            return ""Hello World"";
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = "Nix04",
                Message = @"There are 4 parameters in ""FormatStringAsync"" method but the maximum allowed quantity is 3.",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 7, 52) }
            };

            this.VerifyCSharpDiagnostic(code, expected);
        }

        [TestMethod]
        public void ParametersQuantityIsMoreThanToAllowed_OneDiagnosticIsShown()
        {
            this.settings.Setup(x => x.MaxParametersInMethod).Returns(2);

            var code =
@"namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args, string left, string right)
        {
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = "Nix04",
                Message = @"There are 3 parameters in ""Main"" method but the maximum allowed quantity is 2.",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 5, 32) }
            };

            this.VerifyCSharpDiagnostic(code, expected);
        }
    }
}