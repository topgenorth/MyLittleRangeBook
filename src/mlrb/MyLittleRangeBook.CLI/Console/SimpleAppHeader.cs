using MyLittleRangeBook.IO;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace MyLittleRangeBook.CLI.Console
{
    public class SimpleAppHeader : IConsolePrinter
    {
        string? _appVersion;


        public void Print(IAnsiConsole console)
        {
            console.Write(BuildRenderable());
        }

        public IRenderable BuildRenderable()
        {
            string v = _appVersion ?? GetType().Assembly.GetAssemblyVersionInformation();

            var grid = new Grid();
            grid.AddColumn();
            grid.AddRow($"[bold white]{Markup.Escape(AnsiConsoleExtensions.AppName)} {v}[/]");

            Panel panel = new Panel(grid)
                .Expand()
                .Padding(0, 1, 0, 0)
                .Border(BoxBorder.None);

            return panel;
        }


        public SimpleAppHeader SetAction(string action)
        {
            // [TO20260425] NOOP

            return this;
        }

        public SimpleAppHeader SetAppVersion(string appVersion)
        {
            _appVersion = appVersion.Trim();

            return this;
        }
    }
}
