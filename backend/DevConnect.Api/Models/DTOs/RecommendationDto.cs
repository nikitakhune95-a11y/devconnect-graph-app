namespace DevConnect.Api.Models.DTOs
{
    /// <summary>
    /// Result of matching a developer against a project's required skills.
    /// Used by GET /api/graph/recommendations/{projectId}
    /// </summary>
    public class RecommendationDto
    {
        public string DeveloperId { get; set; } = string.Empty;
        public string DeveloperName { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public int MatchingSkillsCount { get; set; }
        public List<string> MatchingSkills { get; set; } = new();
        public List<string> MissingSkills { get; set; } = new();
        public bool AlreadyOnProject { get; set; }
    }
}
