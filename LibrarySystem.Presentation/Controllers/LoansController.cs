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
    [Authorize(Roles = "Admin,Librarian")] 
    [Produces("application/json")]
    public class LoansController : ControllerBase
    {
        private readonly LibraryDbContext _context;
        public LoansController(LibraryDbContext context) => _context = context;

        public record CreateLoanRequest(int BookId, int Days);

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Loan>), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery] bool activeOnly = false)
        {
            var q = _context.Loans.AsNoTracking().AsQueryable();
            if (activeOnly) q = q.Where(l => l.ReturnDate == null);

            var items = q
                .OrderByDescending(l => l.LoanDate)
                .ToList();

            return Ok(items);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Loan), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetById(int id)
        {
            var loan = _context.Loans.AsNoTracking().FirstOrDefault(l => l.Id == id);
            return loan is null ? NotFound() : Ok(loan);
        }

        [HttpPost]
        [ProducesResponseType(typeof(Loan), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Create([FromBody] CreateLoanRequest req)
        {
            if (req is null) return BadRequest("Body is required.");
            if (req.Days < 1) return BadRequest("Days must be >= 1.");

            var book = _context.Books.Find(req.BookId);
            if (book is null) return NotFound($"Book {req.BookId} not found.");
            if (book.CopiesAvailable <= 0) return BadRequest("No copies available.");

            var loan = new Loan
            {
                BookId = book.Id,
                LoanDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(req.Days),
                ReturnDate = null
            };

            book.CopiesAvailable = Math.Max(0, book.CopiesAvailable - 1);

            _context.Loans.Add(loan);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = loan.Id }, loan);
        }

        [HttpPatch("{id:int}/return")]
        [ProducesResponseType(typeof(Loan), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Return(int id)
        {
            var loan = _context.Loans.Find(id);
            if (loan is null) return NotFound();

            if (loan.ReturnDate != null) return BadRequest("Already returned.");

            loan.ReturnDate = DateTime.UtcNow;

            var book = _context.Books.Find(loan.BookId);
            if (book != null)
            {
                if (book.CopiesAvailable < book.CopiesTotal)
                    book.CopiesAvailable += 1;
            }

            _context.SaveChanges();
            return Ok(loan);
        }
    }
}
