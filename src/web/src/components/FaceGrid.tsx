import type { Colour, Face, FaceStickers } from '../domain/types';
import { Sticker } from './Sticker';

interface FaceGridProps {
  face: Face;
  stickers: FaceStickers;
  /** Colours after the previewed move, if a preview is active. */
  previewStickers?: FaceStickers;
  selected: boolean;
  disabled: boolean;
  onSelect(face: Face): void;
}

export function FaceGrid({ face, stickers, previewStickers, selected, disabled, onSelect }: FaceGridProps) {
  return (
    <button
      type="button"
      className={`face face--${face.toLowerCase()}${selected ? ' face--selected' : ''}`}
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
