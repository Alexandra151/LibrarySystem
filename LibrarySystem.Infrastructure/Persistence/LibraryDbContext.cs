using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibrarySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Infrastructure.Persistence
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

        public DbSet<Author> Authors => Set<Author>();
        public DbSet<Book> Books => Set<Book>();
        public DbSet<Loan> Loans => Set<Loan>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Author>().HasData(
                new Author { Id = 1, Name = "Adam Mickiewicz" },
                new Author { Id = 2, Name = "Henryk Sienkiewicz" }
            );

            modelBuilder.Entity<Book>().HasData(
                new Book { Id = 1, Title = "Pan Tadeusz", AuthorId = 1, CopiesTotal = 3, CopiesAvailable = 3 },
                new Book { Id = 2, Title = "Quo Vadis", AuthorId = 2, CopiesTotal = 2, CopiesAvailable = 2 }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}

