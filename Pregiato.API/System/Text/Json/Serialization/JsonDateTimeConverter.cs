﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace System.Text.Json.Serialization
{
    public class JsonDateTimeConverter : JsonConverter<DateTime>
    {
        private readonly string[] _formats;

        public JsonDateTimeConverter() : this("dd-MM-yyyy") { }
        public JsonDateTimeConverter(params string[] formats)
        {
            _formats = formats ?? new[] { "dd-MM-yyyy" };
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateString = reader.GetString();
            if (DateTime.TryParseExact(dateString, _formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                return date;
            }
            throw new JsonException($"Formato de data inválido: {dateString}. Formatos aceitos: {string.Join(", ", _formats)}");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_formats[0]));
        }
    }
}