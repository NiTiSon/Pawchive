using Pawchive.Tests;

// This probably illegal to run on GitHub servers
[assembly: ContinuousIntegrationSkip]
[assembly: Timeout(timeoutInMilliseconds: 1000 * 15)]
[assembly: Retry(times: 3)]

namespace Pawchive.Tests;

public sealed class PawchiveClientTests
{
	[Test]
	public async Task GetVersion()
	{
		PawchiveClient client = new();

		string version = await client.GetVersionAsync();

		await Assert.That(version).IsNotNullOrWhiteSpace();
	}

	[Test]
	public async Task GetRecentPosts()
	{
		PawchiveClient client = new();

		Post[] posts = await client.GetRecentPosts();

		await Assert.That(posts).All(x => x != null!).And.Count().IsEqualTo(50);
	}

	[Test]
	[Explicit] // this test is heavy
	public async Task GetCreators()
	{
		PawchiveClient client = new();

		Creator[] creators = await client.GetCreatorsAsync();

		await Assert.That(creators).IsNotEmpty();
	}

	[Test]
	public async Task SearchPosts()
	{
		const string query = "ananas";
		PawchiveClient client = new();

		Post[] posts = await client.SearchPosts(query);

		await Assert.That(posts).IsNotEmpty().And.Contains(x => x.Title.Contains(query));
	}
}