using System.Text.Json.Serialization;

namespace LibrarySystem.Domain.Entities
{
    public class Loan
    {
        public int Id { get; set; }
        public int BookId { get; set; }

        [JsonIgnore]
        public Book? Book { get; set; }

        public DateTime LoanDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(14);
        public DateTime? ReturnDate { get; set; }
    }
}
