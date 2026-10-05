using System;
using System.Text.Json.Serialization;

namespace Pawchive.Models;

internal record CommentRevisionModel
{
	[JsonPropertyName("id")] public int Id { get; init; }

	[JsonPropertyName("content")] public string Content { get; init; } = "";

	[JsonPropertyName("added")] public DateTime Added { get; init; }
}