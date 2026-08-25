import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../api";
import { Loading, EmptyState, ErrorState } from "../components/States";

export default function PathFinderPage() {
  const [devs, setDevs] = useState({ status: "loading", data: null, error: null });
  const [fromId, setFromId] = useState("");
  const [toId, setToId] = useState("");
  const [path, setPath] = useState({ status: "idle", data: null, error: null });

  useEffect(() => {
    api
      .developers()
      .then((data) => {
        setDevs({ status: "ok", data, error: null });
        if (data.length >= 2) {
          setFromId(data[0].id);
          setToId(data[1].id);
        }
      })
      .catch((error) => setDevs({ status: "error", data: null, error }));
  }, []);

  function findPath(e) {
    e.preventDefault();
    if (!fromId || !toId || fromId === toId) return;
    setPath({ status: "loading", data: null, error: null });
    api
      .path(fromId, toId)
      .then((data) => setPath({ status: "ok", data, error: null }))
      .catch((error) => setPath({ status: "error", data: null, error }));
  }

  return (
    <div className="container">
      <div className="page-header">
        <span className="eyebrow">Variable-length traversal</span>
        <h1>Collaboration path finder</h1>
        <p>
          Find the shortest chain of COLLABORATED_WITH relationships between any two developers — the kind of
          query that needs a recursive CTE in SQL, and one line of Cypher here.
        </p>
      </div>

      {devs.status === "loading" && <Loading label="Loading developers" />}
      {devs.status === "error" && <ErrorState error={devs.error} />}

      {devs.status === "ok" && devs.data.length < 2 && (
        <EmptyState title="Need at least two developers" detail="Seed the database first." />
      )}

      {devs.status === "ok" && devs.data.length >= 2 && (
        <>
          <form className="path-form" onSubmit={findPath}>
            <div className="field">
              <label htmlFor="from">From</label>
              <select id="from" value={fromId} onChange={(e) => setFromId(e.target.value)}>
                {devs.data.map((d) => (
                  <option key={d.id} value={d.id}>
                    {d.name}
                  </option>
                ))}
              </select>
            </div>
            <div className="field">
              <label htmlFor="to">To</label>
              <select id="to" value={toId} onChange={(e) => setToId(e.target.value)}>
                {devs.data.map((d) => (
                  <option key={d.id} value={d.id}>
                    {d.name}
                  </option>
                ))}
              </select>
            </div>
            <button className="btn" type="submit" disabled={fromId === toId}>
              Find path
            </button>
          </form>

          {fromId === toId && (
            <p style={{ color: "var(--ink-soft)", fontSize: 13.5, marginTop: -16, marginBottom: 24 }}>
              Pick two different developers.
            </p>
          )}

          {path.status === "loading" && <Loading label="Walking the collaboration graph" />}
          {path.status === "error" && <ErrorState error={path.error} onRetry={findPath} />}
          {path.status === "ok" && path.data === null && (
            <EmptyState
              title="No path found"
              detail="These two developers aren't connected through shared projects within 5 hops."
            />
          )}
          {path.status === "ok" && path.data && (
            <div className="card" style={{ padding: 28 }}>
              <div className="stat-row" style={{ marginBottom: 8 }}>
                <div className="stat">
                  <span className="value">{path.data.hopCount}</span>
                  <span className="label">Hops</span>
                </div>
                <div className="stat">
                  <span className="value">{path.data.nodes.length}</span>
                  <span className="label">Developers in chain</span>
                </div>
              </div>
              <div className="path-viz">
                {path.data.nodes.map((n, i) => (
                  <div style={{ display: "flex", alignItems: "center" }} key={n.id}>
                    <Link to={`/developers/${n.id}`} className="path-node" style={{ textDecoration: "none", color: "inherit" }}>
                      <span className="dot">
                        {n.name
                          .split(" ")
                          .map((w) => w[0])
                          .slice(0, 2)
                          .join("")}
                      </span>
                      <span className="name">{n.name}</span>
                      <span className="type">{n.label}</span>
                    </Link>
                    {i < path.data.nodes.length - 1 && <div className="path-edge" />}
                  </div>
                ))}
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}
