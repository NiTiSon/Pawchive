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

	/// <summary>
	/// Filename. The API omits <c>name</c> for a small number of attachments, so this is
	/// never null but may be empty.
	/// </summary>
	public string Name => _model.Name ?? "";

	/// <summary>
	/// Storage path relative to the data host. Empty for deferred attachments, which are only
	/// reachable through <see cref="Url"/> until the API materializes them.
	/// </summary>
	public string RelativePath => _model.Path ?? "";

	/// <summary>Node id the API assigned to this attachment, or null when it did not send one.</summary>
	public int? Node => _model.Node;

	/// <summary>True when the file has no <see cref="RelativePath"/> yet and is served from a temporary URL.</summary>
	public bool Deferred => _model.Deferred;

	/// <summary>True when only a low-resolution preview is available rather than the original file.</summary>
	public bool PreviewOnly => _model.PreviewOnly;

	/// <summary>Temporary stream URL for a deferred attachment. Null for ordinary attachments.</summary>
	public string? TempUrl => _model.TempUrl;

	/// <summary>Temporary direct file URL for a deferred video. Null when the API did not send one.</summary>
	public string? TempDownloadUrl => _model.TempDownloadUrl;

	/// <summary>When <see cref="TempUrl"/> and <see cref="TempDownloadUrl"/> stop working.</summary>
	public DateTime? TempExpires => _model.TempExpires;

	/// <summary>
	/// The URL to fetch this attachment from. Prefers the stable data-host URL built from
	/// <see cref="RelativePath"/>; falls back to the temporary URL for deferred attachments that
	/// have no path yet, and to an empty string when the API supplied neither.
	/// </summary>
	public string Url => !string.IsNullOrEmpty(_model.Path)
		? $"{PawchiveClient.PawchiveDataUrl}{PawchiveClient.DataUrlPrefix}{_model.Path.TrimStart('/')}"
		: _model.TempDownloadUrl ?? _model.TempUrl ?? "";

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