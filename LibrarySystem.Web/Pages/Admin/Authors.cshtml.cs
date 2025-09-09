using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibrarySystem.Web.Pages.Admin;

[Authorize(Roles = "Admin,Librarian")]
public class AuthorsModel : PageModel
{
    private readonly IHttpClientFactory _http;
    public AuthorsModel(IHttpClientFactory http) => _http = http;

    public List<AuthorDto> Items { get; set; } = new();

    [BindProperty] public CreateAuthorDto NewAuthor { get; set; } = new();

    public async Task OnGetAsync()
    {
        var client = _http.CreateClient("api");
        Items = await client.GetFromJsonAsync<List<AuthorDto>>("/api/Authors") ?? new();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) { await OnGetAsync(); return Page(); }

        var client = _http.CreateClient("api");
        var resp = await client.PostAsJsonAsync("/api/Authors", NewAuthor);
        if (!resp.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Nie uda³o siê dodaæ autora (czy nazwa unikalna?).");
        }
        return RedirectToPage();
    }

    public record AuthorDto(int Id, string? Name);
    public class CreateAuthorDto { [Required] public string? Name { get; set; } }
}
