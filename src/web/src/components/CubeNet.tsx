import type { Face, FacesSnapshot } from '../domain/types';
import { FACES, faceKey } from '../domain/types';
import type { AnimatedTurn, TurnCounts } from '../hooks/useCubeSession';
import { FaceGrid } from './FaceGrid';

interface CubeNetProps {
  faces: FacesSnapshot;
  previewFaces?: FacesSnapshot;
  lastTurn?: AnimatedTurn | null;
  turnCounts?: TurnCounts;
  selectedFace: Face | null;
  busy: boolean;
  onSelectFace(face: Face): void;
}

/** The net from the brief (U on top, L F R B, D below). Click a face to pick it. */
export function CubeNet({ faces, previewFaces, lastTurn, turnCounts = {}, selectedFace, busy, onSelectFace }: CubeNetProps) {
  return (
    <div className="net" role="group" aria-label="Cube net">
      {FACES.map((face) => (
        <FaceGrid
          // The key changes only when this face turns, so it remounts and plays its turn with the new colours.
          key={`${face}-${turnCounts[face] ?? 0}`}
          face={face}
          stickers={faces[faceKey(face)]}
          previewStickers={previewFaces?.[faceKey(face)]}
          turning={lastTurn?.face === face ? lastTurn.rotation : undefined}
          selected={selectedFace === face}
          busy={busy}
          onSelect={onSelectFace}
        />
      ))}
    </div>
  );
}
