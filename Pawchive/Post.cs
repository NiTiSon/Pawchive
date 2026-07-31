using System.Threading.Tasks;
using Pawchive.Models;

namespace Pawchive;

public class Post
{
	private readonly PawchiveClient _client;
	private readonly PostModel _model;

	internal Post(PawchiveClient client, PostModel model)
	{
		_client = client;
		_model = model;
	}

	public string Id => _model.Id;

	public string Title => _model.Title;

	public Task<Post?> GetNextPostAsync()
	{
		if (string.IsNullOrWhiteSpace(_model.Next))
		{
			return Task.FromResult<Post?>(null);
		}

		return _client.GetPostByIdAsync(_model.Service, _model.User, _model.Next);
	}

	public Task<Post?> GetPreviousPostAsync()
	{
		if (string.IsNullOrWhiteSpace(_model.Prev))
		{
			return Task.FromResult<Post?>(null);
		}

		return _client.GetPostByIdAsync(_model.Service, _model.User, _model.Prev);
	}
}