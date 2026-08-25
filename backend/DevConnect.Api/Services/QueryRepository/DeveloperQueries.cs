using DevConnect.Api.Models.DTOs;
using Neo4j.Driver;

namespace DevConnect.Api.Services.QueryRepository
{
    public class DeveloperQueries
    {
        private readonly INeo4jService _db;

        public DeveloperQueries(INeo4jService db)
        {
            _db = db;
        }

        public async Task<List<DeveloperDto>> GetAllDevelopersAsync()
        {
            const string cypher = @"
                MATCH (d:Developer)
                OPTIONAL MATCH (d)-[hs:HAS_SKILL]->(s:Skill)
                OPTIONAL MATCH (d)-[:WORKED_ON]->(p:Project)
                RETURN d, collect(DISTINCT {skillId: s.id, name: s.name, proficiency: hs.proficiency}) AS skills,
                       collect(DISTINCT p.name) AS projects
                ORDER BY d.name";

            return await _db.RunQueryAsync(cypher, new Dictionary<string, object>(), record =>
            {
                var node = record["d"].As<INode>();
                return MapDeveloperNode(node, record);
            });
        }

        public async Task<DeveloperDto?> GetDeveloperByIdAsync(string developerId)
        {
            const string cypher = @"
                MATCH (d:Developer {id: $developerId})
                OPTIONAL MATCH (d)-[hs:HAS_SKILL]->(s:Skill)
                OPTIONAL MATCH (d)-[:WORKED_ON]->(p:Project)
                RETURN d, collect(DISTINCT {skillId: s.id, name: s.name, proficiency: hs.proficiency}) AS skills,
                       collect(DISTINCT p.name) AS projects";

            var results = await _db.RunQueryAsync(
                cypher,
                new Dictionary<string, object> { ["developerId"] = developerId },
                record => MapDeveloperNode(record["d"].As<INode>(), record));

            return results.FirstOrDefault();
        }

        /// <summary>
        /// Finds developers connected to a given developer through shared projects
        /// (COLLABORATED_WITH), i.e. their direct professional network.
        /// </summary>
        public async Task<List<DeveloperDto>> GetCollaboratorsAsync(string developerId)
        {
            const string cypher = @"
                MATCH (d:Developer {id: $developerId})-[:COLLABORATED_WITH]-(collaborator:Developer)
                OPTIONAL MATCH (collaborator)-[hs:HAS_SKILL]->(s:Skill)
                OPTIONAL MATCH (collaborator)-[:WORKED_ON]->(p:Project)
                RETURN collaborator AS d,
                       collect(DISTINCT {skillId: s.id, name: s.name, proficiency: hs.proficiency}) AS skills,
                       collect(DISTINCT p.name) AS projects";

            return await _db.RunQueryAsync(
                cypher,
                new Dictionary<string, object> { ["developerId"] = developerId },
                record => MapDeveloperNode(record["d"].As<INode>(), record));
        }

        private static DeveloperDto MapDeveloperNode(INode node, IRecord record)
        {
            var dto = new DeveloperDto
            {
                Id = node.Properties["id"].As<string>(),
                Name = node.Properties["name"].As<string>(),
                Email = node.Properties.GetValueOrDefault("email")?.As<string>() ?? "",
                ExperienceYears = node.Properties.GetValueOrDefault("experienceYears") is { } exp ? exp.As<int>() : 0,
                Location = node.Properties.GetValueOrDefault("location")?.As<string>() ?? "",
                Bio = node.Properties.GetValueOrDefault("bio")?.As<string>() ?? ""
            };

            var skillMaps = record["skills"].As<List<object>>();
            foreach (var item in skillMaps)
            {
                var map = item.As<Dictionary<string, object>>();
                if (map["skillId"] is null) continue;

                dto.Skills.Add(new SkillWithProficiencyDto
                {
                    SkillId = map["skillId"]?.As<string>() ?? "",
                    Name = map["name"]?.As<string>() ?? "",
                    Proficiency = map["proficiency"]?.As<string>() ?? ""
                });
            }

            dto.Projects = record["projects"].As<List<object>>()
                .Where(p => p is not null)
                .Select(p => p.As<string>())
                .ToList();

            return dto;
        }
    }
}
