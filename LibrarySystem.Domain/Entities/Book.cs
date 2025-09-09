using System.Text.Json.Serialization;

namespace LibrarySystem.Domain.Entities
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;

        public int AuthorId { get; set; }

        [JsonIgnore]                 
        public Author? Author { get; set; }  

        public int CopiesTotal { get; set; } = 1;
        public int CopiesAvailable { get; set; } = 1;
    }
}
