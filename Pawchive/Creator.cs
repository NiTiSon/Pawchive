using System;
using System.Threading.Tasks;

namespace Pawchive;

public class Creator
{
	private readonly PawchiveClient _client;
	private readonly Models.CreatorModel _model;

	internal Creator(PawchiveClient client, Models.CreatorModel model)
	{
		_client = client;
		_model = model;
	}

	public string Id => _model.Id;
	public string Name => _model.Name;
	public Service Service => _model.Service;

	public Task<Post?> GetPostByIdAsync(int id) => GetPostByIdAsync(id.ToString());

	public async Task<Post?> GetPostByIdAsync(string id)
	{
		throw new NotImplementedException();
	}
}