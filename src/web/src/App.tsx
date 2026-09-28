import { useMemo, useState } from 'react';
import { createCubeApi } from './api/cubeApi';
import { ActivityLog } from './components/ActivityLog';
import { CubeNet } from './components/CubeNet';
import { RotationPicker } from './components/RotationPicker';
import { CHALLENGE_SEQUENCE, type Face } from './domain/types';
import { useCubeSession } from './hooks/useCubeSession';
import './App.css';

export default function App() {
  const api = useMemo(() => createCubeApi(), []);
  const session = useCubeSession(api);
  const [selectedFace, setSelectedFace] = useState<Face | null>(null);
  const { cube, log, preview, error, busy } = session;

  return (
    <main className="app">
      <header className="app__header">
        <h1>Rubik's Cube Simulator</h1>
        <p className="muted">
          Green front, red right, white up. Pick a face, pick how far to turn it, and watch where every sticker goes.
        </p>
      </header>

      {error && (
        <p role="alert" className="error">
          {error}
        </p>
      )}

      {cube ? (
        <div className="app__body">
          <div>
            <CubeNet
              faces={cube.faces}
              previewFaces={preview?.after}
              selectedFace={selectedFace}
              disabled={busy}
              onSelectFace={(face) => setSelectedFace((current) => (current === face ? null : face))}
            />
            <p className="status" aria-live="polite">
              {cube.isSolved ? 'Solved' : 'Scrambled'}
              {cube.effectiveMoves.length > 0 && <> · {cube.effectiveMoves.join(' ')}</>}
              {preview && (
                <>
                  {' '}
                  · previewing <strong>{preview.move}</strong>: {preview.changes.length} stickers change
                </>
              )}
            </p>
          </div>

          <div className="app__side">
            <RotationPicker
              face={selectedFace}
              disabled={busy}
              onHover={(face, rotation) => void session.showPreview(face, rotation)}
              onLeave={session.clearPreview}
              onRotate={(face, rotation) => void session.rotate(face, rotation)}
            />

            <section className="toolbar" aria-label="Actions">
              <button type="button" disabled={busy || !cube.canUndo} onClick={() => void session.undo()}>
                Undo
              </button>
              <button type="button" className="secondary" disabled={busy} onClick={() => void session.reset()}>
                Reset
              </button>
              <button
                type="button"
                className="secondary"
                disabled={busy}
                onClick={() => void session.rotateSequence(CHALLENGE_SEQUENCE)}
                title="F R' U B' L D'"
              >
                Run challenge sequence
              </button>
            </section>

            <ActivityLog entries={log} />
          </div>
        </div>
      ) : (
        !error && <p className="muted">Creating cube…</p>
      )}
    </main>
  );
}
