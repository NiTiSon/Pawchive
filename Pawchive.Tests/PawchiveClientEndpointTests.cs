using System;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Pawchive.Tests;

/// <summary>
/// Offline tests for the request shape and response parsing. Unlike <see cref="PawchiveClientTests"/>
/// these never touch the network, so they run everywhere including CI.
/// </summary>
public sealed class PawchiveClientEndpointTests
{
	[Test]
	[Arguments("a & b", "a%20%26%20b")]
	[Arguments("plain", "plain")]
	public async Task SearchPosts_UrlEncodesQuery(string query, string encoded)
	{
		RecordingHandler handler = new("[]");
		PawchiveClient client = MakeClient(handler);

		await client.SearchPosts(query);

		await Assert.That(handler.LastRequestUri!.Query).Contains($"q={encoded}");
	}

	[Test]
	public async Task GetCreatorPostsAsync_HitsUserRootWithoutPostsSegment()
	{
		RecordingHandler handler = new("[]");
		PawchiveClient client = MakeClient(handler);

		await client.GetCreatorPostsAsync(Service.Patreon, "user1", 2);

		await Assert.That(handler.LastRequestUri!.AbsolutePath).IsEqualTo("/api/v1/patreon/user/user1");
		await Assert.That(handler.LastRequestUri.Query).Contains("o=100");
	}

	[Test]
	public async Task GetFavoritesAsync_SendsSessionCookieAndParses()
	{
		RecordingHandler handler = new("""
			[
				{ "faved_seq": 1, "id": "c1", "indexed": "1700000000", "last_imported": "", "name": "Fav", "service": "patreon", "updated": "1700000000" },
				{ "faved_seq": 2, "id": "p1", "user": "u1", "title": "A post", "indexed": "", "last_imported": "", "name": "", "service": "fanbox", "updated": "" }
			]
			""");
		PawchiveClient client = MakeClient(handler);
		client.Auth("abc123");

		Favorite[] favorites = await client.GetFavoritesAsync("artist");

		await Assert.That(handler.CookieHeader).IsEqualTo("session=abc123");
		await Assert.That(favorites).Count().IsEqualTo(2);
		await Assert.That(favorites[0].Sequence).IsEqualTo(1);
		await Assert.That(favorites[0].Name).IsEqualTo("Fav");
		await Assert.That(favorites[0].Service).IsEqualTo(Service.Patreon);
		await Assert.That(favorites[1].Title).IsEqualTo("A post");
		await Assert.That(favorites[0].Indexed).IsEqualTo(new DateTime(2023, 11, 14, 22, 13, 20, DateTimeKind.Utc));
		await Assert.That(favorites[0].LastImported).IsNull();
	}

	[Test]
	[Arguments("POST", "/api/v1/favorites/post/patreon/u1/p1")]
	[Arguments("DELETE", "/api/v1/favorites/post/patreon/u1/p1")]
	public async Task FavoritePost_SendsVerbToPath(string method, string path)
	{
		RecordingHandler handler = new("{}");
		PawchiveClient client = MakeClient(handler);

		if (method == "POST")
		{
			await client.AddFavoritePostAsync(Service.Patreon, "u1", "p1");
		}
		else
		{
			await client.RemoveFavoritePostAsync(Service.Patreon, "u1", "p1");
		}

		await Assert.That(handler.LastRequest!.Method.Method).IsEqualTo(method);
		await Assert.That(handler.LastRequestUri!.AbsolutePath).IsEqualTo(path);
	}

	[Test]
	[Arguments("POST", "/api/v1/favorites/creator/fanbox/c1")]
	[Arguments("DELETE", "/api/v1/favorites/creator/fanbox/c1")]
	public async Task FavoriteCreator_SendsVerbToPath(string method, string path)
	{
		RecordingHandler handler = new("{}");
		PawchiveClient client = MakeClient(handler);

		if (method == "POST")
		{
			await client.AddFavoriteCreatorAsync(Service.PixivFanbox, "c1");
		}
		else
		{
			await client.RemoveFavoriteCreatorAsync(Service.PixivFanbox, "c1");
		}

		await Assert.That(handler.LastRequest!.Method.Method).IsEqualTo(method);
		await Assert.That(handler.LastRequestUri!.AbsolutePath).IsEqualTo(path);
	}

