import type { Colour, Face, FaceStickers, Rotation } from '../domain/types';
import { Sticker } from './Sticker';

interface FaceGridProps {
  face: Face;
  stickers: FaceStickers;
  /** Colours after the previewed move, if a preview is active. */
  previewStickers?: FaceStickers;
  /** Set when this face just turned: it animates into place. */
  turning?: Rotation;
  selected: boolean;
  busy: boolean;
  onSelect(face: Face): void;
}

const TURN_CLASS: Record<Rotation, string> = {
  Clockwise: 'face--turn-cw',
  AntiClockwise: 'face--turn-ccw',
  Half: 'face--turn-half',
};

/** "Row 1: red, orange, green. Row 2: ..." with "red becomes white" while previewing. */
function describe(stickers: FaceStickers, previewStickers?: FaceStickers): string {
  const rows: string[] = [];
  for (let row = 0; row * 3 < stickers.length; row++) {
    const cells = stickers.slice(row * 3, row * 3 + 3).map((colour: Colour, column) => {
      const next = previewStickers?.[row * 3 + column];
      return next && next !== colour ? `${colour} becomes ${next}` : colour;
    });
    rows.push(`Row ${row + 1}: ${cells.join(', ')}.`);
  }
  return rows.join(' ').toLowerCase();
}

export function FaceGrid({ face, stickers, previewStickers, turning, selected, busy, onSelect }: FaceGridProps) {
  const classes = ['face', `face--${face.toLowerCase()}`];
  if (selected) classes.push('face--selected');
  if (turning) classes.push(TURN_CLASS[turning]);
  const descriptionId = `face-${face}-stickers`;

  return (
    <>
      <button
        type="button"
        className={classes.join(' ')}
        data-testid={`face-${face}`}
        aria-label={`${face} face`}
        aria-describedby={descriptionId}
        aria-pressed={selected}
        aria-disabled={busy}
        onClick={() => {
          if (!busy) onSelect(face);
        }}
      >
        {stickers.map((colour, index) => (
          <Sticker key={index} colour={colour} becomes={previewStickers?.[index]} />
        ))}
      </button>
      <span id={descriptionId} className="visually-hidden">
        {describe(stickers, previewStickers)}
      </span>
    </>
  );
}
