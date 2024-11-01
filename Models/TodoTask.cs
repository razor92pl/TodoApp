namespace TodoApp.Models
{
    public class TodoTask
    {
        public int Id { get; set; }
        public string? Title { get; set; } // Nullable
        public string? Description { get; set; } // Nullable
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }
    }
}