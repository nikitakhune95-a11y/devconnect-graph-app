using DevConnect.Api.Services.QueryRepository;
using Microsoft.AspNetCore.Mvc;

namespace DevConnect.Api.Controllers
{
    [ApiController]
    [Route("api/developers")]
    public class DevelopersController : ControllerBase
    {
        private readonly DeveloperQueries _developerQueries;

        public DevelopersController(DeveloperQueries developerQueries)
        {
            _developerQueries = developerQueries;
        }

        /// <summary>GET /api/developers — list all developers with their skills and projects.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var developers = await _developerQueries.GetAllDevelopersAsync();
            return Ok(developers);
        }

        /// <summary>GET /api/developers/{id} — a single developer's full profile.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var developer = await _developerQueries.GetDeveloperByIdAsync(id);
            if (developer is null)
                return NotFound(new { error = $"Developer '{id}' was not found." });

            return Ok(developer);
        }

        /// <summary>GET /api/developers/{id}/collaborators — direct professional network.</summary>
        [HttpGet("{id}/collaborators")]
        public async Task<IActionResult> GetCollaborators(string id)
        {
            var collaborators = await _developerQueries.GetCollaboratorsAsync(id);
            return Ok(collaborators);
        }
    }
}
