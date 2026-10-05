using System;
using Pawchive.Models;

namespace Pawchive;

public sealed class CommentRevision
{
	private readonly CommentRevisionModel _model;

	internal CommentRevision(CommentRevisionModel model)
	{
		_model = model;
	}

	public int Id => _model.Id;

	public string Content => _model.Content;

	public DateTime Added => _model.Added;
}