	[Test]
	public async Task FlagPost_SendsPostToFlagEndpoint()
	{
		RecordingHandler handler = new("{}");
		PawchiveClient client = MakeClient(handler);

		await client.FlagPostAsync(Service.Patreon, "u1", "p1");

		await Assert.That(handler.LastRequest!.Method.Method).IsEqualTo("POST");
		await Assert.That(handler.LastRequestUri!.AbsolutePath).IsEqualTo("/api/v1/patreon/user/u1/post/p1/flag");
	}

	[Test]
	[Arguments(HttpStatusCode.OK, true)]
	[Arguments(HttpStatusCode.NotFound, false)]
	public async Task CheckPostFlagAsync_MapsStatusToBool(HttpStatusCode status, bool expected)
	{
		RecordingHandler handler = new("{}", status);
		PawchiveClient client = MakeClient(handler);

		bool flagged = await client.CheckPostFlagAsync(Service.Patreon, "u1", "p1");

		await Assert.That(flagged).IsEqualTo(expected);
	}

	[Test]
	public async Task GetPostCommentsAsync_ParsesNestedRevisions()
	{
		RecordingHandler handler = new("""
			[
				{
					"id": "c1",
					"parent_id": null,
					"commenter": "someone",
					"content": "first",
					"published": "2026-01-01T00:00:00",
					"revisions": [ { "id": 1, "content": "edited", "added": "2026-01-02T00:00:00" } ]
				}
			]
			""");
		PawchiveClient client = MakeClient(handler);

		Comment[] comments = await client.GetPostCommentsAsync(Service.Patreon, "u1", "p1");

		await Assert.That(comments).Count().IsEqualTo(1);
		await Assert.That(comments[0].Commenter).IsEqualTo("someone");
		await Assert.That(comments[0].ParentId).IsNull();
		await Assert.That(comments[0].GetRevisions()).Count().IsEqualTo(1);
		await Assert.That(comments[0].GetRevisions()[0].Content).IsEqualTo("edited");
	}

	[Test]
	public async Task GetPostRevisionsAsync_ParsesRevisionId()
	{
		RecordingHandler handler = new("""
			[
				{
					"revision_id": 7,
					"id": "p1",
					"user": "u1",
					"service": "patreon",
					"title": "Old title",
					"content": "old body",
					"embed": {},
					"shared_file": false,
					"added": "2026-01-01T00:00:00",
					"published": "2026-01-01T00:00:00",
					"edited": "2026-01-02T00:00:00",
					"file": null,
					"attachments": []
				}
			]
			""");
		PawchiveClient client = MakeClient(handler);

		PostRevision[] revisions = await client.GetPostRevisionsAsync(Service.Patreon, "u1", "p1");

		await Assert.That(revisions).Count().IsEqualTo(1);
		await Assert.That(revisions[0].RevisionId).IsEqualTo(7);
		await Assert.That(revisions[0].Title).IsEqualTo("Old title");
		await Assert.That(revisions[0].CreatorId).IsEqualTo("u1");
	}

	[Test]
	public async Task SearchFileByHashAsync_ParsesMatches()
	{
		RecordingHandler handler = new("""
			{
				"id": 1,
				"hash": "aabbcc",
				"mtime": "",
				"ctime": "",
				"mime": "image/png",
				"ext": "png",
				"added": "2026-01-01T00:00:00",
				"size": 1024,
				"ihash": null,
				"posts": [
					{
						"file_id": 1,
						"id": "p1",
						"user": "u1",
						"service": "patreon",
						"title": "Hash match",
						"substring": "test",
						"published": "2026-01-01T00:00:00",
						"file": { "name": "img.png", "path": "/ab/cd" },
						"attachments": []
					}
				],
				"discord_posts": [
					{
						"file_id": 2,
						"id": "d1",
						"server": "srv",
						"channel": "chan",
						"substring": "hi",
						"published": "2026-01-01T00:00:00",
						"embeds": [],
						"mentions": [],
						"attachments": []
					}
				]
			}
			""");
		PawchiveClient client = MakeClient(handler);

		FileHashResult result = await client.SearchFileByHashAsync("aabbcc");

		await Assert.That(result.Hash).IsEqualTo("aabbcc");
		await Assert.That(result.Mime).IsEqualTo("image/png");
		await Assert.That(result.Size).IsEqualTo(1024);
		await Assert.That(result.IHash).IsNull();
		await Assert.That(result.GetPosts()).Count().IsEqualTo(1);
		await Assert.That(result.GetPosts()[0].Title).IsEqualTo("Hash match");
		await Assert.That(result.GetPosts()[0].File!.Url).IsEqualTo("https://file.pawchive.pw/data/ab/cd");
		await Assert.That(result.GetDiscordPosts()).Count().IsEqualTo(1);
		await Assert.That(result.GetDiscordPosts()[0].Server).IsEqualTo("srv");
	}

