namespace CampusEats.Tests.APITests;

public class CampusEatsAPITests(CustomWebApplicationFactory factory) :  IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    
    [Fact]
    public async Task GetMenuItems_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/menu");
        response.EnsureSuccessStatusCode();
    }
}