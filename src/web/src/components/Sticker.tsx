import type { Colour } from '../domain/types';
import { COLOUR_HEX, COLOUR_SYMBOL } from '../domain/colours';

interface StickerProps {
  colour: Colour;
  /** When previewing a move, the colour this sticker would get. */
  becomes?: Colour;
}

export function Sticker({ colour, becomes }: StickerProps) {
  const changing = becomes !== undefined && becomes !== colour;
  const shown = changing ? becomes : colour;
  return (
    <div
      className={changing ? 'sticker sticker--changing' : 'sticker'}
      style={{ backgroundColor: COLOUR_HEX[shown] }}
      role="img"
      aria-label={changing ? `${colour} becomes ${becomes}` : colour}
      title={changing ? `${colour} → ${becomes}` : colour}
    >
      <span className="sticker__symbol">{COLOUR_SYMBOL[shown]}</span>
    </div>
  );
}
