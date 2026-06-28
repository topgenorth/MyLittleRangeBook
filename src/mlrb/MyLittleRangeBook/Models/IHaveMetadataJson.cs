using System.Text.Json.Serialization;

namespace MyLittleRangeBook.Models
{
    public interface IHaveMetadataJson
    {
        /// <summary>
        /// Gets or sets the JSON string representation of metadata.
        /// This property is ignored during JSON serialization and deserialization.
        /// </summary>
        [JsonIgnore]
        string? MetadataJson { get; set; }
    }
}