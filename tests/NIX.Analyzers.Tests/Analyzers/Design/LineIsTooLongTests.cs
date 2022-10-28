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
    public class LineIsTooLongTests : DiagnosticVerifier
    {
        private readonly Mock<LineIsTooLongAnalyzer.Settings> settings;

        public LineIsTooLongTests()
        {
            this.settings = new Mock<LineIsTooLongAnalyzer.Settings>();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new LineIsTooLongAnalyzer(this.settings.Object);

        [TestMethod]
        public void NoCode_NoDiagnosticIsShown()
        {
            this.settings.Setup(x => x.MaxLineLength).Returns(5);

            var code = string.Empty;

            this.VerifyCSharpDiagnostic(code);
        }

        [TestMethod]
        public void AllLinesAreShorterOrEqualToAllowed_NoDiagnosticIsShown()
        {
            this.settings.Setup(x => x.MaxLineLength).Returns(65);

            var code =
@"namespace ConsoleApplication
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine(""Some text which is not so long"");
        }
    }
}";

            this.VerifyCSharpDiagnostic(code);
        }

        [TestMethod]
        public void OneLineIsLongerThanAllowed_OneDiagnosticIsShown()
        {
            this.settings.Setup(x => x.MaxLineLength).Returns(65);

            var code =
@"namespace ConsoleApplication
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine(""Some text which is not so long but it's longer than allowed"");
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = "Nix01",
                Message = "Line must be no longer than 65 characters (now 93).",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 7, 29) }
            };

            this.VerifyCSharpDiagnostic(code, expected);
        }
    }
}