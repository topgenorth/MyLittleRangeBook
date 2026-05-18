using MyLittleRangeBook.IO;
using Spectre.Console.Rendering;

namespace MyLittleRangeBook.CLI.Console
{
    public class SimpleAppHeader : IConsolePrinter, ICommandHeaderPrinter
    {
        protected string? Action;
        protected string? AppVersion;

        public virtual  void Print(IAnsiConsole console)
        {
            console.Write(BuildRenderable());
        }

        public IRenderable BuildRenderable()
        {
            string v = AppVersion ?? GetType().Assembly.GetAssemblyVersionInformation();

            var grid = new Grid();
            grid.AddColumn();
            grid.AddRow($"[bold white]{Markup.Escape(AnsiConsoleExtensions.AppName)} {v}[/]");
            if (!string.IsNullOrEmpty(Action))
            {
                grid.AddRow($"[bold white]{Markup.Escape(Action)}[/]");
            }


            Panel panel = new Panel(grid)
                .Expand()
                .Padding(0, 1, 0, 0)
                .Border(BoxBorder.None);

            Action = null;
            return panel;
        }


        public ICommandHeaderPrinter SetAction(string? action)
        {
            Action = string.IsNullOrWhiteSpace(action) ? null : action.Trim();
            return this;
        }

        public SimpleAppHeader SetAppVersion(string appVersion)
        {
            AppVersion = appVersion.Trim();
            return this;
        }
    }
}
