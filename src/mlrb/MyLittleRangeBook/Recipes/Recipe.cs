using System.Text.Json.Serialization;

namespace MyLittleRangeBook.Recipes
{
    /// <summary>
    ///     Represents a reloading recipe that combines various components required for creating
    ///     ammunition. This includes details about the cartridge, projectile, powder, case, and
    ///     measurements such as COAL (Cartridge Overall Length) and CBTO (Cartridge Base To Ogive),
    ///     as well as metadata and additional comments.
    /// </summary>
    public class Recipe
    {
        public Recipe()
        {
            Guid id = Guid.CreateVersion7();
            Id            = id;
            CorrelationId = id;
            Headers       = string.Empty;
            CausationId   = id;
        }

        public required Guid Id            { get; set; }
        public required Guid CorrelationId { get; set; }
        public required Guid CausationId   { get; set; }

        public required string Headers { get; set; }

        /// <summary>
        ///     The name of the cartridge this recipe is for.
        /// </summary>
        [JsonPropertyName("cartridge")]
        public required CartridgeValues Cartridge { get; set; }

        /// <summary>
        ///     Specifies the projectile used in the recipe, including information such as
        ///     manufacturer, type, weight, and measurement units.
        /// </summary>
        [JsonPropertyName("projectile")]
        public required ProjecticleValues Projectile { get; set; }

        /// <summary>
        ///     Represents the powder used in the recipe, including its manufacturer, type, weight, units, and a unique value
        ///     identifier.
        /// </summary>
        [JsonPropertyName("powder")]
        public required PowderValues Powder { get; set; }

        /// <summary>
        ///     Represents the coal used in the recipe.
        /// </summary>
        [JsonPropertyName("coal")]
        public required CoalValues COAL { get; set; }

        /// <summary>
        ///     Represents the cbto used in the recipe.
        /// </summary>
        [JsonPropertyName("cbto")]
        public required CbtoValues CBTO { get; set; }

        /// <summary>
        ///     Represents the case used in the recipe.
        /// </summary>
        [JsonPropertyName("case")]
        public required CaseValues Case { get; set; }

        /// <summary>
        ///     Represents comments about the recipe.
        /// </summary>
        [JsonPropertyName("comments")]
        public required string Comments { get; set; }

        /// <summary>
        ///     Represents metadata about the recipe.
        /// </summary>
        [JsonPropertyName("metadata")]
        public required MetadataValues Metadata { get; set; }

        /// <summary>
        ///     The time this document was created.
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        ///     The time this document was last modified.
        /// </summary>
        public DateTimeOffset Modified { get; set; }

        /// <summary>
        ///     Represents a cartridge used in a reloading recipe. A cartridge typically includes
        ///     information about the specific type or name of the ammunition cartridge.
        /// </summary>
        public class CartridgeValues
        {
            [JsonPropertyName("name")] public required string Name { get; set; }
        }

        /// <summary>
        ///     Represents the properties of a projectile used in a reloading recipe. This encompasses
        ///     details about the projectile's manufacturer, type, weight, and units of measurement.
        /// </summary>
        public class ProjecticleValues
        {
            [JsonPropertyName("manufacturer")] public required string  Manufacturer { get; set; }
            [JsonPropertyName("type")]         public required string  Type         { get; set; }
            [JsonPropertyName("weight")]       public required decimal Weight       { get; set; }
            [JsonPropertyName("units")]        public required string  Units        { get; set; }
        }

        /// <summary>
        ///     Represents details about the powder used in a reloading recipe, including
        ///     information about the manufacturer, type, weight, and units. Additionally,
        ///     this includes a unique value to identify or categorize the powder.
        /// </summary>
        public class PowderValues
        {
            [JsonPropertyName("manufacturer")] public required string  Manufacturer { get; set; }
            [JsonPropertyName("type")]         public required string  Type         { get; set; }
            [JsonPropertyName("weight")]       public required decimal Weight       { get; set; }
            [JsonPropertyName("units")]        public required string  Units        { get; set; }
            /// <summary>
            /// The value of the UniqueTek powder bar measure for the Dillon powder dispenser.
            /// </summary>
            [JsonPropertyName("uniquetek_value")] public required int UniqueTekValue { get; set; }
        }

        /// <summary>
        ///     Represents the Cartridge Overall Length (COAL) values, which include the measurement of
        ///     the total length of a loaded cartridge and the unit of measurement. This is a critical
        ///     specification in ammunition reloading to ensure proper fit and functionality.
        /// </summary>
        public class CoalValues
        {
            [JsonPropertyName("value")] public required decimal Value { get; set; }
            [JsonPropertyName("unit")]  public required string  Unit  { get; set; }
        }

        /// <summary>
        ///     Represents the Case Base Diameter (CBD) values, which include the measurement of
        ///     the diameter of the case base and the unit of measurement. This is a critical
        ///     specification in ammunition reloading to ensure proper fit and functionality.
        /// </summary>
        public class CbtoValues
        {
            [JsonPropertyName("value")] public required decimal Value { get; set; }
            [JsonPropertyName("unit")]  public required string  Unit  { get; set; }
        }

        /// <summary>
        ///     Represents the properties and specifications of a cartridge case used in a reloading recipe.
        ///     Includes details such as the trim length, units of measurement, manufacturer, and primer type.
        /// </summary>
        public class CaseValues
        {
            [JsonPropertyName("trim_length")] public required decimal TrimLength { get; set; }

            [JsonPropertyName("trim_length_units")]
            public required string TrimLengthUnits { get; set; }

            [JsonPropertyName("manufacturer")] public required string Manufacturer { get; set; }

            [JsonPropertyName("primer")] public required string Primer { get; set; }
        }

        /// <summary>
        ///     Encapsulates additional metadata related to a recipe, including details about the original
        ///     input string, the processing model used, and the time it took to process the request in seconds.
        /// </summary>
        public class MetadataValues
        {
            [JsonPropertyName("original_string")] public required string OriginalString { get; set; }

            [JsonPropertyName("model")] public required string Model { get; set; }

            [JsonPropertyName("request_time_seconds")]
            public required double RequestTimeSeconds { get; set; }
        }
    }
}