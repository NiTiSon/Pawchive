using System.Threading.Tasks;

namespace Pawchive;

public class Post
{
	public Creator Creator { get; }

	private readonly Models.PostModel _model;

	internal Post(Creator creator, Models.PostModel model)
	{
		Creator = creator;
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

		return Creator.GetPostByIdAsync(_model.Next);
	}

	public Task<Post?> GetPreviousPostAsync()
	{
		if (string.IsNullOrWhiteSpace(_model.Prev))
		{
			return Task.FromResult<Post?>(null);
		}

		return Creator.GetPostByIdAsync(_model.Prev);
	}
}