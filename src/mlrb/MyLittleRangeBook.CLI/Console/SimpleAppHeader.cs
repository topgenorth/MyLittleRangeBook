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
            _appVersion = SimpleAssemblyVersionInformation();
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


        internal string SimpleAssemblyVersionInformation()
        {
            string v = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? "Unknown";

            if (v.Contains('+'))
            {
                string[] versionParts= v.Split('+');
                string version = versionParts[0];
                string afterPlus = versionParts[1];

                string[] shaParts = afterPlus.Split('.');

                if (shaParts.Length >= 2)
                {
                    return version + "+" + shaParts[0];
                }

                return version;

            }
            else
            {
                return v;
            }
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


        static string GetInformationalVersion()
        {
            string v = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? "Unknown";

            // 0.9.0+0e971a3.0e971a30e99d9114d2f90ca38b6feab611685ac0
            return v;
        }
    }
}
