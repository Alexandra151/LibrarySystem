using Microsoft.AspNetCore.Authorization;

namespace LibrarySystem.Presentation.Controllers
{
    public record CreateLoanRequest(int BookId, int Days = 14);
}
