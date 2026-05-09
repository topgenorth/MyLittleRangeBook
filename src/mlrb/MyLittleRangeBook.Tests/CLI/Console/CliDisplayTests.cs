using Spectre.Console.Testing;

namespace MyLittleRangeBook.CLI.Console
{
    public class CliDisplayTests
    {
        [Fact]
        public void WriteSuccess_should_start_with_check()
        {
            ICommandHeaderPrinter? chp = Substitute.For<ICommandHeaderPrinter>();
            var c = new TestConsole();

            var cliDisplay = new CliDisplay(c, chp);
            cliDisplay.PrintSuccess("Hello World");

            c.Output.ShouldStartWith("✓ Hello World");
        }

        [Fact]
        public void WriteSuccess_should_start_with_X()
        {
            var c = new TestConsole();
            ICommandHeaderPrinter? chp = Substitute.For<ICommandHeaderPrinter>();
            var cliDisplay = new CliDisplay(c, chp);
            cliDisplay.PrintFailure("Hello World");

            c.Output.ShouldStartWith("✗ Hello World");
        }
    }
}
