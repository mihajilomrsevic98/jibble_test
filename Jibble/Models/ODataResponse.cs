namespace Jibble.Models
{
    using System.Text.Json.Serialization;

    public class ODataResponse<T>
    {
        // Ignore the OData context metadata
        [JsonPropertyName("@odata.context")]
        public string ODataContext { get; set; } = string.Empty;

        // This property holds the actual list of records
        [JsonPropertyName("value")]
        public T Value { get; set; } = default!;
    }
}
