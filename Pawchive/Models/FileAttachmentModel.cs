using System.Text.Json.Serialization;

namespace Pawchive.Models;

internal record FileAttachmentModel
{
	[JsonPropertyName("name")] public string Name { get; init; } = "";

	[JsonPropertyName("path")] public string Path { get; init; } = "";
}