using System.Linq;
using HotChocolate;
using HotChocolate.Data;
using LibrarySystem.Infrastructure.Persistence;
using LibrarySystem.Domain.Entities;

namespace LibrarySystem.Presentation.GraphQL
{
    public class Query
    {
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Book> GetBooks([Service] LibraryDbContext ctx)
            => ctx.Books;

        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Author> GetAuthors([Service] LibraryDbContext ctx)
            => ctx.Authors;

        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Loan> GetLoans([Service] LibraryDbContext ctx, bool onlyActive = false)
            => onlyActive ? ctx.Loans.Where(l => l.ReturnDate == null) : ctx.Loans;
    }
}
