using System.Reflection;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace MyLittleRangeBook.CLI.Console
{
    class SimpleAppHeader : IConsolePrinter
    {
        const string AppName = "MyLittleRangeBook CLI";

        string _appVersion = FileExtensions.SimpleAssemblyVersionInformation();

        public SimpleAppHeader()
        {

        }

        public void Print(IAnsiConsole console)
        {
            console.Write(BuildRenderable());
        }

        public IRenderable BuildRenderable()
        {
            var grid = new Grid();
            grid.AddColumn();
            grid.AddRow($"[bold white]{Markup.Escape(AppName)} {_appVersion}[/]");

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
            _appVersion = appVersion;

            return this;
        }


        static string GetInformationalVersion()
        {
            string v = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? "Unknown";


            return v;
        }
    }
}
