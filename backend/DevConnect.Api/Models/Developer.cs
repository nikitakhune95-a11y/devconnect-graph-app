namespace DevConnect.Api.Models
{
    public class Developer
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
    }
}
