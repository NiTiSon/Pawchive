using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pawchive.Models;

internal record PostRevisionModel
{
	[JsonPropertyName("revision_id")] public int RevisionId { get; init; }

	[JsonPropertyName("id")] public string Id { get; init; } = "";

	[JsonPropertyName("user")] public string User { get; init; } = "";

	[JsonPropertyName("service")] public string Service { get; init; } = "";

	[JsonPropertyName("title")] public string Title { get; init; } = "";

	[JsonPropertyName("content")] public string? Content { get; init; }

	[JsonPropertyName("embed")] public JsonElement Embed { get; init; }

	[JsonPropertyName("shared_file")] public bool SharedFile { get; init; }

	[JsonPropertyName("added")] public DateTime Added { get; init; }

	[JsonPropertyName("published")] public DateTime Published { get; init; }

	[JsonPropertyName("edited")] public DateTime Edited { get; init; }

	[JsonPropertyName("file")] public FileAttachmentModel? File { get; init; }

	[JsonPropertyName("attachments")] public FileAttachmentModel[] Attachments { get; init; } = [];
}