using DevConnect.Api.Models;
using Neo4j.Driver;

namespace DevConnect.Api.Services.QueryRepository
{
    public class ProjectQueries
    {
        private readonly INeo4jService _db;

        public ProjectQueries(INeo4jService db)
        {
            _db = db;
        }

        public async Task<List<Project>> GetAllProjectsAsync()
        {
            const string cypher = @"
                MATCH (p:Project)
                RETURN p
                ORDER BY p.startDate DESC";

            return await _db.RunQueryAsync(cypher, new Dictionary<string, object>(), record =>
                MapProjectNode(record["p"].As<INode>()));
        }

        public async Task<Project?> GetProjectByIdAsync(string projectId)
        {
            const string cypher = @"
                MATCH (p:Project {id: $projectId})
                RETURN p";

            var results = await _db.RunQueryAsync(
                cypher,
                new Dictionary<string, object> { ["projectId"] = projectId },
                record => MapProjectNode(record["p"].As<INode>()));

            return results.FirstOrDefault();
        }

        /// <summary>
        /// Required skills for a project, with a flag for whether any current
        /// team member already covers that skill.
        /// </summary>
        public async Task<List<Dictionary<string, object>>> GetRequiredSkillsAsync(string projectId)
        {
            const string cypher = @"
                MATCH (p:Project {id: $projectId})-[:REQUIRES]->(s:Skill)
                OPTIONAL MATCH (p)<-[:WORKED_ON]-(:Developer)-[:HAS_SKILL]->(s)
                RETURN s.id AS skillId, s.name AS name, s.category AS category,
                       count(*) > 0 AS isCovered";

            return await _db.RunQueryAsync(
                cypher,
                new Dictionary<string, object> { ["projectId"] = projectId },
                record => new Dictionary<string, object>
                {
                    ["skillId"] = record["skillId"].As<string>(),
                    ["name"] = record["name"].As<string>(),
                    ["category"] = record["category"].As<string>(),
                    ["isCovered"] = record["isCovered"].As<bool>()
                });
        }

        /// <summary>
        /// Team members currently on a project along with their role.
        /// </summary>
        public async Task<List<Dictionary<string, object>>> GetTeamAsync(string projectId)
        {
            const string cypher = @"
                MATCH (d:Developer)-[r:WORKED_ON]->(p:Project {id: $projectId})
                RETURN d.id AS developerId, d.name AS name, r.role AS role
                ORDER BY d.name";

            return await _db.RunQueryAsync(
                cypher,
                new Dictionary<string, object> { ["projectId"] = projectId },
                record => new Dictionary<string, object>
                {
                    ["developerId"] = record["developerId"].As<string>(),
                    ["name"] = record["name"].As<string>(),
                    ["role"] = record["role"].As<string>()
                });
        }

        private static Project MapProjectNode(INode node)
        {
            return new Project
            {
                Id = node.Properties["id"].As<string>(),
                Name = node.Properties["name"].As<string>(),
                Description = node.Properties.GetValueOrDefault("description")?.As<string>() ?? "",
                Status = node.Properties.GetValueOrDefault("status")?.As<string>() ?? "",
                StartDate = node.Properties.GetValueOrDefault("startDate")?.As<string>() ?? ""
            };
        }
    }
}
