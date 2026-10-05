using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pawchive.Models;

internal record PostModel
{
	[JsonPropertyName("id")] public string Id { get; init; } = "";

	[JsonPropertyName("user")] public string User { get; init; } = "";

	[JsonPropertyName("service")] public string Service { get; init; } = "";

	[JsonPropertyName("title")] public string Title { get; init; } = "";

	[JsonPropertyName("content")] public string? Content { get; init; }

	[JsonPropertyName("substring")] public string? Substring { get; init; }

	[JsonPropertyName("embed")] public JsonElement Embed { get; init; }

	[JsonPropertyName("shared_file")] public bool SharedFile { get; init; }

	[JsonPropertyName("added")] public DateTime Added { get; init; }

	[JsonPropertyName("published")] public DateTime Published { get; init; }

	[JsonPropertyName("edited")] public DateTime? Edited { get; init; }

	[JsonPropertyName("file")] public FileAttachmentModel? File { get; init; }

	[JsonPropertyName("attachments")] public FileAttachmentModel[] Attachments { get; init; } = [];

	[JsonPropertyName("preview_state")] public string? PreviewState { get; init; }

	[JsonPropertyName("has_full")] public bool? HasFull { get; init; }

	[JsonPropertyName("tags")][JsonConverter(typeof(TagsConverter))] public string[] Tags { get; init; } = [];

	[JsonPropertyName("origin")] public string? Origin { get; init; }

	[JsonPropertyName("next")] public string? Next { get; init; }

	[JsonPropertyName("prev")] public string? Prev { get; init; }
}