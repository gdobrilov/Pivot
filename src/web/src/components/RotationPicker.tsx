import type { Face, Rotation } from '../domain/types';
import { ROTATIONS, ROTATION_LABELS } from '../domain/types';

interface RotationPickerProps {
  face: Face | null;
  disabled: boolean;
  onHover(face: Face, rotation: Rotation): void;
  onLeave(): void;
  onRotate(face: Face, rotation: Rotation): void;
}

/** Second step of "turn a face": pick how far. Hovering a choice previews it on the net. */
export function RotationPicker({ face, disabled, onHover, onLeave, onRotate }: RotationPickerProps) {
  if (!face) {
    return (
      <section className="picker" aria-label="Rotation">
        <p className="muted">Click a face on the net to choose what to turn.</p>
      </section>
    );
  }

  return (
    <section className="picker" aria-label="Rotation">
      <h2>
        Turn the <strong>{face}</strong> face
      </h2>
      <div className="picker__options" onMouseLeave={onLeave}>
        {ROTATIONS.map((rotation) => (
          <button
            key={rotation}
            type="button"
            disabled={disabled}
            onMouseEnter={() => onHover(face, rotation)}
            onFocus={() => onHover(face, rotation)}
            onBlur={onLeave}
            onClick={() => onRotate(face, rotation)}
          >
            {ROTATION_LABELS[rotation]}
          </button>
        ))}
      </div>
      <p className="muted picker__hint">Hover a choice to see which stickers move and what colour they take.</p>
    </section>
  );
}
