using DevConnect.Api.Models.DTOs;
using Neo4j.Driver;

namespace DevConnect.Api.Services.QueryRepository
{
    public class RecommendationQueries
    {
        private readonly INeo4jService _db;

        public RecommendationQueries(INeo4jService db)
        {
            _db = db;
        }

        /// <summary>
        /// THE "AWKWARD IN SQL" QUERY.
        /// For a given project, find developers who already cover most of the
        /// required skills but are NOT yet on the team — ranked by how many
        /// matching skills they have. In SQL this needs a chain of self-joins
        /// and set-difference logic across three tables; here it's one pattern.
        /// </summary>
        public async Task<List<RecommendationDto>> GetRecommendationsForProjectAsync(string projectId)
        {
            const string cypher = @"
                MATCH (p:Project {id: $projectId})-[:REQUIRES]->(required:Skill)
                WITH p, collect(required) AS requiredSkills
                MATCH (d:Developer)-[:HAS_SKILL]->(s:Skill)
                WHERE s IN requiredSkills
                WITH p, d, requiredSkills, collect(DISTINCT s) AS matchingSkills
                OPTIONAL MATCH (d)-[:WORKED_ON]->(p)
                WITH d, requiredSkills, matchingSkills, count(*) > 0 AS alreadyOnProject
                WHERE NOT alreadyOnProject
                RETURN d.id AS developerId,
                       d.name AS developerName,
                       d.experienceYears AS experienceYears,
                       size(matchingSkills) AS matchingSkillsCount,
                       [s IN matchingSkills | s.name] AS matchingSkillNames,
                       [s IN requiredSkills WHERE NOT s IN matchingSkills | s.name] AS missingSkillNames,
                       alreadyOnProject
                ORDER BY matchingSkillsCount DESC, experienceYears DESC
                LIMIT 10";

            return await _db.RunQueryAsync(
                cypher,
                new Dictionary<string, object> { ["projectId"] = projectId },
                record => new RecommendationDto
                {
                    DeveloperId = record["developerId"].As<string>(),
                    DeveloperName = record["developerName"].As<string>(),
                    ExperienceYears = record["experienceYears"].As<int>(),
                    MatchingSkillsCount = record["matchingSkillsCount"].As<int>(),
                    MatchingSkills = record["matchingSkillNames"].As<List<object>>().Select(x => x.As<string>()).ToList(),
                    MissingSkills = record["missingSkillNames"].As<List<object>>().Select(x => x.As<string>()).ToList(),
                    AlreadyOnProject = record["alreadyOnProject"].As<bool>()
                });
        }

        /// <summary>
        /// THE MULTI-HOP TRAVERSAL QUERY (2+ hops).
        /// Given a developer, find "skill neighbours" they don't have yet —
        /// skills held by people they've collaborated with, reached by walking
        /// Developer -> COLLABORATED_WITH -> Developer -> HAS_SKILL -> Skill.
        /// This is a 2-hop pattern that would require multiple joins + a
        /// NOT EXISTS subquery in SQL.
        /// </summary>
        public async Task<List<Dictionary<string, object>>> GetSkillGapsThroughNetworkAsync(string developerId)
        {
            const string cypher = @"
                MATCH (me:Developer {id: $developerId})-[:COLLABORATED_WITH]-(peer:Developer)
                      -[:HAS_SKILL]->(peerSkill:Skill)
                WHERE NOT (me)-[:HAS_SKILL]->(peerSkill)
                RETURN peerSkill.id AS skillId,
                       peerSkill.name AS skillName,
                       peerSkill.category AS category,
                       collect(DISTINCT peer.name) AS knownBy
                ORDER BY size(knownBy) DESC";

            return await _db.RunQueryAsync(
                cypher,
                new Dictionary<string, object> { ["developerId"] = developerId },
                record => new Dictionary<string, object>
                {
                    ["skillId"] = record["skillId"].As<string>(),
                    ["skillName"] = record["skillName"].As<string>(),
                    ["category"] = record["category"].As<string>(),
                    ["knownBy"] = record["knownBy"].As<List<object>>().Select(x => x.As<string>()).ToList()
                });
        }

        /// <summary>
        /// Shortest path between two developers through the collaboration/
        /// project graph — a variable-length multi-hop traversal.
        /// </summary>
        public async Task<CollaborationPathDto?> GetShortestPathAsync(string fromDeveloperId, string toDeveloperId)
        {
            const string cypher = @"
                MATCH path = shortestPath(
                    (a:Developer {id: $fromId})-[:COLLABORATED_WITH*1..5]-(b:Developer {id: $toId})
                )
                RETURN path";

            var results = await _db.RunQueryAsync(
                cypher,
                new Dictionary<string, object> { ["fromId"] = fromDeveloperId, ["toId"] = toDeveloperId },
                record =>
                {
                    var path = record["path"].As<IPath>();
                    var dto = new CollaborationPathDto { HopCount = path.Relationships.Count };

                    foreach (var node in path.Nodes)
                    {
                        dto.Nodes.Add(new PathNodeDto
                        {
                            Label = node.Labels.FirstOrDefault() ?? "",
                            Id = node.Properties.GetValueOrDefault("id")?.As<string>() ?? "",
                            Name = node.Properties.GetValueOrDefault("name")?.As<string>() ?? ""
                        });
                    }

                    dto.RelationshipTypes = path.Relationships.Select(r => r.Type).ToList();
                    return dto;
                });

            return results.FirstOrDefault();
        }
    }
}
