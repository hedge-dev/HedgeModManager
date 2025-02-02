using System.Text.Json;
using System.Text.Json.Serialization;

namespace HedgeModManager.UI.Converters.Json;

public class StringDynamicConverter : JsonConverter<dynamic?>
{
    public override dynamic? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string? str = reader.GetString();

            if (int.TryParse(str, out int valueInt))
                return valueInt;
            if (double.TryParse(str, out double valueDouble))
                return valueDouble;
            if (bool.TryParse(str, out bool valueBool))
                return valueBool;
            return str;
        }
        else if (reader.TokenType == JsonTokenType.Number)
            return reader.GetDouble();
        else if (reader.TokenType == JsonTokenType.True || reader.TokenType == JsonTokenType.False)
            return reader.GetBoolean();
        return null;
    }

    public override void Write(Utf8JsonWriter writer, dynamic? value, JsonSerializerOptions options)
    {
        if (value is double valueDouble)
            writer.WriteNumberValue(valueDouble);
        else if (value is bool valueBool)
            writer.WriteBooleanValue(valueBool);
        else if (value is string valueString)
            writer.WriteStringValue(valueString);
    }
}
