using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PeterPedia.Shared
{
    public class JsonTimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? value = reader.GetString();

            if (string.IsNullOrWhiteSpace(value))
            {
                return TimeSpan.FromSeconds(0);
            }

            if (TimeSpan.TryParseExact(value, "c", CultureInfo.InvariantCulture, out TimeSpan timeSpan))
            {
                return timeSpan;
            }

            return TimeSpan.FromSeconds(0);
        }


        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteStringValue(value.ToString("c", CultureInfo.InvariantCulture));
        }
    }
}
