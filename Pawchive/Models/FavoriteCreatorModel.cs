using System;
using System.Text.Json.Serialization;

namespace Pawchive.Models;

internal record FavoriteCreatorModel
{
	[JsonPropertyName("faved_seq")] public int Sequence { get; init; }

	[JsonPropertyName("id")] public string Id { get; init; } = "";

	[JsonPropertyName("name")] public string Name { get; init; } = "";

	[JsonPropertyName("service")] public string Service { get; init; } = "";

	// The API sends timestamps inconsistently (unix seconds or ISO-8601), so these are normalised.
	[JsonPropertyName("indexed")]
	[JsonConverter(typeof(FlexibleDateTimeConverter))]
	public DateTime Indexed { get; init; }

	[JsonPropertyName("updated")]
	[JsonConverter(typeof(FlexibleDateTimeConverter))]
	public DateTime Updated { get; init; }

	/// <summary>default when the creator has never been imported.</summary>
	[JsonPropertyName("last_imported")]
	[JsonConverter(typeof(FlexibleDateTimeConverter))]
	public DateTime LastImported { get; init; }

	// Only present when listing favorited posts.
	[JsonPropertyName("user")] public string? User { get; init; }

	[JsonPropertyName("creator_id")] public string? CreatorId { get; init; }

	[JsonPropertyName("title")] public string? Title { get; init; }
}