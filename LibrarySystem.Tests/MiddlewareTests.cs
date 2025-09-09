using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LibrarySystem.Tests;

public class MiddlewareTests : IClassFixture<TestWebAppFactory>
{
    private readonly TestWebAppFactory _factory;

    public MiddlewareTests(TestWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Api_without_XClientName_returns_400()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var resp = await client.GetAsync("/api/Books");

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Api_with_XClientName_returns_200()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Client-Name", "Tests");

        var resp = await client.GetAsync("/api/Books");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("/swagger/v1/swagger.json")]
    [InlineData("/swagger/index.html")]
    [InlineData("/graphql")]
    public async Task NonApi_endpoints_do_not_require_header(string path)
    {
        var client = _factory.CreateClient();

        var resp = await client.GetAsync(path);

        resp.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
    }
}
