using System;
using Pawchive.Models;

namespace Pawchive;

public sealed class Favorite
{
	private readonly FavoriteCreatorModel _model;

	internal Favorite(FavoriteCreatorModel model)
	{
		_model = model;
	}

	/// <summary>Position in the favorites list, lowest first.</summary>
	public int Sequence => _model.Sequence;

	public string Id => _model.Id;

	public string Name => _model.Name;

	public Service Service => _model.Service;

	/// <summary>When the creator was first indexed.</summary>
	public DateTime Indexed => _model.Indexed;

	/// <summary>Last update.</summary>
	public DateTime Updated => _model.Updated;

	/// <summary>Null when the creator has never been imported.</summary>
	public DateTime? LastImported => _model.LastImported == default ? null : _model.LastImported;

	/// <summary>Post title. Only set when listing favorited posts rather than creators.</summary>
	public string? Title => _model.Title;
}