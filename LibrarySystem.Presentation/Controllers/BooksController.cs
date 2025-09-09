using System.Collections.Generic;
using System.Linq;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    [Produces("application/json")]
    public class BooksController : ControllerBase
    {
        private readonly LibraryDbContext _context;
        public BooksController(LibraryDbContext context) => _context = context;

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<Book>), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery] string? title = null)
        {
            var q = _context.Books.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                var t = title.Trim().ToLowerInvariant();
                q = q.Where(b => b.Title != null && b.Title.ToLowerInvariant().Contains(t));
            }

            return Ok(q.ToList());
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Book), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetById(int id)
        {
            var book = _context.Books.AsNoTracking().FirstOrDefault(b => b.Id == id);
            return book is null ? NotFound() : Ok(book);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        [ProducesResponseType(typeof(Book), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Create([FromBody] Book book)
        {
            if (book is null) return BadRequest("Body is required.");

            var title = (book.Title ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(title))
                return BadRequest("Title is required.");
            book.Title = title;

            if (!_context.Authors.Any(a => a.Id == book.AuthorId))
                return BadRequest("AuthorId does not exist.");

            if (book.CopiesTotal < 1) book.CopiesTotal = 1;
            if (book.CopiesAvailable < 0) book.CopiesAvailable = 0;
            if (book.CopiesAvailable > book.CopiesTotal) book.CopiesAvailable = book.CopiesTotal;

            _context.Books.Add(book);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Librarian")]
        [ProducesResponseType(typeof(Book), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Update(int id, [FromBody] Book book)
        {
            var existing = _context.Books.Find(id);
            if (existing is null) return NotFound();
            if (book is null) return BadRequest("Body is required.");

            var newTitle = (book.Title ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(newTitle))
                return BadRequest("Title is required.");

            if (!_context.Authors.Any(a => a.Id == book.AuthorId))
                return BadRequest("AuthorId does not exist.");

            existing.Title = newTitle;
            existing.AuthorId = book.AuthorId;
            existing.CopiesTotal = Math.Max(1, book.CopiesTotal);
            existing.CopiesAvailable = Math.Clamp(book.CopiesAvailable, 0, existing.CopiesTotal);

            _context.SaveChanges();
            return Ok(existing);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(int id)
        {
            var existing = _context.Books.Find(id);
            if (existing is null) return NotFound();

            if (_context.Loans.Any(l => l.BookId == id && l.ReturnDate == null))
                return BadRequest("Book has active loans.");

            _context.Books.Remove(existing);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
