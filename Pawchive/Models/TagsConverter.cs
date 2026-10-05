using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pawchive.Models;

/// <summary>
/// The API sends post <c>tags</c> in three different shapes for the same field: <c>null</c>, a real
/// JSON array (<c>["tekken"]</c>), or a brace-wrapped list that is double-encoded inside a string
/// (<c>"{\"Bloodborne Eternal Beast\"}"</c> or <c>"{Note,US}"</c>). This normalises all of them to
/// a flat array of tag strings.
/// </summary>
internal sealed class TagsConverter : JsonConverter<string[]>
{
	[RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Deserialize<TValue>(ref Utf8JsonReader, JsonSerializerOptions)")]
	[RequiresDynamicCode("Calls System.Text.Json.JsonSerializer.Deserialize<TValue>(ref Utf8JsonReader, JsonSerializerOptions)")]
	public override string[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		switch (reader.TokenType)
		{
			case JsonTokenType.Null:
				return [];

			case JsonTokenType.StartArray:
				return JsonSerializer.Deserialize<string[]>(ref reader, options) ?? [];

			case JsonTokenType.String:
				return SplitBraceList(reader.GetString());

			default:
				throw new JsonException($"Cannot read tags from a {reader.TokenType} token.");
		}
	}

	public override void Write(Utf8JsonWriter writer, string[] value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();

		foreach (string tag in value)
		{
			writer.WriteStringValue(tag);
		}

		writer.WriteEndArray();
	}

	/// <summary>Turns <c>{Note,US}</c> or <c>{"A B"}</c> into its individual, unquoted tags.</summary>
	private static string[] SplitBraceList(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return [];
		}

		string inner = text.Trim().Trim('{', '}');

		if (inner.Length == 0)
		{
			return [];
		}

		string[] tags = inner
			.Split(',', StringSplitOptions.TrimEntries)
			.Select(tag => tag.Trim().Trim('"').Trim())
			.Where(tag => tag.Length > 0)
			.ToArray();

		return tags;
	}
}