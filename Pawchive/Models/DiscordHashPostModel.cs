using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pawchive.Models;

internal record DiscordHashPostModel
{
	[JsonPropertyName("file_id")] public int FileId { get; init; }

	[JsonPropertyName("id")] public string Id { get; init; } = "";

	[JsonPropertyName("server")] public string Server { get; init; } = "";

	[JsonPropertyName("channel")] public string Channel { get; init; } = "";

	[JsonPropertyName("substring")] public string Substring { get; init; } = "";

	[JsonPropertyName("published")] public DateTime Published { get; init; }

	[JsonPropertyName("embeds")] public JsonElement[] Embeds { get; init; } = [];

	[JsonPropertyName("mentions")] public JsonElement[] Mentions { get; init; } = [];

	[JsonPropertyName("attachments")] public FileAttachmentModel[] Attachments { get; init; } = [];
}