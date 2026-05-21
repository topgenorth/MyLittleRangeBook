using MyLittleRangeBook.Console;
using Spectre.Console;
using Spectre.Console.Rendering;
using Spectre.Console.Testing;
using AnsiConsoleExtensions = MyLittleRangeBook.Console.AnsiConsoleExtensions;

namespace MyLittleRangeBook.CLI.Console
{
    public class SimpleAppHeaderTests
    {
        [Fact]
        public void BuildRenderable_should_return_a_panel()
        {
            SimpleAppHeader header = new SimpleAppHeader().SetAppVersion("1.2.3");

            IRenderable r = header.BuildRenderable();

            Assert.IsType<Panel>(r);
        }

        [Fact]
        public void BuildRenderable_should_contain_app_name_and_version()
        {
            SimpleAppHeader header = new SimpleAppHeader().SetAppVersion("1.2.3");
            var c = new TestConsole();
            header.Print(c);
            c.Output.ShouldContain(AnsiConsoleExtensions.AppName);
            c.Output.ShouldContain("1.2.3");
        }
    }
}
