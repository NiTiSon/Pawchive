using System;
using System.Text.Json.Serialization;

namespace Pawchive.Models;

internal record FileHashResultModel
{
	[JsonPropertyName("id")] public int Id { get; init; }

	[JsonPropertyName("hash")] public string Hash { get; init; } = "";

	[JsonPropertyName("mtime")] public string MTime { get; init; } = "";

	[JsonPropertyName("ctime")] public string CTime { get; init; } = "";

	[JsonPropertyName("mime")] public string Mime { get; init; } = "";

	[JsonPropertyName("ext")] public string Ext { get; init; } = "";

	[JsonPropertyName("added")] public DateTime Added { get; init; }

	[JsonPropertyName("size")] public long Size { get; init; }

	[JsonPropertyName("ihash")] public string? IHash { get; init; }

	[JsonPropertyName("posts")] public HashPostModel[] Posts { get; init; } = [];

	[JsonPropertyName("discord_posts")] public DiscordHashPostModel[] DiscordPosts { get; init; } = [];
}