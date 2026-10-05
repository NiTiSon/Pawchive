using System;
using System.Text.Json.Serialization;

namespace Pawchive.Models;

internal record HashPostModel
{
	[JsonPropertyName("file_id")] public int FileId { get; init; }

	[JsonPropertyName("id")] public string Id { get; init; } = "";

	[JsonPropertyName("user")] public string User { get; init; } = "";

	[JsonPropertyName("service")] public string Service { get; init; } = "";

	[JsonPropertyName("title")] public string Title { get; init; } = "";

	[JsonPropertyName("substring")] public string Substring { get; init; } = "";

	[JsonPropertyName("published")] public DateTime Published { get; init; }

	[JsonPropertyName("file")] public FileAttachmentModel? File { get; init; }

	[JsonPropertyName("attachments")] public FileAttachmentModel[] Attachments { get; init; } = [];
}