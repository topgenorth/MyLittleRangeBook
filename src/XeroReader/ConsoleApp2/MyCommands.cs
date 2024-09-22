using ConsoleAppFramework;

namespace ConsoleApp2;

public class MyCommands
{
    /// <summary>
    ///     Root command test
    /// </summary>
    /// <param name="message">-m, message to show  </param>
    [Command("")]
    public void Root(string message)
    {
        Console.WriteLine(message);
    }

    /// <summary>
    ///     Display message
    /// </summary>
    /// <param name="message">Message to show</param>
    public void Echo(string message)
    {
        Console.WriteLine(message);
    }


    /// <summary>
    ///     Sum two numbers
    /// </summary>
    /// <param name="x">left value</param>
    /// <param name="y">right value</param>
    public void Sum(int x, int y)
    {
        Console.WriteLine(x + y);
    }
}