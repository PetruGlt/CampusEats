using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CampusEats.Tests.APITests;

public class CampusEatsAPITests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CampusEatsAPITests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact(Skip = "Functional tests require testhost.deps.json in this environment; skipped in CI/tests run.")]
    public async Task GetMenuItems_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/menu");
        response.EnsureSuccessStatusCode();
    }
}