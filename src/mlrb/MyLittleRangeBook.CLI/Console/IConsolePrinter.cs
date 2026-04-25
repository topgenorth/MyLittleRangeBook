using Spectre.Console;
using Spectre.Console.Rendering;

namespace MyLittleRangeBook.CLI.Console
{
    public interface IConsolePrinter
    {
        void Print(IAnsiConsole console);
        IRenderable BuildRenderable();
    }

    class OriginalAppHeaderPrinter : IConsolePrinter
    {
        const string AppName = "MyLittleRangeBook CLI";

        string _action = string.Empty;
        string _appVersion = string.Empty;


        public void Print(IAnsiConsole console)
        {
            IRenderable header = BuildRenderable();
            console.Write(header);
            console.WriteLine();
        }

        public IRenderable BuildRenderable()
        {
            var grid = new Grid();
            grid.AddColumn();

            grid.AddRow($"[bold]{Markup.Escape(AppName)}[/]");
            grid.AddRow($"[grey]Version:[/] [green]{Markup.Escape(_appVersion)}[/]");
            grid.AddRow($"[grey]Action:[/] [yellow]{Markup.Escape(_action)}[/]");

            Panel panel = new Panel(grid).Expand()
                .Header("[bold blue]Starting[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(new Style(Color.SteelBlue1))
                .Padding(1, 0, 1, 0);

            return panel;
        }

        public OriginalAppHeaderPrinter SetAction(string action)
        {
            _action = action;

            return this;
        }

        public OriginalAppHeaderPrinter SetAppVersion(string appVersion)
        {
            _appVersion = appVersion;

            return this;
        }
    }
}
