namespace Pawchive.Tests;

internal sealed class ContinuousIntegrationSkipAttribute() : SkipAttribute("This test isn't allowed to execute on remote server.")
{
	public override Task<bool> ShouldSkip(TestRegisteredContext context)
	{
		return Task.FromResult(Environment.GetEnvironmentVariable("CI") is not (null or "false"));
	}
}