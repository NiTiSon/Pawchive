using System.Text.Json.Serialization;

namespace Pawchive.Models;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(CreatorModel))]
[JsonSerializable(typeof(PostModel))]
[JsonSerializable(typeof(FileAttachmentModel))]
internal partial class PawchiveJsonSerializationContext : JsonSerializerContext;