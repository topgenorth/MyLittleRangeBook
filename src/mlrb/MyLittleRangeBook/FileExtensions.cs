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
        public static  string SimpleAssemblyVersionInformation()
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
    }
}
