using Spectre.Console;
using Spectre.Console.Rendering;

namespace MyLittleRangeBook.CLI.Console
{
    /// <summary>
    ///     An interface for classes that can print to the console. This is used to decouple the console printing logic from
    ///     the rest of the application, and to allow for different implementations of console printing (e.g. for testing
    ///     purposes).
    /// </summary>
    public interface IConsolePrinter
    {
        void Print(IAnsiConsole console);
        IRenderable BuildRenderable();
    }
}
