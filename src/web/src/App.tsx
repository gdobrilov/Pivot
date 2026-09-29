import { useMemo, useState } from 'react';
import { createCubeApi } from './api/cubeApi';
import { ActivityLog } from './components/ActivityLog';
import { CubeNet } from './components/CubeNet';
import { Logo } from './components/Logo';
import { RotationPicker } from './components/RotationPicker';
import { CHALLENGE_SEQUENCE, type Face } from './domain/types';
import { useCubeSession } from './hooks/useCubeSession';
import './App.css';

function statusLine(turns: number, solved: boolean, previewMove?: string, previewChanges?: number): string {
  if (previewMove !== undefined) return `Previewing ${previewMove}: ${previewChanges} stickers would change.`;
  if (solved && turns === 0) return 'Solved. Nothing to see here.';
  if (solved) return `Solved again, after ${turns} ${turns === 1 ? 'turn' : 'turns'} on the record.`;
  return `Scrambled after ${turns} ${turns === 1 ? 'turn' : 'turns'}.`;
}

export default function App() {
  const api = useMemo(() => createCubeApi(), []);
  const session = useCubeSession(api);
  const [selectedFace, setSelectedFace] = useState<Face | null>(null);
  const { cube, log, preview, lastTurn, error, busy } = session;

  return (
    <main className="app">
      <header className="app__header">
        <Logo />
        <div>
          <h1>Pivot</h1>
          <p className="tagline">We turn things around. Every quarter turn, on the record.</p>
        </div>
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
              lastTurn={lastTurn}
              selectedFace={selectedFace}
              disabled={busy}
              onSelectFace={(face) => setSelectedFace((current) => (current === face ? null : face))}
            />
            <p className="status" aria-live="polite">
              {cube.isSolved && log.length > 0 && !preview && (
                <span key={log.length} className="badge">
                  Solved
                </span>
              )}
              {statusLine(log.length, cube.isSolved, preview?.move, preview?.changes.length)}
              {cube.effectiveMoves.length > 0 && <span className="moves"> {cube.effectiveMoves.join(' ')}</span>}
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
                Run the brief's sequence
              </button>
            </section>

            <ActivityLog entries={log} />
          </div>
        </div>
      ) : (
        !error && <p className="muted">Setting up your cube…</p>
      )}
    </main>
  );
}
