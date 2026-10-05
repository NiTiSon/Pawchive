using System;
using System.Text.Json.Serialization;

namespace Pawchive.Models;

internal class FancardModel
{
	[JsonPropertyName("id")]
	public int Id { get; init; }

	[JsonPropertyName("user_id")]
	public string UserId { get; init; } = "";

	[JsonPropertyName("file_id")]
	public int FileId { get; init; }

	[JsonPropertyName("hash")]
	public string Hash { get; init; } = "";

	[JsonPropertyName("mtime")]
	public DateTime MTime { get; init; }

	[JsonPropertyName("ctime")]
	public DateTime CTime { get; init; }

	[JsonPropertyName("mime")]
	public string Mime { get; init; } = "";

	[JsonPropertyName("ext")]
	public string Ext { get; init; } = "";

	[JsonPropertyName("added")]
	public DateTime Added { get; init; }

	[JsonPropertyName("size")]
	public long Size { get; init; }

	[JsonPropertyName("ihash")]
	public string? IHash { get; init; }
}