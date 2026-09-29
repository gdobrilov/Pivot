import type { Face, FacesSnapshot } from '../domain/types';
import { FACES, faceKey } from '../domain/types';
import { FaceGrid } from './FaceGrid';

interface CubeNetProps {
  faces: FacesSnapshot;
  previewFaces?: FacesSnapshot;
  selectedFace: Face | null;
  disabled: boolean;
  onSelectFace(face: Face): void;
}

/** The net from the brief (U on top, L F R B, D below). Click a face to pick it. */
export function CubeNet({ faces, previewFaces, selectedFace, disabled, onSelectFace }: CubeNetProps) {
  return (
    <div className="net" aria-label="Cube net">
      {FACES.map((face) => (
        <FaceGrid
          key={face}
          face={face}
          stickers={faces[faceKey(face)]}
          previewStickers={previewFaces?.[faceKey(face)]}
          selected={selectedFace === face}
          disabled={disabled}
          onSelect={onSelectFace}
        />
      ))}
    </div>
  );
}
