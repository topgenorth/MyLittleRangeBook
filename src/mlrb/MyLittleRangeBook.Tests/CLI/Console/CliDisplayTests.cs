using Spectre.Console.Testing;

namespace MyLittleRangeBook.CLI.Console
{
    public class CliDisplayTests
    {
        [Fact]
        public void WriteSuccess_should_start_with_check()
        {
            var c = new TestConsole();

            var cliDisplay = new CliDisplay(c);
            cliDisplay.PrintSuccess("Hello World");

            c.Output.ShouldStartWith("✓ Hello World");
        }

        [Fact]
        public void WriteSuccess_should_start_with_X()
        {
            var c = new TestConsole();

            var cliDisplay = new CliDisplay(c);
            cliDisplay.PrintFailure("Hello World");

            c.Output.ShouldStartWith("✗ Hello World");
        }
    }
}
