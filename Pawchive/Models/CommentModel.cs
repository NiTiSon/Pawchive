using System;
using System.Text.Json.Serialization;

namespace Pawchive.Models;

internal record CommentModel
{
	[JsonPropertyName("id")] public string Id { get; init; } = "";

	[JsonPropertyName("parent_id")] public string? ParentId { get; init; }

	[JsonPropertyName("commenter")] public string Commenter { get; init; } = "";

	[JsonPropertyName("content")] public string Content { get; init; } = "";

	[JsonPropertyName("published")] public DateTime Published { get; init; }

	[JsonPropertyName("revisions")] public CommentRevisionModel[] Revisions { get; init; } = [];
}