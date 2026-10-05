using System;
using Pawchive.Models;

namespace Pawchive;

public sealed class FileHashResult
{
	private readonly FileHashResultModel _model;

	internal FileHashResult(FileHashResultModel model)
	{
		_model = model;
	}

	public int Id => _model.Id;

	public string Hash => _model.Hash;

	public string Mime => _model.Mime;

	public string Extension => _model.Ext;

	public long Size => _model.Size;

	public string? IHash => _model.IHash;

	public DateTime Added => _model.Added;

	/// <summary>Last content modification, as sent by the API.</summary>
	public string Modified => _model.MTime;

	/// <summary>Creation time, as sent by the API.</summary>
	public string Created => _model.CTime;

	public HashPost[] GetPosts()
	{
		HashPostModel[] models = _model.Posts;

		if (models.Length == 0)
		{
			return [];
		}

		HashPost[] result = new HashPost[models.Length];
		for (int i = 0; i < models.Length; i++)
		{
			result[i] = new HashPost(models[i]);
		}

		return result;
	}

	public DiscordHashPost[] GetDiscordPosts()
	{
		DiscordHashPostModel[] models = _model.DiscordPosts;

		if (models.Length == 0)
		{
			return [];
		}

		DiscordHashPost[] result = new DiscordHashPost[models.Length];
		for (int i = 0; i < models.Length; i++)
		{
			result[i] = new DiscordHashPost(models[i]);
		}

		return result;
	}
}