using System;
using Pawchive.Models;

namespace Pawchive;

public sealed class FileAttachment
{
	private readonly FileAttachmentModel _model;

	internal FileAttachment(FileAttachmentModel model)
	{
		_model = model;
	}

	public string Name => _model.Name;

	public string RelativePath => _model.Path;

	/// <summary>Node id the API assigned to this attachment, or null when it did not send one.</summary>
	public int? Node => _model.Node;

	public string Url => $"{PawchiveClient.PawchiveDataUrl}{PawchiveClient.DataUrlPrefix}{_model.Path.TrimStart('/')}";

	/// <summary>
	/// Wraps attachment models for the public API. Tolerates a missing array, which the API sends
	/// for posts that carry no attachments.
	/// </summary>
	internal static FileAttachment[] Wrap(FileAttachmentModel[]? models)
	{
		if (models is null || models.Length == 0)
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