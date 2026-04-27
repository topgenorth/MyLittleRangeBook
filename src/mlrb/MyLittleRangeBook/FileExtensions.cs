using System.Reflection;

namespace MyLittleRangeBook
{
    public static class FileExtensions
    {
        /// <summary>
        /// Will retrieve the app info version from the executing assembly.
        /// </summary>
        /// <remarks>Will clean up information versions that include the full SHA, they look like 0.9.0+0e971a3.0e971a30e99d9114d2f90ca38b6feab611685ac0</remarks>
        /// <returns></returns>
        public static string GetAssemblyVersionInformation(this Assembly assembly)
        {
            string v = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? "Unknown";


            return RemoveFullGitShaFromInformationalVersion(v);

        }

        public static string RemoveFullGitShaFromInformationalVersion(string? v)
        {
            if ("unknown".Equals(v, StringComparison.OrdinalIgnoreCase))
            {
                return v;
            }
            if (string.IsNullOrWhiteSpace(v))
            {
                return v ?? string.Empty;
            }

            if (!v.Contains('+'))
            {
                return v;
            }

            string[] versionParts= v.Split('+');
            if (versionParts.Length < 2)
            {
                return v;
            }

            string version = versionParts[0];
            string afterPlus = versionParts[1];

            string[] shaParts = afterPlus.Split('.');

            if (shaParts.Length >0)
            {
                return version + "+" + shaParts[0];
            }

            return version;
        }

    }
}
