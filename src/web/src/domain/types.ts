export const COLOURS = ['White', 'Orange', 'Green', 'Red', 'Blue', 'Yellow'] as const;
export type Colour = (typeof COLOURS)[number];

/** Face names as the API spells them (enum names). */
export const FACES = ['Up', 'Left', 'Front', 'Right', 'Back', 'Down'] as const;
export type Face = (typeof FACES)[number];

export const ROTATIONS = ['Clockwise', 'AntiClockwise', 'Half'] as const;
export type Rotation = (typeof ROTATIONS)[number];

export const ROTATION_LABELS: Readonly<Record<Rotation, string>> = {
  Clockwise: '90° clockwise',
  AntiClockwise: '90° anti-clockwise',
  Half: '180°',
};

/** Nine stickers in row-major order, as returned by the API. */
export type FaceStickers = readonly Colour[];

/** The API serialises the six faces with camelCase keys. */
export type FacesSnapshot = Readonly<Record<Lowercase<Face>, FaceStickers>>;

export interface CubeState {
  readonly id: string;
  readonly isSolved: boolean;
  readonly version: number;
  readonly canUndo: boolean;
  readonly effectiveMoves: readonly string[];
  readonly faces: FacesSnapshot;
}

export interface StickerChange {
  readonly face: Face;
  readonly row: number;
  readonly column: number;
  readonly from: Colour;
  readonly to: Colour;
}

export interface RotationPreview {
  readonly move: string;
  readonly after: FacesSnapshot;
  readonly changes: readonly StickerChange[];
}

export interface LogEntry {
  readonly sequence: number;
  readonly kind: 'Rotation' | 'Undo' | 'Reset';
  readonly face: Face | null;
  readonly rotation: Rotation | null;
  readonly move: string | null;
  readonly occurredAtUtc: string;
}

export interface Turn {
  readonly face: Face;
  readonly rotation: Rotation;
}

/** The sequence from the TTC challenge brief: F R' U B' L D'. */
export const CHALLENGE_SEQUENCE: readonly Turn[] = [
  { face: 'Front', rotation: 'Clockwise' },
  { face: 'Right', rotation: 'AntiClockwise' },
  { face: 'Up', rotation: 'Clockwise' },
  { face: 'Back', rotation: 'AntiClockwise' },
  { face: 'Left', rotation: 'Clockwise' },
  { face: 'Down', rotation: 'AntiClockwise' },
];

export function faceKey(face: Face): Lowercase<Face> {
  return face.toLowerCase() as Lowercase<Face>;
}