	[Test]
	public async Task GetVersionAsync_TrimsWhitespace()
	{
		RecordingHandler handler = new("  1.2.3\n", HttpStatusCode.OK, "text/plain");
		PawchiveClient client = MakeClient(handler);

		string version = await client.GetVersionAsync();

		await Assert.That(version).IsEqualTo("1.2.3");
	}

	[Test]
	// /creators sends unix seconds as a number.
	[Arguments("""{"id":"c1","name":"A","service":"patreon","indexed":1785423600,"updated":1791154800,"favorited":2319,"ever_imported":true}""", "2026-07-30T15:00:00Z", "2026-10-04T23:00:00Z")]
	// /profile and /links send ISO-8601 strings instead.
	[Arguments("""{"id":"4969886","name":"B","service":"patreon","indexed":"2026-06-10T21:00:00","updated":"2026-10-05T17:00:00","public_id":"B","relation_id":null,"ever_imported":true}""", "2026-06-10T21:00:00Z", "2026-10-05T17:00:00Z")]
	// Numeric strings are tolerated too.
	[Arguments("""{"id":"c3","name":"C","service":"patreon","indexed":"1781125200","updated":"1791219600","public_id":"C"}""", "2026-06-10T21:00:00Z", "2026-10-05T17:00:00Z")]
	public async Task CreatorTimestamps_AcceptNumberIsoAndNumericString(string body, string expectedIndexed, string expectedUpdated)
	{
		RecordingHandler handler = new(body);
		PawchiveClient client = MakeClient(handler);

		Creator? creator = await client.GetCreatorByIdAsync(Service.Patreon, "c1");

		await Assert.That(handler.LastRequestUri!.AbsolutePath).IsEqualTo("/api/v1/patreon/user/c1/profile");
		await Assert.That(creator).IsNotNull();
		await Assert.That(creator!.Indexed).IsEqualTo(DateTime.Parse(expectedIndexed, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal));
		await Assert.That(creator.Updated).IsEqualTo(DateTime.Parse(expectedUpdated, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal));
		await Assert.That(creator.Indexed.Kind).IsEqualTo(DateTimeKind.Utc);
	}

	[Test]
	public async Task GetCreatorsAsync_ParsesUnixSecondTimestamps()
	{
		RecordingHandler handler = new("""
			[
				{ "id": "c1", "name": "A", "service": "patreon", "indexed": 1785423600, "updated": 1791154800, "favorited": 2319, "ever_imported": true }
			]
			""");
		PawchiveClient client = MakeClient(handler);

		Creator[] creators = await client.GetCreatorsAsync();

		await Assert.That(creators).Count().IsEqualTo(1);
		await Assert.That(creators[0].Id).IsEqualTo("c1");
		await Assert.That(creators[0].Followers).IsEqualTo(2319);
	}

	private const int PageSize = 50;

	[Test]
	public async Task EnumeratePostsAsync_StopsAfterShortPage()
	{
		SequenceHandler handler = new(PostsPage(3));
		PawchiveClient client = MakeClient(handler);

		List<Post> posts = await CollectAsync(client.EnumeratePostsAsync());

		await Assert.That(posts).Count().IsEqualTo(3);
		await Assert.That(handler.Requests).Count().IsEqualTo(1);
	}

