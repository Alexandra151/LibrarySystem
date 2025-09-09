using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LibrarySystem.Tests;

public class SmokeTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;

    private sealed record LoginResponse(string token);

    public SmokeTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        _client.DefaultRequestHeaders.Add("X-Client-Name", "Tests");
    }

    private async Task<string> LoginAsync(string user = "admin", string pass = "admin123")
    {
        var resp = await _client.PostAsJsonAsync("/api/Auth/login", new { username = user, password = pass });
        resp.EnsureSuccessStatusCode();

        var payload = await resp.Content.ReadFromJsonAsync<LoginResponse>();
        return payload!.token;
    }

    [Fact]
    public async Task SwaggerJson_Is_Available()
    {
        var resp = await _client.GetAsync("/swagger/v1/swagger.json");
        resp.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Authors_Get_DoesNotRequireAuth()
    {
        var resp = await _client.GetAsync("/api/Authors");
        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task Authors_Post_RequiresAuth_AndCreates_WithLibrarian()
    {
        var r1 = await _client.PostAsJsonAsync("/api/Authors", new { name = "Any Name" });
        r1.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);

        var token = await LoginAsync("librarian", "librarian123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var uniqueName = $"Test Author {Guid.NewGuid():N}".Substring(0, 20);

        var r2 = await _client.PostAsJsonAsync("/api/Authors", new { name = uniqueName });
        r2.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        _client.DefaultRequestHeaders.Authorization = null;
    }
}
