import type { Face, FacesSnapshot } from '../domain/types';
import { FACES, faceKey } from '../domain/types';
import type { AnimatedTurn } from '../hooks/useCubeSession';
import { FaceGrid } from './FaceGrid';

interface CubeNetProps {
  faces: FacesSnapshot;
  previewFaces?: FacesSnapshot;
  lastTurn?: AnimatedTurn | null;
  selectedFace: Face | null;
  disabled: boolean;
  onSelectFace(face: Face): void;
}

/** The net from the brief (U on top, L F R B, D below). Click a face to pick it. */
export function CubeNet({ faces, previewFaces, lastTurn, selectedFace, disabled, onSelectFace }: CubeNetProps) {
  return (
    <div className="net" aria-label="Cube net">
      {FACES.map((face) => {
        const turning = lastTurn?.face === face ? lastTurn : null;
        return (
          <FaceGrid
            // A new key remounts the turned face, so it starts the turn with its new colours.
            key={turning ? `${face}-${turning.id}` : face}
            face={face}
            stickers={faces[faceKey(face)]}
            previewStickers={previewFaces?.[faceKey(face)]}
            turning={turning?.rotation}
            selected={selectedFace === face}
            disabled={disabled}
            onSelect={onSelectFace}
          />
        );
      })}
    </div>
  );
}
