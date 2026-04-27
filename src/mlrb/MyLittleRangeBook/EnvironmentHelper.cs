namespace MyLittleRangeBook
{
    public static class EnvironmentHelper
    {
        public static bool IsDevelopment => "Development".Equals(
            Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"),
            StringComparison.OrdinalIgnoreCase);

        public static bool IsStaging => "Staging".Equals(Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"),
            StringComparison.OrdinalIgnoreCase);

        public static bool IsProduction
        {
            get
            {
                string env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "";

                return string.IsNullOrWhiteSpace(env) || env.Equals("Production", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        ///     Will inject the environment into the file name if the environment is not production.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns>
        ///     A new FileInfo object with the environment injected (if relevant) or the original FileInfo object if it is
        ///     not.
        /// </returns>
        public static FileInfo InjectEnvironmentIntoFileName(this FileInfo fileInfo)
        {
            if (EnvironmentHelper.IsProduction)
            {
                return fileInfo;
            }

            string env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(env))
            {
                return fileInfo;
            }

            string path = fileInfo.DirectoryName! ?? string.Empty;
            string name = Path.GetFileNameWithoutExtension(fileInfo.FullName);
            string ext = fileInfo.Extension;

            var newName = $"{name}-{env}{ext}";

            string s = Path.Combine(path, newName);

            return new FileInfo(s);
        }
    }
}
