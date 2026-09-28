import type { Colour, CubeState, FacesSnapshot, LogEntry, RotationPreview } from '../domain/types';

const SYMBOLS: Record<string, Colour> = { W: 'White', O: 'Orange', G: 'Green', R: 'Red', B: 'Blue', Y: 'Yellow' };

/** Builds the six faces from facelet strings in U L F R B D order. */
export function facesFrom(up: string, left: string, front: string, right: string, back: string, down: string): FacesSnapshot {
  const toColours = (facelets: string) => [...facelets].map((symbol) => SYMBOLS[symbol]!);
  return {
    up: toColours(up),
    left: toColours(left),
    front: toColours(front),
    right: toColours(right),
    back: toColours(back),
    down: toColours(down),
  };
}

export const SOLVED: CubeState = {
  id: 'session-1',
  isSolved: true,
  version: 0,
  canUndo: false,
  effectiveMoves: [],
  faces: facesFrom('WWWWWWWWW', 'OOOOOOOOO', 'GGGGGGGGG', 'RRRRRRRRR', 'BBBBBBBBB', 'YYYYYYYYY'),
};

/** Expected state after the challenge sequence F R' U B' L D' (from the brief). */
export const CHALLENGE_RESULT: CubeState = {
  id: 'session-1',
  isSolved: false,
  version: 6,
  canUndo: true,
  effectiveMoves: ['F', "R'", 'U', "B'", 'L', "D'"],
  faces: facesFrom('ROGBWWBBB', 'GYYOOGBGO', 'ORROGWWWW', 'YBORRWOYR', 'YBWOBYYYW', 'GGBRYRRGG'),
};

/** Preview of F on the solved cube: the front face keeps its colour, twelve strip stickers change. */
export const FRONT_PREVIEW: RotationPreview = {
  move: 'F',
  after: facesFrom('WWWWWWOOO', 'OOYOOYOOY', 'GGGGGGGGG', 'WRRWRRWRR', 'BBBBBBBBB', 'RRRYYYYYY'),
  changes: [
    { face: 'Up', row: 2, column: 0, from: 'White', to: 'Orange' },
    { face: 'Up', row: 2, column: 1, from: 'White', to: 'Orange' },
    { face: 'Up', row: 2, column: 2, from: 'White', to: 'Orange' },
    { face: 'Left', row: 0, column: 2, from: 'Orange', to: 'Yellow' },
    { face: 'Left', row: 1, column: 2, from: 'Orange', to: 'Yellow' },
    { face: 'Left', row: 2, column: 2, from: 'Orange', to: 'Yellow' },
    { face: 'Right', row: 0, column: 0, from: 'Red', to: 'White' },
    { face: 'Right', row: 1, column: 0, from: 'Red', to: 'White' },
    { face: 'Right', row: 2, column: 0, from: 'Red', to: 'White' },
    { face: 'Down', row: 0, column: 0, from: 'Yellow', to: 'Red' },
    { face: 'Down', row: 0, column: 1, from: 'Yellow', to: 'Red' },
    { face: 'Down', row: 0, column: 2, from: 'Yellow', to: 'Red' },
  ],
};

export const LOG: LogEntry[] = [
  { sequence: 1, kind: 'Rotation', face: 'Front', rotation: 'Clockwise', move: 'F', occurredAtUtc: '2026-09-28T10:00:00Z' },
  { sequence: 2, kind: 'Undo', face: 'Front', rotation: 'Clockwise', move: 'F', occurredAtUtc: '2026-09-28T10:00:05Z' },
  { sequence: 3, kind: 'Reset', face: null, rotation: null, move: null, occurredAtUtc: '2026-09-28T10:00:09Z' },
];
