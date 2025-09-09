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
    public class AuthorsController : ControllerBase
    {
        private readonly LibraryDbContext _context;
        public AuthorsController(LibraryDbContext context) => _context = context;

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<Author>), StatusCodes.Status200OK)]
        public IActionResult Get(
            [FromQuery] string? name = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 50;

            var query = _context.Authors.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                var n = name.Trim().ToLowerInvariant();
                query = query.Where(a => a.Name != null && a.Name.ToLowerInvariant().Contains(n));
            }

            var total = query.Count();
            var items = query
                .OrderBy(a => a.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(items);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Author), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetById(int id, [FromQuery] bool includeBooks = false)
        {
            Author? author = includeBooks
                ? _context.Authors.Include(a => a.Books).AsNoTracking().FirstOrDefault(a => a.Id == id)
                : _context.Authors.AsNoTracking().FirstOrDefault(a => a.Id == id);

            return author is null ? NotFound() : Ok(author);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        [ProducesResponseType(typeof(Author), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Post([FromBody] Author author)
        {
            if (author is null) return BadRequest("Body is required.");

            author.Name = (author.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(author.Name))
                return BadRequest("Name is required.");

            bool exists = _context.Authors.Any(a =>
                a.Name != null && a.Name.ToLowerInvariant() == author.Name.ToLowerInvariant());
            if (exists) return Conflict("Author with this name already exists.");

            _context.Authors.Add(author);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = author.Id }, author);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Librarian")]
        [ProducesResponseType(typeof(Author), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Put(int id, [FromBody] Author author)
        {
            var existing = _context.Authors.Find(id);
            if (existing is null) return NotFound();

            if (author is null) return BadRequest("Body is required.");
            var newName = (author.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(newName))
                return BadRequest("Name is required.");

            bool duplicate = _context.Authors.Any(a =>
                a.Id != id && a.Name != null && a.Name.ToLowerInvariant() == newName.ToLowerInvariant());
            if (duplicate) return Conflict("Another author with this name already exists.");

            existing.Name = newName;
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
            var existing = _context.Authors.Find(id);
            if (existing is null) return NotFound();

            if (_context.Books.Any(b => b.AuthorId == id))
                return BadRequest("Cannot delete author that has books. Remove or reassign books first.");

            _context.Authors.Remove(existing);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
