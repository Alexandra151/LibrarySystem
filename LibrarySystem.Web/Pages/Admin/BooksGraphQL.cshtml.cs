using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibrarySystem.Web.Pages.Admin;

public class BooksGraphQLModel : PageModel
{
    private readonly IHttpClientFactory _http;
    public List<BookVm> Books { get; set; } = new();

    public BooksGraphQLModel(IHttpClientFactory http) => _http = http;

    public async Task OnGetAsync()
    {
        var client = _http.CreateClient("api");
        var query = new { query = "query { books { id title authorId copiesTotal copiesAvailable } }" };
        var resp = await client.PostAsJsonAsync("/graphql", query);
        var payload = await resp.Content.ReadFromJsonAsync<GraphQlResp<BookVm>>();
        Books = payload?.data?.books ?? new();
    }

    public record BookVm(int Id, string? Title, int AuthorId, int CopiesTotal, int CopiesAvailable);
    public class GraphQlResp<T>
    {
        public DataObj? data { get; set; }
        public class DataObj { public List<T>? books { get; set; } }
    }
}
