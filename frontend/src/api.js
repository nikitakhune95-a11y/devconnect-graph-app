const API_URL = import.meta.env.VITE_API_URL || "http://localhost:5000";

class ApiError extends Error {
  constructor(message, status) {
    super(message);
    this.status = status;
  }
}

async function request(path) {
  let res;
  try {
    res = await fetch(`${API_URL}${path}`);
  } catch (err) {
    // Network-level failure (backend down, wrong URL, CORS block, etc.)
    throw new ApiError(
      "Can't reach the DevConnect API. Check that the backend is running and VITE_API_URL is correct.",
      0
    );
  }

  if (!res.ok) {
    let detail = "";
    try {
      const body = await res.json();
      detail = body.error || "";
    } catch {
      // response wasn't JSON — ignore
    }
    throw new ApiError(detail || `Request failed (${res.status})`, res.status);
  }

  return res.json();
}

export const api = {
  health: () => request("/api/graph/health"),
  developers: () => request("/api/developers"),
  developer: (id) => request(`/api/developers/${encodeURIComponent(id)}`),
  collaborators: (id) => request(`/api/developers/${encodeURIComponent(id)}/collaborators`),
  projects: () => request("/api/projects"),
  project: (id) => request(`/api/projects/${encodeURIComponent(id)}`),
  projectSkills: (id) => request(`/api/projects/${encodeURIComponent(id)}/skills`),
  projectTeam: (id) => request(`/api/projects/${encodeURIComponent(id)}/team`),
  recommendations: (projectId) => request(`/api/graph/recommendations/${encodeURIComponent(projectId)}`),
  skillGaps: (developerId) => request(`/api/graph/skill-gaps/${encodeURIComponent(developerId)}`),
  path: (fromDevId, toDevId) =>
    request(`/api/graph/path?fromDevId=${encodeURIComponent(fromDevId)}&toDevId=${encodeURIComponent(toDevId)}`),
};

export { ApiError };
