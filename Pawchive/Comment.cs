using System;
using Pawchive.Models;

namespace Pawchive;

public sealed class Comment
{
	private readonly CommentModel _model;

	internal Comment(CommentModel model)
	{
		_model = model;
	}

	public string Id => _model.Id;

	public string? ParentId => _model.ParentId;

	public string Commenter => _model.Commenter;

	public string Content => _model.Content;

	public DateTime Published => _model.Published;

	public CommentRevision[] GetRevisions()
	{
		CommentRevisionModel[] models = _model.Revisions;

		if (models.Length == 0)
		{
			return [];
		}

		CommentRevision[] result = new CommentRevision[models.Length];
		for (int i = 0; i < models.Length; i++)
		{
			result[i] = new CommentRevision(models[i]);
		}

		return result;
	}
}