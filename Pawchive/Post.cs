using System.Threading;
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

	public Service Service => _model.Service;

	public string CreatorId => _model.User;

	public string? Content => _model.Content;

	public string? Substring => _model.Substring;

	public bool SharedFile => _model.SharedFile;

	public System.DateTime Added => _model.Added;

	public System.DateTime Published => _model.Published;

	public System.DateTime? Edited => _model.Edited;

	public string[] Tags => _model.Tags ?? [];

	public FileAttachment? File => _model.File is null ? null : new FileAttachment(_model.File);

	public Task<Post?> GetNextPostAsync(CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(_model.Next))
		{
			return Task.FromResult<Post?>(null);
		}

		return _client.GetPostByIdAsync(_model.Service, _model.User, _model.Next, cancellationToken);
	}

	public Task<Post?> GetPreviousPostAsync(CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(_model.Prev))
		{
			return Task.FromResult<Post?>(null);
		}

		return _client.GetPostByIdAsync(_model.Service, _model.User, _model.Prev, cancellationToken);
	}

	public Task<PostRevision[]> GetRevisionsAsync(CancellationToken cancellationToken = default)
	{
		return _client.GetPostRevisionsAsync(_model.Service, _model.User, _model.Id, cancellationToken);
	}

	public Task<Comment[]> GetCommentsAsync(CancellationToken cancellationToken = default)
	{
		return _client.GetPostCommentsAsync(_model.Service, _model.User, _model.Id, cancellationToken);
	}

	public Task FavoriteAsync(CancellationToken cancellationToken = default)
	{
		return _client.AddFavoritePostAsync(_model.Service, _model.User, _model.Id, cancellationToken);
	}

	public Task UnfavoriteAsync(CancellationToken cancellationToken = default)
	{
		return _client.RemoveFavoritePostAsync(_model.Service, _model.User, _model.Id, cancellationToken);
	}

	public Task FlagAsync(CancellationToken cancellationToken = default)
	{
		return _client.FlagPostAsync(_model.Service, _model.User, _model.Id, cancellationToken);
	}

	public Task<bool> IsFlaggedAsync(CancellationToken cancellationToken = default)
	{
		return _client.CheckPostFlagAsync(_model.Service, _model.User, _model.Id, cancellationToken);
	}
}