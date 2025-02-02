using System.Text.Json;
using System.Text.Json.Serialization;

namespace HedgeModManager.UI.Converters.Json;

public class StringDoubleConverter : JsonConverter<double?>
{
    public override double? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string? str = reader.GetString();
            if (double.TryParse(str, out double valueDouble))
                return valueDouble;
            throw new JsonException($"Could not convert \"{str}\" to double type");
        }
        else if (reader.TokenType == JsonTokenType.Number)
            return reader.GetDouble();
        return null;
    }

    public override void Write(Utf8JsonWriter writer, double? value, JsonSerializerOptions options)
    {
        if (value is double valueDouble)
            writer.WriteNumberValue(valueDouble);
    }
}
