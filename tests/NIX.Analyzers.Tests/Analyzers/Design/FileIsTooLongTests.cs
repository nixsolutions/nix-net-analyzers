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
    public class FileIsTooLongTests : DiagnosticVerifier
    {
        private readonly Mock<FileIsTooLongAnalyzer.Settings> settings;

        public FileIsTooLongTests()
        {
            this.settings = new Mock<FileIsTooLongAnalyzer.Settings>();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new FileIsTooLongAnalyzer(this.settings.Object);

        [TestMethod]
        public void NoCode_NoDiagnosticIsShown()
        {
            this.settings.Setup(x => x.MaxFileLines).Returns(1);

            var code = string.Empty;

            this.VerifyCSharpDiagnostic(code);
        }

        [TestMethod]
        public void FileLengthIsShorterOrEqualToAllowed_NoDiagnosticIsShown()
        {
            this.settings.Setup(x => x.MaxFileLines).Returns(11);

            var code =
@"namespace ConsoleApplication
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine(""Some text which is not so long"");
            Console.WriteLine(""Some text which is not so long"");
        }
    }
}";

            this.VerifyCSharpDiagnostic(code);
        }

        [TestMethod]
        public void FileLengthIsLongerThanAllowed_OneDiagnosticIsShown()
        {
            this.settings.Setup(x => x.MaxFileLines).Returns(10);

            var code =
@"namespace ConsoleApplication
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine(""Some text which is not so long"");
            Console.WriteLine(""Some text which is not so long"");
            Console.WriteLine(""Some text which is not so long"");
            Console.WriteLine(""Some text which is not so long"");
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = "Nix03",
                Message = @"File ""Test0.cs"" must be no longer than 10 lines (now 13).",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 11, 0) }
            };

            this.VerifyCSharpDiagnostic(code, expected);
        }
    }
}