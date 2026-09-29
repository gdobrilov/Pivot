import type { Face, Rotation, Turn } from './types';

const FACE_BY_LETTER: Record<string, Face> = { U: 'Up', L: 'Left', F: 'Front', R: 'Right', B: 'Back', D: 'Down' };

/** "F", "R'" or "U2" to a turn. Only used to animate an undo. */
export function parseMove(notation: string): Turn | null {
  const face = FACE_BY_LETTER[notation[0] ?? ''];
  if (!face) return null;
  const rotation: Rotation = notation.endsWith("'") ? 'AntiClockwise' : notation.endsWith('2') ? 'Half' : 'Clockwise';
  return { face, rotation };
}

export function inverse(turn: Turn): Turn {
  const rotation: Rotation =
    turn.rotation === 'Clockwise' ? 'AntiClockwise' : turn.rotation === 'AntiClockwise' ? 'Clockwise' : 'Half';
  return { face: turn.face, rotation };
}
