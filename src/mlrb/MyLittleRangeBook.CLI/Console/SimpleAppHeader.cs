using System.Reflection;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace MyLittleRangeBook.CLI.Console
{
    class SimpleAppHeader : IConsolePrinter
    {
        const string AppName = "MyLittleRangeBook CLI";

        readonly string _appVersion = string.Empty;

        public SimpleAppHeader()
        {
            _appVersion = SimplAssemblyVersionInformation();
        }


        internal string SimplAssemblyVersionInformation()
        {
            string v = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? "Unknown";

            return v;
        }

        public SimpleAppHeader SetAction(string action)
        {
            // [TO20260425] NOOP

            return this;
        }

        public SimpleAppHeader SetAppVersion(string appVersion)
        {
            // [TO20260425] NOOP

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
