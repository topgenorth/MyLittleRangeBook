using Spectre.Console;
using Spectre.Console.Rendering;

namespace MyLittleRangeBook.CLI.Console
{
    class SimpleAppHeader : IConsolePrinter
    {
        const string AppName = "MyLittleRangeBook CLI";

        string _action = string.Empty;
        string _appVersion = string.Empty;

        public SimpleAppHeader SetAction(string action)
        {
            _action = action;

            return this;
        }

        public SimpleAppHeader SetAppVersion(string appVersion)
        {
            _appVersion = appVersion;

            return this;
        }
        public void Print(IAnsiConsole console)
        {
            console.Write(BuildRenderable());
        }

        public IRenderable BuildRenderable()
        {
            Grid grid = new Grid();
            grid.AddColumn();
            grid.AddRow($"[bold white]{Markup.Escape(AppName)} {_appVersion}[/]");

            Panel panel = new Panel(grid)
                .Expand()
                .Padding(0, 1,0,0)
                .Border(BoxBorder.None);

            return panel;
        }
    }
}
