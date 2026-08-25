namespace DevConnect.Api.Config
{
    /// <summary>
    /// Holds CognoDB connection settings. Values are populated from
    /// environment variables at startup (see Program.cs) — never
    /// from appsettings.json, so nothing sensitive is committed.
    /// </summary>
    public class CognoDbSettings
    {
        public string Uri { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public static CognoDbSettings FromEnvironment()
        {
            return new CognoDbSettings
            {
                Uri = Environment.GetEnvironmentVariable("COGNODB_URI")
                      ?? throw new InvalidOperationException("COGNODB_URI environment variable is not set."),
                User = Environment.GetEnvironmentVariable("COGNODB_USER")
                       ?? throw new InvalidOperationException("COGNODB_USER environment variable is not set."),
                Password = Environment.GetEnvironmentVariable("COGNODB_PASSWORD")
                           ?? throw new InvalidOperationException("COGNODB_PASSWORD environment variable is not set.")
            };
        }
    }
}
