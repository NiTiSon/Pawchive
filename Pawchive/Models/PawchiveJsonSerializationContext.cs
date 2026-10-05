using System.Text.Json.Serialization;

namespace Pawchive.Models;

[JsonSerializable(typeof(AnnouncementModel))]
[JsonSerializable(typeof(CommentModel))]
[JsonSerializable(typeof(CommentModel[]))]
[JsonSerializable(typeof(CommentRevisionModel))]
[JsonSerializable(typeof(CreatorModel))]
[JsonSerializable(typeof(CreatorModel[]))]
[JsonSerializable(typeof(ErrorModel))]
[JsonSerializable(typeof(FavoriteCreatorModel))]
[JsonSerializable(typeof(FavoriteCreatorModel[]))]
[JsonSerializable(typeof(FancardModel))]
[JsonSerializable(typeof(FancardModel[]))]
[JsonSerializable(typeof(FileAttachmentModel))]
[JsonSerializable(typeof(FileAttachmentModel[]))]
[JsonSerializable(typeof(FileHashResultModel))]
[JsonSerializable(typeof(HashPostModel))]
[JsonSerializable(typeof(DiscordHashPostModel))]
[JsonSerializable(typeof(PostModel))]
[JsonSerializable(typeof(PostModel[]))]
[JsonSerializable(typeof(PostRevisionModel))]
[JsonSerializable(typeof(PostRevisionModel[]))]
internal partial class PawchiveJsonSerializationContext : JsonSerializerContext;