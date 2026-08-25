using DevConnect.Api.Services.QueryRepository;
using Microsoft.AspNetCore.Mvc;

namespace DevConnect.Api.Controllers
{
    [ApiController]
    [Route("api/projects")]
    public class ProjectsController : ControllerBase
    {
        private readonly ProjectQueries _projectQueries;

        public ProjectsController(ProjectQueries projectQueries)
        {
            _projectQueries = projectQueries;
        }

        /// <summary>GET /api/projects — list all projects.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var projects = await _projectQueries.GetAllProjectsAsync();
            return Ok(projects);
        }

        /// <summary>GET /api/projects/{id} — a single project's details.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var project = await _projectQueries.GetProjectByIdAsync(id);
            if (project is null)
                return NotFound(new { error = $"Project '{id}' was not found." });

            return Ok(project);
        }

        /// <summary>GET /api/projects/{id}/skills — required skills, flagged by whether the current team covers them.</summary>
        [HttpGet("{id}/skills")]
        public async Task<IActionResult> GetRequiredSkills(string id)
        {
            var skills = await _projectQueries.GetRequiredSkillsAsync(id);
            return Ok(skills);
        }

        /// <summary>GET /api/projects/{id}/team — developers currently on this project.</summary>
        [HttpGet("{id}/team")]
        public async Task<IActionResult> GetTeam(string id)
        {
            var team = await _projectQueries.GetTeamAsync(id);
            return Ok(team);
        }
    }
}
