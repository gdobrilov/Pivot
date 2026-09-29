import type { Colour } from '../domain/types';
import { COLOUR_HEX, COLOUR_SYMBOL } from '../domain/colours';

interface StickerProps {
  colour: Colour;
  /** When previewing a move, the colour this sticker would get. */
  becomes?: Colour;
}

/** Decorative: the face describes its stickers to screen readers. */
export function Sticker({ colour, becomes }: StickerProps) {
  const changing = becomes !== undefined && becomes !== colour;
  const shown = changing ? becomes : colour;
  return (
    <span
      className={changing ? 'sticker sticker--changing' : 'sticker'}
      style={{ backgroundColor: COLOUR_HEX[shown], color: shown === 'Blue' ? '#fff' : '#111' }}
      data-testid="sticker"
      data-colour={colour}
      data-becomes={changing ? becomes : undefined}
      aria-hidden="true"
      title={changing ? `${colour} → ${becomes}` : colour}
    >
      {COLOUR_SYMBOL[shown]}
    </span>
  );
}
