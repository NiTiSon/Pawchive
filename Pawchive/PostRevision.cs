using System;
using Pawchive.Models;

namespace Pawchive;

public sealed class PostRevision
{
	private readonly PostRevisionModel _model;

	internal PostRevision(PostRevisionModel model)
	{
		_model = model;
	}

	public int RevisionId => _model.RevisionId;

	public string Id => _model.Id;

	public Service Service => _model.Service;

	public string CreatorId => _model.User;

	public string Title => _model.Title;

	public string? Content => _model.Content;

	public bool SharedFile => _model.SharedFile;

	public DateTime Added => _model.Added;

	public DateTime Published => _model.Published;

	public DateTime Edited => _model.Edited;

	public FileAttachment? File => _model.File is null ? null : new FileAttachment(_model.File);

	public FileAttachment[] GetAttachments()
	{
		return FileAttachment.Wrap(_model.Attachments);
	}
}