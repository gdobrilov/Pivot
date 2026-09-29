/** Three faces of a cube, drawn in the brand colours. */
export function Logo() {
  return (
    <svg className="logo" viewBox="0 0 40 44" width="36" height="40" aria-hidden="true">
      <polygon points="20,2 38,12 20,22 2,12" fill="#ffffff" stroke="#111" strokeWidth="1.5" />
      <polygon points="2,12 20,22 20,42 2,32" fill="#4caf50" stroke="#111" strokeWidth="1.5" />
      <polygon points="38,12 20,22 20,42 38,32" fill="#e53935" stroke="#111" strokeWidth="1.5" />
    </svg>
  );
}
