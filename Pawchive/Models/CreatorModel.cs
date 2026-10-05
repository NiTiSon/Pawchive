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

	// /creators sends unix seconds, /profile and /links send ISO-8601. See FlexibleDateTimeConverter.
	[JsonPropertyName("indexed")]
	[JsonConverter(typeof(FlexibleDateTimeConverter))]
	public DateTime Indexed { get; init; }

	[JsonPropertyName("updated")]
	[JsonConverter(typeof(FlexibleDateTimeConverter))]
	public DateTime Updated { get; init; }

	[JsonPropertyName("favorited")]
	public int Favorited { get; init; }

	[JsonPropertyName("ever_imported")]
	public bool? EverImported { get; init; }

	[JsonPropertyName("public_id")]
	public string? PublicId { get; init; }

	/// <summary>Only present on /profile and /links, where the caller has a session.</summary>
	[JsonPropertyName("relation_id")]
	public string? RelationId { get; init; }
}