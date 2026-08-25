import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../api";
import { Loading, EmptyState, ErrorState } from "../components/States";
import SkillBadge from "../components/SkillBadge";

export default function DevelopersPage() {
  const [state, setState] = useState({ status: "loading", data: null, error: null });
  const [query, setQuery] = useState("");

  function load() {
    setState({ status: "loading", data: null, error: null });
    api
      .developers()
      .then((data) => setState({ status: "ok", data, error: null }))
      .catch((error) => setState({ status: "error", data: null, error }));
  }

  useEffect(load, []);

  const filtered = useMemo(() => {
    if (!state.data) return [];
    const q = query.trim().toLowerCase();
    if (!q) return state.data;
    return state.data.filter(
      (d) =>
        d.name.toLowerCase().includes(q) ||
        d.location.toLowerCase().includes(q) ||
        d.skills.some((s) => s.name.toLowerCase().includes(q))
    );
  }, [state.data, query]);

  return (
    <div className="container">
      <div className="page-header">
        <span className="eyebrow">Graph explorer</span>
        <h1>Developers</h1>
        <p>Every developer, the skills they hold, and the projects they've shipped — sourced straight from the graph.</p>
      </div>

      {state.status === "ok" && state.data.length > 0 && (
        <div className="field" style={{ marginBottom: 24, maxWidth: 320 }}>
          <label htmlFor="dev-search">Search by name, location or skill</label>
          <input
            id="dev-search"
            type="text"
            placeholder="e.g. Priya, Berlin, Kubernetes…"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
          />
        </div>
      )}

      {state.status === "loading" && <Loading label="Loading developers" />}
      {state.status === "error" && <ErrorState error={state.error} onRetry={load} />}
      {state.status === "ok" && state.data.length === 0 && (
        <EmptyState title="No developers yet" detail="Run the seed script in data/Seeder to load sample data." />
      )}
      {state.status === "ok" && state.data.length > 0 && filtered.length === 0 && (
        <EmptyState title="No matches" detail={`Nothing matches "${query}".`} />
      )}

      {state.status === "ok" && filtered.length > 0 && (
        <div className="grid">
          {filtered.map((d) => (
            <Link key={d.id} to={`/developers/${d.id}`} className="card">
              <h3>{d.name}</h3>
              <div className="meta">
                {d.location} · {d.experienceYears} yrs experience
              </div>
              <div className="badge-row">
                {d.skills.slice(0, 4).map((s) => (
                  <SkillBadge key={s.skillId} name={s.name} />
                ))}
                {d.skills.length > 4 && <SkillBadge name={`+${d.skills.length - 4} more`} />}
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
