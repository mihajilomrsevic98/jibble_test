namespace Jibble
{
    public class ODataSettings
    {
        public const string SectionName = "ODataService";

        public string BaseUrl { get; set; } = string.Empty;

        public int RequestTimeoutSeconds { get; set; } = 15;
    }
}
