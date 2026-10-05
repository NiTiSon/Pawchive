using System;
using Pawchive.Models;

namespace Pawchive;

public sealed class HashPost
{
	private readonly HashPostModel _model;

	internal HashPost(HashPostModel model)
	{
		_model = model;
	}

	public int FileId => _model.FileId;

	public string Id => _model.Id;

	public Service Service => _model.Service;

	public string CreatorId => _model.User;

	public string Title => _model.Title;

	public string Substring => _model.Substring;

	public DateTime Published => _model.Published;

	public FileAttachment? File => _model.File is null ? null : new FileAttachment(_model.File);

	/// <summary>Attachments attached to this post. Empty when it has none.</summary>
	public FileAttachment[] GetAttachments()
	{
		return FileAttachment.Wrap(_model.Attachments);
	}
}