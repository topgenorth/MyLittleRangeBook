using System.Text.Json.Nodes;

namespace MyLittleRangeBook.EventSourcing
{
    public class MetaData
    {
        readonly Dictionary<string, string> _metaData = new();

        public MetaData Add(string key, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                _metaData[key] = value;
            }
            return this;
        }

        public override string ToString()
        {
            if (_metaData.Count == 0)
            {
                return "{}";
            }

            var json = new JsonObject();
            foreach (var kvp in _metaData)
            {
                json[kvp.Key] = kvp.Value;
            }

            return json.ToString();
        }
    }
}