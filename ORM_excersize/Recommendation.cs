namespace ORM.Models
{
    public class Recommendation
    {
        public int Id { get; set; } // Optional: for database ID

        public string Type { get; set; } = string.Empty; // "trip", "adventure", "equipment"

        public string Description { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.Now;

        public string Reporter { get; set; } = string.Empty; // e.g., name or username of the person who submitted it
    }
}
