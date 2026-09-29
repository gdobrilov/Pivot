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
  disabled: boolean;
  onSelect(face: Face): void;
}

const TURN_CLASS: Record<Rotation, string> = {
  Clockwise: 'face--turn-cw',
  AntiClockwise: 'face--turn-ccw',
  Half: 'face--turn-half',
};

export function FaceGrid({ face, stickers, previewStickers, turning, selected, disabled, onSelect }: FaceGridProps) {
  const classes = ['face', `face--${face.toLowerCase()}`];
  if (selected) classes.push('face--selected');
  if (turning) classes.push(TURN_CLASS[turning]);

  return (
    <button
      type="button"
      className={classes.join(' ')}
      data-testid={`face-${face}`}
      aria-label={`${face} face`}
      aria-pressed={selected}
      disabled={disabled}
      onClick={() => onSelect(face)}
    >
      {stickers.map((colour: Colour, index) => (
        <Sticker key={index} colour={colour} becomes={previewStickers?.[index]} />
      ))}
    </button>
  );
}
