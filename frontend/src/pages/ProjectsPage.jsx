import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../api";
import { Loading, EmptyState, ErrorState } from "../components/States";

export default function ProjectsPage() {
  const [state, setState] = useState({ status: "loading", data: null, error: null });

  function load() {
    setState({ status: "loading", data: null, error: null });
    api
      .projects()
      .then((data) => setState({ status: "ok", data, error: null }))
      .catch((error) => setState({ status: "error", data: null, error }));
  }

  useEffect(load, []);

  return (
    <div className="container">
      <div className="page-header">
        <span className="eyebrow">Graph explorer</span>
        <h1>Projects</h1>
        <p>Each project's required skills and current team, drawn from REQUIRES and WORKED_ON relationships.</p>
      </div>

      {state.status === "loading" && <Loading label="Loading projects" />}
      {state.status === "error" && <ErrorState error={state.error} onRetry={load} />}
      {state.status === "ok" && state.data.length === 0 && (
        <EmptyState title="No projects yet" detail="Run the seed script in data/Seeder to load sample data." />
      )}

      {state.status === "ok" && state.data.length > 0 && (
        <div className="grid">
          {state.data.map((p) => (
            <Link key={p.id} to={`/projects/${p.id}`} className="card">
              <h3>{p.name}</h3>
              <div className="meta">
                {p.status} · started {p.startDate}
              </div>
              <p style={{ fontSize: 13.5, color: "var(--ink-soft)", margin: 0 }}>{p.description}</p>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
