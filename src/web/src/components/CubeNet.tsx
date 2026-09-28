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

/**
 * Exploded view of the cube, laid out as in the challenge brief:
 *        U
 *    L   F   R   B
 *        D
 * Each face is a button: click it to choose which face to turn.
 */
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
