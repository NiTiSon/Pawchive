using System;
using System.Threading;
using System.Threading.Tasks;
using Pawchive.Models;

namespace Pawchive;

public class Announcement
{
	private readonly PawchiveClient _client;
	private readonly AnnouncementModel _model;

	internal Announcement(PawchiveClient client, AnnouncementModel model)
	{
		_client = client;
		_model = model;
	}

	public string Hash => _model.Hash;
	public string Content => _model.Content;
	public DateTime Added => _model.Added;

	public Task<Creator> GetCreator(CancellationToken cancellationToken = default)
	{
		return _client.GetCreatorByIdAsync(_model.Service, _model.UserId, cancellationToken)!;
	}
}