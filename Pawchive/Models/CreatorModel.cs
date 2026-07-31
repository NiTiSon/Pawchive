using System;
using System.Text.Json.Serialization;

namespace Pawchive.Models;

internal record CreatorModel
{
	[JsonPropertyName("id")]
	public string Id { get; init; } = "";

	[JsonPropertyName("name")]
	public string Name { get; init; } = "";

	[JsonPropertyName("service")]
	public string Service { get; init; } = "";

	[JsonPropertyName("indexed")]
	public long Indexed { get; init; }

	[JsonPropertyName("updated")]
	public long Updated { get; init; }

	[JsonPropertyName("favorited")]
	public int Favorited { get; init; }

	[JsonPropertyName("ever_imported")]
	public bool? EverImported { get; init; }

	[JsonPropertyName("public_id")]
	public string? PublicId { get; init; }
}