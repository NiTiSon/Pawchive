using System;
using Pawchive.Models;

namespace Pawchive;

public sealed class DiscordHashPost
{
	private readonly DiscordHashPostModel _model;

	internal DiscordHashPost(DiscordHashPostModel model)
	{
		_model = model;
	}

	public int FileId => _model.FileId;

	public string Id => _model.Id;

	public string Server => _model.Server;

	public string Channel => _model.Channel;

	public string Substring => _model.Substring;

	public DateTime Published => _model.Published;

	public FileAttachment[] GetAttachments()
	{
		FileAttachmentModel[] models = _model.Attachments;

		if (models.Length == 0)
		{
			return [];
		}

		FileAttachment[] result = new FileAttachment[models.Length];
		for (int i = 0; i < models.Length; i++)
		{
			result[i] = new FileAttachment(models[i]);
		}

		return result;
	}
}