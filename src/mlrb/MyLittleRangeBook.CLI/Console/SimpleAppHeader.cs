using MyLittleRangeBook.IO;
using Spectre.Console.Rendering;

namespace MyLittleRangeBook.Console
{
    public class SimpleAppHeader : IConsolePrinter, ICommandHeaderPrinter
    {
        protected string? Action;
        protected string? AppVersion;


        public ICommandHeaderPrinter SetAction(string? action)
        {
            Action = string.IsNullOrWhiteSpace(action) ? null : action.Trim();

            return this;
        }

        public virtual void Print(IAnsiConsole console)
        {
            console.Write(BuildRenderable());
        }

        public IRenderable BuildRenderable()
        {
            string v = AppVersion ?? GetType().Assembly.GetAssemblyVersionInformation();

            var grid = new Grid().Expand();
            grid.AddColumn();
            grid.AddRow($"[bold deepskyblue3]{Markup.Escape(AnsiConsoleExtensions.AppName)} {v}[/]");
            if (!string.IsNullOrEmpty(Action))
            {
                grid.AddRow($"[bold cadetblue_1]{Markup.Escape(Action)}[/]");
            }


            Panel panel = new Panel(grid)
                .Expand()
                .Padding(0, 1, 0, 0)
                .Border(BoxBorder.None);

            Action = null;

            return panel;
        }

        public SimpleAppHeader SetAppVersion(string appVersion)
        {
            AppVersion = appVersion.Trim();

            return this;
        }
    }
}
