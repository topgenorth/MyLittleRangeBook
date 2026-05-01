using System.Reflection;

namespace MyLittleRangeBook
{
    public static class AssemblyExtensions
    {
        public static async Task<string> ReadEmbeddedTextFileAsync(this Assembly assembly, string fullResourceName)
        {
            using var stream = assembly.GetManifestResourceStream(fullResourceName);
            if (stream == null)
            {
                throw new FileNotFoundException($"Embedded resource '{fullResourceName}' not found.");
            }

            using var reader = new StreamReader(stream);

            return await reader.ReadToEndAsync();
        }
    }
}
