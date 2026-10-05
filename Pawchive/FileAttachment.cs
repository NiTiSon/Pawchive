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

	public string Path => _model.Path;

	public string Url => $"{PawchiveClient.PawchiveDataUrl}{PawchiveClient.DataUrlPrefix}{_model.Path.TrimStart('/')}";
}