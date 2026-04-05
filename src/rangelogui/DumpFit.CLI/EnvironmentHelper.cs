namespace MyLittleRangeBook.Cli
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
                var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "";

                return string.IsNullOrWhiteSpace(env) || env.Equals("Production", StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}