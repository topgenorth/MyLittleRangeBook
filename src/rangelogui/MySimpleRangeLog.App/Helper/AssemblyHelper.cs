using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace MySimpleRangeLog.Helper
{
    public static class AssemblyHelper
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
