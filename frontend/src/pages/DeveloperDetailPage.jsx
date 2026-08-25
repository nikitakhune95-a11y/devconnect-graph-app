import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { api } from "../api";
import { Loading, EmptyState, ErrorState } from "../components/States";
import SkillBadge from "../components/SkillBadge";

export default function DeveloperDetailPage() {
  const { id } = useParams();
  const [dev, setDev] = useState({ status: "loading", data: null, error: null });
  const [collaborators, setCollaborators] = useState({ status: "loading", data: null, error: null });
  const [gaps, setGaps] = useState({ status: "loading", data: null, error: null });

  useEffect(() => {
    setDev({ status: "loading", data: null, error: null });
    setCollaborators({ status: "loading", data: null, error: null });
    setGaps({ status: "loading", data: null, error: null });

    api
      .developer(id)
      .then((data) => setDev({ status: "ok", data, error: null }))
      .catch((error) => setDev({ status: "error", data: null, error }));

    api
      .collaborators(id)
      .then((data) => setCollaborators({ status: "ok", data, error: null }))
      .catch((error) => setCollaborators({ status: "error", data: null, error }));

    api
      .skillGaps(id)
      .then((data) => setGaps({ status: "ok", data, error: null }))
      .catch((error) => setGaps({ status: "error", data: null, error }));
  }, [id]);

  if (dev.status === "loading") {
    return (
      <div className="container">
        <Loading label="Loading developer" />
      </div>
    );
  }

  if (dev.status === "error") {
    return (
      <div className="container">
        <ErrorState error={dev.error} />
      </div>
    );
  }

  const d = dev.data;

  return (
    <div className="container">
      <Link to="/" className="back-link">
        ← All developers
      </Link>

      <div className="detail-header">
        <div>
          <span className="eyebrow">Developer</span>
          <h1>{d.name}</h1>
          <p style={{ color: "var(--ink-soft)", marginTop: 6, maxWidth: "60ch" }}>{d.bio}</p>
        </div>
      </div>

      <div className="stat-row">
        <div className="stat">
          <span className="value">{d.experienceYears}</span>
          <span className="label">Years exp.</span>
        </div>
        <div className="stat">
          <span className="value">{d.location}</span>
          <span className="label">Location</span>
        </div>
        <div className="stat">
          <span className="value">{d.skills.length}</span>
          <span className="label">Skills</span>
        </div>
        <div className="stat">
          <span className="value">{d.projects.length}</span>
          <span className="label">Projects</span>
        </div>
      </div>

      <div className="section">
        <h2>Skills</h2>
        <p className="section-sub">HAS_SKILL relationships, with proficiency.</p>
        {d.skills.length === 0 ? (
          <EmptyState title="No skills recorded" />
        ) : (
          <div className="badge-row">
            {d.skills.map((s) => (
              <SkillBadge key={s.skillId} name={`${s.name} · ${s.proficiency}`} />
            ))}
          </div>
        )}
      </div>

      <div className="section">
        <h2>Projects</h2>
        <p className="section-sub">WORKED_ON relationships.</p>
        {d.projects.length === 0 ? (
          <EmptyState title="Not on any projects yet" />
        ) : (
          <div className="badge-row">
            {d.projects.map((p) => (
              <span className="badge" key={p}>
                {p}
              </span>
            ))}
          </div>
        )}
      </div>

      <div className="section">
        <h2>Collaborators</h2>
        <p className="section-sub">Direct network — developers reached via COLLABORATED_WITH.</p>
        {collaborators.status === "loading" && <Loading label="Loading collaborators" />}
        {collaborators.status === "error" && <ErrorState error={collaborators.error} />}
        {collaborators.status === "ok" && collaborators.data.length === 0 && (
          <EmptyState title="No collaborators yet" detail="This developer hasn't shared a project with anyone yet." />
        )}
        {collaborators.status === "ok" && collaborators.data.length > 0 && (
          <div className="grid">
            {collaborators.data.map((c) => (
              <Link key={c.id} to={`/developers/${c.id}`} className="card">
                <h3>{c.name}</h3>
                <div className="meta">{c.location}</div>
                <div className="badge-row">
                  {c.skills.slice(0, 3).map((s) => (
                    <SkillBadge key={s.skillId} name={s.name} />
                  ))}
                </div>
              </Link>
            ))}
          </div>
        )}
      </div>

      <div className="section">
        <h2>Skill gaps through the network</h2>
        <p className="section-sub">
          2-hop traversal: Developer → COLLABORATED_WITH → Developer → HAS_SKILL → Skill — skills {d.name.split(" ")[0]}'s
          collaborators know that they don't, yet.
        </p>
        {gaps.status === "loading" && <Loading label="Walking the network" />}
        {gaps.status === "error" && <ErrorState error={gaps.error} />}
        {gaps.status === "ok" && gaps.data.length === 0 && (
          <EmptyState title="No gaps found" detail="This developer already has every skill their collaborators have." />
        )}
        {gaps.status === "ok" && gaps.data.length > 0 && (
          <div>
            {gaps.data.map((g) => (
              <div className="gap-row" key={g.skillId}>
                <SkillBadge name={g.skillName} variant="gap" />
                <span className="known-by">Known by {g.knownBy.join(", ")}</span>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
