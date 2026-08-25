namespace DevConnect.Api.Models.DTOs
{
    public class DeveloperDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public List<SkillWithProficiencyDto> Skills { get; set; } = new();
        public List<string> Projects { get; set; } = new();
    }

    public class SkillWithProficiencyDto
    {
        public string SkillId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Proficiency { get; set; } = string.Empty;
    }
}
