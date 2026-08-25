namespace DevConnect.Api.Models.DTOs
{
    /// <summary>
    /// Represents a multi-hop path between two developers, e.g. through
    /// shared projects and collaborators.
    /// Used by GET /api/graph/path?fromDevId=&toDevId=
    /// </summary>
    public class CollaborationPathDto
    {
        public List<PathNodeDto> Nodes { get; set; } = new();
        public List<string> RelationshipTypes { get; set; } = new();
        public int HopCount { get; set; }
    }

    public class PathNodeDto
    {
        public string Label { get; set; } = string.Empty;  // "Developer", "Project", "Skill"
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
