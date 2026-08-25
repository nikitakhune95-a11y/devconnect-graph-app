export default function SkillBadge({ name, variant = "default" }) {
  const cls = variant === "gap" ? "badge gap" : variant === "uncovered" ? "badge uncovered" : "badge covered";
  return (
    <span className={cls}>
      <span className="dot" aria-hidden="true" />
      {name}
    </span>
  );
}
