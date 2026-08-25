using DevConnect.Api.Services;
using DevConnect.Api.Services.QueryRepository;
using Microsoft.AspNetCore.Mvc;

namespace DevConnect.Api.Controllers
{
    /// <summary>
    /// Endpoints that showcase what a graph database is genuinely good at:
    /// multi-hop traversals and relationship-based recommendations.
    /// </summary>
    [ApiController]
    [Route("api/graph")]
    public class GraphController : ControllerBase
    {
        private readonly RecommendationQueries _recommendationQueries;
        private readonly INeo4jService _db;

        public GraphController(RecommendationQueries recommendationQueries, INeo4jService db)
        {
            _recommendationQueries = recommendationQueries;
            _db = db;
        }

        /// <summary>
        /// GET /api/graph/recommendations/{projectId}
        /// Ranks developers not yet on the project by how many required skills they already have.
        /// </summary>
        [HttpGet("recommendations/{projectId}")]
        public async Task<IActionResult> GetRecommendations(string projectId)
        {
            var recommendations = await _recommendationQueries.GetRecommendationsForProjectAsync(projectId);
            return Ok(recommendations);
        }

        /// <summary>
        /// GET /api/graph/skill-gaps/{developerId}
        /// 2-hop traversal: skills held by this developer's collaborators that they don't have yet.
        /// </summary>
        [HttpGet("skill-gaps/{developerId}")]
        public async Task<IActionResult> GetSkillGaps(string developerId)
        {
            var gaps = await _recommendationQueries.GetSkillGapsThroughNetworkAsync(developerId);
            return Ok(gaps);
        }

        /// <summary>
        /// GET /api/graph/path?fromDevId=dev001&amp;toDevId=dev006
        /// Shortest collaboration path between two developers (variable-length traversal).
        /// </summary>
        [HttpGet("path")]
        public async Task<IActionResult> GetShortestPath([FromQuery] string fromDevId, [FromQuery] string toDevId)
        {
            if (string.IsNullOrWhiteSpace(fromDevId) || string.IsNullOrWhiteSpace(toDevId))
                return BadRequest(new { error = "Both fromDevId and toDevId query parameters are required." });

            var path = await _recommendationQueries.GetShortestPathAsync(fromDevId, toDevId);
            if (path is null)
                return NotFound(new { error = "No collaboration path exists between these developers." });

            return Ok(path);
        }

        /// <summary>GET /api/graph/health — DB connectivity check for the frontend's error/empty states.</summary>
        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            var isHealthy = await _db.VerifyConnectivityAsync();
            return isHealthy
                ? Ok(new { status = "connected" })
                : StatusCode(503, new { status = "unreachable" });
        }
    }
}
