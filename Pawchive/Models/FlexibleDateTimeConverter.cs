using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pawchive.Models;

/// <summary>
/// The API is inconsistent about timestamps: <c>/creators</c> sends unix seconds as a JSON number,
/// while <c>/profile</c> and <c>/links</c> send ISO-8601 strings. This accepts either form and
/// normalises to a UTC <see cref="DateTime"/>.
/// </summary>
internal sealed class FlexibleDateTimeConverter : JsonConverter<DateTime>
{
	public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		switch (reader.TokenType)
		{
			case JsonTokenType.Number:
				return DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64()).UtcDateTime;

			case JsonTokenType.String:
				return ParseString(reader.GetString());

			case JsonTokenType.Null:
				return default;

			default:
				throw new JsonException($"Cannot read a timestamp from a {reader.TokenType} token.");
		}
	}

	public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value);
	}

	private static DateTime ParseString(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return default;
		}

		if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long seconds))
		{
			return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
		}

		if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out DateTime moment))
		{
			return moment;
		}

		return default;
	}
}