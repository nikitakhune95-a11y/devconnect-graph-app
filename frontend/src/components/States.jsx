export function Loading({ label = "Loading" }) {
  return (
    <div className="state-box" role="status" aria-live="polite">
      <div className="spinner" aria-hidden="true" />
      <p>{label}…</p>
    </div>
  );
}

export function EmptyState({ title = "Nothing here yet", detail }) {
  return (
    <div className="state-box">
      <h3>{title}</h3>
      {detail && <p>{detail}</p>}
    </div>
  );
}

export function ErrorState({ error, onRetry }) {
  const message = error?.message || "Something went wrong.";
  return (
    <div className="state-box error" role="alert">
      <h3>Couldn't load this</h3>
      <p>{message}</p>
      {onRetry && (
        <button className="btn" style={{ marginTop: 14 }} onClick={onRetry}>
          Try again
        </button>
      )}
    </div>
  );
}
