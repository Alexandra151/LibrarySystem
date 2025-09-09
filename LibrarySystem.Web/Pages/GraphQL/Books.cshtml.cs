using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibrarySystem.Web.Pages.GraphQL;

public class BooksModel : PageModel
{
    private readonly IHttpClientFactory _http;
    public BooksModel(IHttpClientFactory http) => _http = http;

    public List<BookVm> Items { get; set; } = new();

    public async Task OnGetAsync()
    {
        var client = _http.CreateClient("api");
        var body = new
        {
            query = "query { books { id title authorId copiesTotal copiesAvailable } }"
        };
        var resp = await client.PostAsJsonAsync("/graphql", body);
        resp.EnsureSuccessStatusCode();

        var data = await resp.Content.ReadFromJsonAsync<GqlBooksResponse>();
        Items = data?.Data?.Books ?? new();
    }

    public record GqlBooksResponse(GqlData Data);
    public record GqlData(List<BookVm> Books);
    public record BookVm(int Id, string? Title, int AuthorId, int CopiesTotal, int CopiesAvailable);
}