	[Test]
	public async Task EnumeratePostsAsync_KeepsPagingUntilShortPage()
	{
		SequenceHandler handler = new(PostsPage(PageSize), PostsPage(PageSize), PostsPage(2));
		PawchiveClient client = MakeClient(handler);

		List<Post> posts = await CollectAsync(client.EnumeratePostsAsync());

		await Assert.That(posts).Count().IsEqualTo((PageSize * 2) + 2);
		await Assert.That(handler.Requests).Count().IsEqualTo(3);
		await Assert.That(handler.Requests[0].Query).Contains("o=0");
		await Assert.That(handler.Requests[1].Query).Contains($"o={PageSize}");
		await Assert.That(handler.Requests[2].Query).Contains($"o={PageSize * 2}");
	}

	[Test]
	public async Task EnumeratePostsAsync_StopsOnEmptyPage()
	{
		SequenceHandler handler = new();
		PawchiveClient client = MakeClient(handler);

		List<Post> posts = await CollectAsync(client.EnumeratePostsAsync());

		await Assert.That(posts).IsEmpty();
		await Assert.That(handler.Requests).Count().IsEqualTo(1);
	}

	[Test]
	public async Task EnumeratePostsAsync_StartsFromGivenPage()
	{
		SequenceHandler handler = new(PostsPage(1));
		PawchiveClient client = MakeClient(handler);

		await CollectAsync(client.EnumeratePostsAsync(3, "hello"));

		await Assert.That(handler.Requests[0].Query).Contains("o=150");
		await Assert.That(handler.Requests[0].Query).Contains("q=hello");
	}

	[Test]
	public async Task EnumeratePostsAsync_ThrowsWhenCancelled()
	{
		SequenceHandler handler = new(PostsPage(PageSize), PostsPage(PageSize));
		PawchiveClient client = MakeClient(handler);
		using CancellationTokenSource cts = new();

		List<Post> posts = new();

		await Assert.ThrowsAsync<OperationCanceledException>(async () =>
		{
			await foreach (Post post in client.EnumeratePostsAsync(cancellationToken: cts.Token))
			{
				posts.Add(post);

				if (posts.Count == PageSize)
				{
					await cts.CancelAsync();
				}
			}
		});

		// The page already in flight is still yielded before the token is observed.
		await Assert.That(posts).Count().IsEqualTo(PageSize);
		await Assert.That(handler.Requests).Count().IsEqualTo(1);
	}

	[Test]
	public async Task EnumerateCreatorPostsAsync_HitsCreatorPathAndPaginates()
	{
		SequenceHandler handler = new(PostsPage(PageSize), PostsPage(4));
		PawchiveClient client = MakeClient(handler);

		List<Post> posts = await CollectAsync(client.EnumerateCreatorPostsAsync(Service.Patreon, "u1"));

		await Assert.That(posts).Count().IsEqualTo(PageSize + 4);
		await Assert.That(handler.Requests[0].AbsolutePath).IsEqualTo("/api/v1/patreon/user/u1");
	}

	[Test]
	public async Task EnumeratePostsAsync_StopsWhenApiRejectsOffsetPastCap()
	{
		// The live API 400s past ~50k posts rather than returning a short page.
		SequenceHandler handler = new SequenceHandler(PostsPage(PageSize))
			.WithStatus(HttpStatusCode.OK, HttpStatusCode.BadRequest);
		PawchiveClient client = MakeClient(handler);

		List<Post> posts = await CollectAsync(client.EnumeratePostsAsync());

		await Assert.That(posts).Count().IsEqualTo(PageSize);
		await Assert.That(handler.Requests).Count().IsEqualTo(2);
	}

	[Test]
	public async Task EnumeratePostsAsync_ThrowsWhenFirstPageIsRejected()
	{
		// A 400 on the opening page is a genuine error, not the end of the feed.
		SequenceHandler handler = new SequenceHandler("{}").WithStatus(HttpStatusCode.BadRequest);
		PawchiveClient client = MakeClient(handler);

		await Assert.ThrowsAsync<HttpRequestException>(async () => await CollectAsync(client.EnumeratePostsAsync()));

		await Assert.That(handler.Requests).Count().IsEqualTo(1);
	}

