using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibrarySystem.Web.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _http;

    public LoginModel(IHttpClientFactory http) => _http = http;

    [BindProperty] public LoginInput Input { get; set; } = new();
    public string? Error { get; set; }

    public void OnGet() { }

    public class LoginInput
    {
        [Required] public string Username { get; set; } = "";
        [Required] public string Password { get; set; } = "";
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid) return Page();

        var client = _http.CreateClient("api");
        using var resp = await client.PostAsJsonAsync("/api/Auth/login", new
        {
            username = Input.Username,
            password = Input.Password
        });

        if (!resp.IsSuccessStatusCode)
        {
            Error = "B³êdny login lub has³o.";
            return Page();
        }

        var payload = await resp.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        var token = payload?["token"] ?? payload?["Token"];
        if (string.IsNullOrWhiteSpace(token))
        {
            Error = "Brak 'token' w odpowiedzi.";
            return Page();
        }

        // Zapisz JWT do sesji (HeaderHandler dopnie Bearer do kolejnych ¿¹dañ)
        HttpContext.Session.SetString("jwt", token);

        // Utwórz cookie z to¿samoœci¹ (Name + Role)
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var claims = new List<Claim> { new Claim(ClaimTypes.Name, Input.Username) };

        // Zbierz role z "role"/"roles"/ClaimTypes.Role, tak¿e CSV
        var roles = jwt.Claims
            .Where(c => c.Type is "role" or "roles" or ClaimTypes.Role)
            .SelectMany(c => c.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        foreach (var r in roles)
            claims.Add(new Claim(ClaimTypes.Role, r));

        var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(id);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        // Bezpieczne przekierowanie po zalogowaniu
        var target = (returnUrl != null && Url.IsLocalUrl(returnUrl)) ? returnUrl : "/";
        return LocalRedirect(target);
    }
}
