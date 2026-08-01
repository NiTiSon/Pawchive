using System;
using System.Threading;
using System.Threading.Tasks;
using Pawchive.Models;

namespace Pawchive;

public class Fancard
{
	private readonly PawchiveClient _client;
	private readonly FancardModel _model;

	internal Fancard(PawchiveClient client, FancardModel model)
	{
		_client = client;
		_model = model;
	}

	public string MimeType => _model.Mime;
	public string Extension => _model.Ext;
	public DateTime Changed => _model.CTime;
	public DateTime Modified => _model.MTime;
	public DateTime Added => _model.Added;
	public long Size => _model.Size;
	public string Hash => _model.Hash;
	public int FileId => _model.FileId;
	public int Id => _model.Id;

	public Task<Creator> GetCreator(CancellationToken cancellationToken = default)
	{
		return _client.GetCreatorByIdAsync(Service.PixivFanbox, _model.UserId, cancellationToken)!;
	}
}