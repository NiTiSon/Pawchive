using System.Text.Json.Serialization;

namespace Pawchive.Models;

internal record struct ErrorModel
{
	[JsonPropertyName("error")]
	public string Message { get; init; }
}