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
    public class MethodIsTooLongTests : DiagnosticVerifier
    {
        private readonly Mock<MethodIsTooLongAnalyzer.Settings> settings;

        public MethodIsTooLongTests()
        {
            this.settings = new Mock<MethodIsTooLongAnalyzer.Settings>();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new MethodIsTooLongAnalyzer(this.settings.Object);

        [TestMethod]
        public void NoCode_NoDiagnosticIsShown()
        {
            this.settings.Setup(x => x.MaxMethodLines).Returns(1);

            var code = string.Empty;

            this.VerifyCSharpDiagnostic(code);
        }

        [TestMethod]
        public void MethodLengthIsShorterOrEqualToAllowed_NoDiagnosticIsShown()
        {
            this.settings.Setup(x => x.MaxMethodLines).Returns(5);

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
        public void MethodLengthIsLongerThanAllowed_OneDiagnosticIsShown()
        {
            this.settings.Setup(x => x.MaxMethodLines).Returns(5);

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
            Console.WriteLine(""Some text which is not so long"");
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = "Nix02",
                Message = @"Method ""Main"" must be no longer than 5 lines (now 8).",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 5, 9) }
            };

            this.VerifyCSharpDiagnostic(code, expected);
        }

        [TestMethod]
        public void MethodLengthIsLongerThanAllowedButItIsTest_OneDiagnosticIsShown()
        {
            this.settings.Setup(x => x.MaxMethodLines).Returns(5);

            var code =
@"namespace ConsoleApplication
{
    public class Program
    {
        [TestMethod]
        public void Check_Check()
        {
            Console.WriteLine(""Some text which is not so long"");
            Console.WriteLine(""Some text which is not so long"");
            Console.WriteLine(""Some text which is not so long"");
            Console.WriteLine(""Some text which is not so long"");
            Console.WriteLine(""Some text which is not so long"");
        }
    }
}";
            this.VerifyCSharpDiagnostic(code);
        }

        [TestMethod]
        public void AutoProperty_NoDiagnosticIsShown()
        {
            this.settings.Setup(x => x.MaxMethodLines).Returns(5);

            var code =
@"namespace ConsoleApplication
{
    public class Program
    {
        public string SomeProperty { get; set; }
    }
}";

            this.VerifyCSharpDiagnostic(code);
        }

        [TestMethod]
        public void GettorAndSettorInPropertyAreShorterThanAllowed_NoDiagnosticIsShown()
        {
            this.settings.Setup(x => x.MaxMethodLines).Returns(5);

            var code =
@"namespace ConsoleApplication
{
    public class Program
    {
        public string SomeProperty
        {
            get { return ""Value""; }
            set { Console.WriteLine(value); }
        }
    }
}";

            this.VerifyCSharpDiagnostic(code);
        }

        [TestMethod]
        public void GettorAndSettorInPropertyAreLongerThanAllowed_TwoDiagnosticsAreShown()
        {
            this.settings.Setup(x => x.MaxMethodLines).Returns(5);

            var code =
@"namespace ConsoleApplication
{
    public class Program
    {
        public string SomeProperty
        {
            get
            {
                Console.WriteLine(value);
                Console.WriteLine(value);
                return ""Value"";
            }

            set
            {
                Console.WriteLine(value);
                Console.WriteLine(value);
                Console.WriteLine(value);
                Console.WriteLine(value);
            }
        }
    }
}";
            var expectedGet = new DiagnosticResult
            {
                Id = "Nix02",
                Message = @"Get accessor for property ""SomeProperty"" must be no longer than 5 lines (now 6).",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 7, 13) }
            };

            var expectedSet = new DiagnosticResult
            {
                Id = "Nix02",
                Message = @"Set accessor for property ""SomeProperty"" must be no longer than 5 lines (now 7).",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 14, 13) }
            };

            this.VerifyCSharpDiagnostic(code, expectedGet, expectedSet);
        }
    }
}