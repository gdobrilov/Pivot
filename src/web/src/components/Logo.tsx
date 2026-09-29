import { COLOUR_HEX } from '../domain/colours';

/** Three faces of a solved cube: white up, green front, red right. */
export function Logo() {
  return (
    <svg className="logo" viewBox="0 0 40 44" width="36" height="40" aria-hidden="true">
      <polygon points="20,2 38,12 20,22 2,12" fill={COLOUR_HEX.White} stroke="#111" strokeWidth="1.5" />
      <polygon points="2,12 20,22 20,42 2,32" fill={COLOUR_HEX.Green} stroke="#111" strokeWidth="1.5" />
      <polygon points="38,12 20,22 20,42 38,32" fill={COLOUR_HEX.Red} stroke="#111" strokeWidth="1.5" />
    </svg>
  );
}
