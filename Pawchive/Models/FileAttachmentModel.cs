using System;
using System.Text.Json.Serialization;

namespace Pawchive.Models;

internal record FileAttachmentModel
{
	[JsonPropertyName("name")] public string? Name { get; init; }

	[JsonPropertyName("path")] public string? Path { get; init; }

	[JsonPropertyName("node")] public int? Node { get; init; }

	/// <summary>Set when the file has no <c>path</c> yet and can only be fetched via a temp URL.</summary>
	[JsonPropertyName("deferred")] public bool Deferred { get; init; }

	/// <summary>Set when only a low-resolution preview is available rather than the original file.</summary>
	[JsonPropertyName("preview_only")] public bool PreviewOnly { get; init; }

	[JsonPropertyName("temp_url")] public string? TempUrl { get; init; }

	/// <summary>Direct file URL for deferred video attachments; falls back to <c>temp_url</c> when absent.</summary>
	[JsonPropertyName("temp_download_url")] public string? TempDownloadUrl { get; init; }

	[JsonPropertyName("temp_expires")] public DateTime? TempExpires { get; init; }
}