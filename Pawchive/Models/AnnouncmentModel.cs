using System;
using System.Text.Json.Serialization;

namespace Pawchive.Models;

internal class AnnouncementModel
{
	[JsonPropertyName("service")]
	public string Service { get; init; } = "";

	[JsonPropertyName("user_id")]
	public string UserId { get; init; } = "";

	[JsonPropertyName("hash")]
	public string Hash { get; init; } = "";

	[JsonPropertyName("content")]
	public string Content { get; init; } = "";

	[JsonPropertyName("added")]
	public DateTime Added { get; init; }
}