using DevConnect.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

namespace DevConnect.Api.Controllers
{
    [ApiController]
    [Route("api/skills")]
    public class SkillsController : ControllerBase
    {
        private readonly INeo4jService _db;

        public SkillsController(INeo4jService db)
        {
            _db = db;
        }

        /// <summary>GET /api/skills — list all skills grouped by category.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            const string cypher = @"
                MATCH (s:Skill)
                OPTIONAL MATCH (d:Developer)-[:HAS_SKILL]->(s)
                RETURN s.id AS id, s.name AS name, s.category AS category, count(d) AS developerCount
                ORDER BY s.category, s.name";

            var skills = await _db.RunQueryAsync(cypher, new Dictionary<string, object>(), record => new
            {
                Id = record["id"].As<string>(),
                Name = record["name"].As<string>(),
                Category = record["category"].As<string>(),
                DeveloperCount = record["developerCount"].As<int>()
            });

            return Ok(skills);
        }

        /// <summary>GET /api/skills/{id}/developers — developers who have this skill.</summary>
        [HttpGet("{id}/developers")]
        public async Task<IActionResult> GetDevelopersWithSkill(string id)
        {
            const string cypher = @"
                MATCH (d:Developer)-[r:HAS_SKILL]->(s:Skill {id: $skillId})
                RETURN d.id AS developerId, d.name AS name, r.proficiency AS proficiency
                ORDER BY r.proficiency DESC";

            var developers = await _db.RunQueryAsync(
                cypher,
                new Dictionary<string, object> { ["skillId"] = id },
                record => new
                {
                    DeveloperId = record["developerId"].As<string>(),
                    Name = record["name"].As<string>(),
                    Proficiency = record["proficiency"].As<string>()
                });

            return Ok(developers);
        }
    }
}
