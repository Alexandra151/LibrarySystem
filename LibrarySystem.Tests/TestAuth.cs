using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace LibrarySystem.Tests;

public static class TestAuth
{
    private record LoginRequest(string username, string password);
    private record LoginResponse(string token);

    public static async Task<string> LoginAndGetJwtAsync(HttpClient client, string user, string pass)
    {
        var res = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequest(user, pass));
        res.EnsureSuccessStatusCode();
        var payload = await res.Content.ReadFromJsonAsync<LoginResponse>();
        return payload!.token;
    }

    public static void UseJwt(this HttpClient client, string jwt)
        => client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
}
