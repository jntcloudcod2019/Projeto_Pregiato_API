﻿using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

public class DecimalJsonConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetDecimal(); 
        }
        else if (reader.TokenType == JsonTokenType.String)
        {
            string numberString = reader.GetString();
            if (string.IsNullOrWhiteSpace(numberString))
                throw new JsonException("Valor decimal inválido.");

            numberString = numberString.Replace(".", "").Replace(",", ".");

            if (decimal.TryParse(numberString, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }

            throw new JsonException($"Não foi possível converter '{numberString}' para decimal.");
        }
        throw new JsonException("Valor numérico esperado.");
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("N2", new CultureInfo("pt-BR")));
    }
}