	[Test]
	public async Task EnumerateCreatorPostsAsync_DeserializesNullEditedAndTags()
	{
		// The live API returns edited/tags as JSON null for most creator posts.
		string page =
			"""
			[
				{ "id": "a", "user": "u1", "service": "patreon", "title": "A", "embed": {}, "added": "2026-01-01T00:00:00", "published": "2026-01-01T00:00:00", "edited": null, "tags": null, "file": null, "attachments": [] },
				{ "id": "b", "user": "u1", "service": "patreon", "title": "B", "embed": {}, "added": "2026-01-01T00:00:00", "published": "2026-01-01T00:00:00", "edited": "2026-01-02T03:04:05", "tags": "x", "file": null, "attachments": [] }
			]
			""";
		PawchiveClient client = MakeClient(new RecordingHandler(page));

		Post[] posts = await client.GetCreatorPostsAsync(Service.Patreon, "u1");

		await Assert.That(posts).Count().IsEqualTo(2);
		await Assert.That(posts[0].Edited).IsNull();
		await Assert.That(posts[1].Edited).IsEqualTo(new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc));
		await Assert.That(posts[0].Tags).IsEmpty();
	}

	[Test]
	[Arguments("null", "")]
	[Arguments("[\"tekken\"]", "tekken")]
	[Arguments("\"{\\\"Bloodborne Eternal Beast\\\"}\"", "Bloodborne Eternal Beast")]
	[Arguments("\"{Note,US}\"", "Note|US")]
	[Arguments("\"{Bloodborne Eternal Beast}\"", "Bloodborne Eternal Beast")]
	public async Task PostTags_AcceptsEveryShapeTheApiSends(string tagsJson, string expected)
	{
		string page =
			$$"""
			[ { "id": "a", "user": "u1", "service": "patreon", "title": "A", "embed": {}, "added": "2026-01-01T00:00:00", "published": "2026-01-01T00:00:00", "edited": null, "tags": {{tagsJson}}, "file": null, "attachments": [] } ]
			""";
		PawchiveClient client = MakeClient(new RecordingHandler(page));

		Post[] posts = await client.GetCreatorPostsAsync(Service.Patreon, "u1");

		string actual = string.Join("|", posts[0].Tags);
		await Assert.That(actual).IsEqualTo(expected);
	}

	[Test]
	public async Task PostAttachments_ExposesNamePathAndNode()
	{
		string page =
			"""
			[ { "id": "a", "user": "u1", "service": "patreon", "title": "A", "embed": {}, "added": "2026-01-01T00:00:00", "published": "2026-01-01T00:00:00", "edited": null, "tags": null,
			  "file": { "name": "main.epub", "path": "/aa/bb/main.epub" },
			  "attachments": [
				{ "name": "one.epub", "path": "/cc/dd/one.epub", "node": 2 },
				{ "name": "two.epub", "path": "/ee/ff/two.epub" }
			  ] } ]
			""";
		PawchiveClient client = MakeClient(new RecordingHandler(page));

		Post post = (await client.GetCreatorPostsAsync(Service.Patreon, "u1"))[0];

		FileAttachment[] attachments = post.GetAttachments();

		await Assert.That(attachments).Count().IsEqualTo(2);
		await Assert.That(attachments[0].Name).IsEqualTo("one.epub");
		await Assert.That(attachments[0].RelativePath).IsEqualTo("/cc/dd/one.epub");
		await Assert.That(attachments[0].Node).IsEqualTo(2);
		await Assert.That(attachments[0].Url).IsEqualTo("https://file.pawchive.pw/data/cc/dd/one.epub");

		// node is absent here, so it must surface as null rather than throw.
		await Assert.That(attachments[1].Node).IsNull();

		await Assert.That(post.File!.Name).IsEqualTo("main.epub");
	}

	[Test]
	[Arguments("[]")]
	[Arguments("null")]
	public async Task PostAttachments_AreEmptyWhenAbsentOrEmpty(string attachmentsJson)
	{
		string page =
			$$"""
			[ { "id": "a", "user": "u1", "service": "patreon", "title": "A", "embed": {}, "added": "2026-01-01T00:00:00", "published": "2026-01-01T00:00:00", "edited": null, "tags": null, "file": null, "attachments": {{attachmentsJson}} } ]
			""";
		PawchiveClient client = MakeClient(new RecordingHandler(page));

		Post post = (await client.GetCreatorPostsAsync(Service.Patreon, "u1"))[0];

		await Assert.That(post.GetAttachments()).IsEmpty();
	}

	[Test]
	public async Task DeferredAttachment_WithoutPath_FallsBackToTempUrl()
	{
		// This shape has no "path" key at all, which used to throw NullReferenceException in Url.
		string page =
			"""
			[ { "id": "a", "user": "u1", "service": "patreon", "title": "A", "embed": {}, "added": "2026-01-01T00:00:00", "published": "2026-01-01T00:00:00", "edited": null, "tags": null, "file": null,
			  "attachments": [ { "name": "Lulu.zip", "deferred": true, "temp_url": "https://t1.pawchive.pw/f/abc/Lulu.zip", "temp_expires": "2026-10-20T03:00:00+03:00" } ] } ]
			""";
		PawchiveClient client = MakeClient(new RecordingHandler(page));

		Post post = (await client.GetCreatorPostsAsync(Service.Patreon, "u1"))[0];
		FileAttachment attachment = post.GetAttachments()[0];

		await Assert.That(attachment.Deferred).IsTrue();
		await Assert.That(attachment.RelativePath).IsEmpty();
		await Assert.That(attachment.Url).IsEqualTo("https://t1.pawchive.pw/f/abc/Lulu.zip");
		await Assert.That(attachment.TempExpires).IsNotNull();
	}

	[Test]
	public async Task DeferredAttachment_PrefersTempDownloadUrlOverStreamUrl()
	{
		string page =
			"""
			[ { "id": "a", "user": "u1", "service": "patreon", "title": "A", "embed": {}, "added": "2026-01-01T00:00:00", "published": "2026-01-01T00:00:00", "edited": null, "tags": null, "file": null,
			  "attachments": [ { "name": "v.mp4", "deferred": true, "temp_url": "https://t1.pawchive.pw/v/1/master.m3u8", "temp_download_url": "https://t1.pawchive.pw/d/1/v.mp4" } ] } ]
			""";
		PawchiveClient client = MakeClient(new RecordingHandler(page));

		Post post = (await client.GetCreatorPostsAsync(Service.Patreon, "u1"))[0];
		FileAttachment attachment = post.GetAttachments()[0];

		await Assert.That(attachment.Url).IsEqualTo("https://t1.pawchive.pw/d/1/v.mp4");
		await Assert.That(attachment.TempUrl).IsEqualTo("https://t1.pawchive.pw/v/1/master.m3u8");
	}

	[Test]
	public async Task Attachment_WithoutName_IsEmptyRatherThanNull()
	{
		string page =
			"""
			[ { "id": "a", "user": "u1", "service": "patreon", "title": "A", "embed": {}, "added": "2026-01-01T00:00:00", "published": "2026-01-01T00:00:00", "edited": null, "tags": null, "file": null,
			  "attachments": [ { "path": "/d5/bd/abc.jpg" } ] } ]
			""";
		PawchiveClient client = MakeClient(new RecordingHandler(page));

		FileAttachment attachment = (await client.GetCreatorPostsAsync(Service.Patreon, "u1"))[0].GetAttachments()[0];

		await Assert.That(attachment.Name).IsEmpty();
		await Assert.That(attachment.Url).IsEqualTo("https://file.pawchive.pw/data/d5/bd/abc.jpg");
		await Assert.That(attachment.PreviewOnly).IsFalse();
	}

	[Test]
	public async Task Attachment_ExposesPreviewOnlyFlag()
	{
		string page =
			"""
			[ { "id": "a", "user": "u1", "service": "patreon", "title": "A", "embed": {}, "added": "2026-01-01T00:00:00", "published": "2026-01-01T00:00:00", "edited": null, "tags": null, "file": null,
			  "attachments": [ { "name": "p.jpg", "path": "/7d/92/p.jpg", "preview_only": true } ] } ]
			""";
		PawchiveClient client = MakeClient(new RecordingHandler(page));

		FileAttachment attachment = (await client.GetCreatorPostsAsync(Service.Patreon, "u1"))[0].GetAttachments()[0];

		await Assert.That(attachment.PreviewOnly).IsTrue();
		await Assert.That(attachment.Deferred).IsFalse();
	}

	[Test]
	public async Task Attachment_WithNeitherPathNorTempUrl_HasEmptyUrl()
	{
		// Neither fallback is available, so Url must be empty instead of throwing.
		string page =
			"""
			[ { "id": "a", "user": "u1", "service": "patreon", "title": "A", "embed": {}, "added": "2026-01-01T00:00:00", "published": "2026-01-01T00:00:00", "edited": null, "tags": null, "file": null,
			  "attachments": [ { "name": "Gaussian Splatting Modular Toolkit.zip", "deferred": true } ] } ]
			""";
		PawchiveClient client = MakeClient(new RecordingHandler(page));

		FileAttachment attachment = (await client.GetCreatorPostsAsync(Service.Patreon, "u1"))[0].GetAttachments()[0];

		await Assert.That(attachment.Url).IsEmpty();
		await Assert.That(attachment.Deferred).IsTrue();
		await Assert.That(attachment.TempExpires).IsNull();
	}

	private static async Task<List<Post>> CollectAsync(IAsyncEnumerable<Post> source)
	{
		List<Post> posts = new();

		await foreach (Post post in source)
		{
			posts.Add(post);
		}

		return posts;
	}

	private static string PostsPage(int count)
	{
		return "[" + string.Join(",", Enumerable.Range(0, count).Select(i =>
			$$"""{"id":"p{{i}}","user":"u1","service":"patreon","title":"T{{i}}","embed":{},"shared_file":false,"added":"2026-01-01T00:00:00","published":"2026-01-01T00:00:00","edited":"2026-01-01T00:00:00","file":null,"attachments":[]}""")) + "]";
	}

	private static PawchiveClient MakeClient(HttpMessageHandler handler)
	{
		return new PawchiveClient(new HttpClient(handler) { BaseAddress = new Uri("https://pawchive.pw") });
	}

	/// <summary>Serves a canned body per request, then empty pages, so paging can be observed.</summary>
	private sealed class SequenceHandler : HttpMessageHandler
	{
		private readonly Queue<string> _bodies;
		private Queue<HttpStatusCode> _statuses;

		public List<Uri> Requests { get; } = new();

		public SequenceHandler(params string[] bodies)
		{
			_bodies = new Queue<string>(bodies);
			_statuses = new Queue<HttpStatusCode>();
		}

		/// <summary>Statuses for the first N responses; responses past that default to 200.</summary>
		public SequenceHandler WithStatus(params HttpStatusCode[] statuses)
		{
			_statuses = new Queue<HttpStatusCode>(statuses);

			return this;
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			Requests.Add(request.RequestUri!);

			HttpResponseMessage response = new(_statuses.Count > 0 ? _statuses.Dequeue() : HttpStatusCode.OK)
			{
				Content = new StringContent(_bodies.Count > 0 ? _bodies.Dequeue() : "[]", System.Text.Encoding.UTF8, "application/json")
			};

			return Task.FromResult(response);
		}
	}

	private sealed class RecordingHandler : HttpMessageHandler
	{
		private readonly string _body;
		private readonly HttpStatusCode _status;
		private readonly string _contentType;

		public HttpRequestMessage? LastRequest { get; private set; }

		public Uri? LastRequestUri => LastRequest?.RequestUri;

		public string? CookieHeader => LastRequest?.Headers.TryGetValues("Cookie", out IEnumerable<string>? values) is true
			? string.Join("; ", values)
			: null;

		public RecordingHandler(string body, HttpStatusCode status = HttpStatusCode.OK, string contentType = "application/json")
		{
			_body = body;
			_status = status;
			_contentType = contentType;
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			LastRequest = request;

			HttpResponseMessage response = new(_status)
			{
				Content = new StringContent(_body)
				{
					Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(_contentType) }
				}
			};

			return Task.FromResult(response);
		}
	}
}