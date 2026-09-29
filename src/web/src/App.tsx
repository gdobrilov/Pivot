import { useMemo, useState } from 'react';
import { createCubeApi } from './api/cubeApi';
import { ActivityLog } from './components/ActivityLog';
import { CubeNet } from './components/CubeNet';
import { Logo } from './components/Logo';
import { RotationPicker } from './components/RotationPicker';
import { CHALLENGE_SEQUENCE, type Face, type LogEntry } from './domain/types';
import { useCubeSession } from './hooks/useCubeSession';
import './App.css';

function plural(count: number, word: string): string {
  return `${count} ${word}${count === 1 ? '' : 's'}`;
}

function statusLine(log: readonly LogEntry[], solved: boolean): string {
  const turns = log.filter((entry) => entry.kind === 'Rotation').length;
  if (solved && turns === 0) return 'Solved. Nothing to see here.';
  if (solved) return `Solved again, after ${plural(turns, 'turn')} on the record.`;
  return `Scrambled after ${plural(turns, 'turn')}.`;
}

export default function App() {
  const api = useMemo(() => createCubeApi(), []);
  const session = useCubeSession(api);
  const [selectedFace, setSelectedFace] = useState<Face | null>(null);
  const { cube, log, preview, lastTurn, turnCounts, error, busy } = session;

  return (
    <main className="app">
      <header className="app__header">
        <Logo />
        <div>
          <h1>Pivot</h1>
          <p className="tagline">We turn things around. Every turn, on the record.</p>
        </div>
      </header>

      {error && (
        <p role="alert" className="error">
          {error}
        </p>
      )}

      {cube ? (
        <div className="app__body">
          <div className="app__main">
            <CubeNet
              faces={cube.faces}
              previewFaces={preview?.after}
              lastTurn={lastTurn}
              turnCounts={turnCounts}
              selectedFace={selectedFace}
              busy={busy}
              onSelectFace={(face) => setSelectedFace((current) => (current === face ? null : face))}
            />
            <p className="status">
              {cube.isSolved && log.some((entry) => entry.kind === 'Rotation') && <span className="badge">Solved</span>}
              <span aria-live="polite">
                {preview
                  ? `Previewing ${preview.move}: ${plural(preview.changes.length, 'sticker')} would change.`
                  : statusLine(log, cube.isSolved)}
              </span>
              {cube.effectiveMoves.length > 0 && <span className="moves">{cube.effectiveMoves.join(' ')}</span>}
            </p>
          </div>

          <div className="app__side">
            <RotationPicker
              face={selectedFace}
              busy={busy}
              onHover={(face, rotation) => void session.showPreview(face, rotation)}
              onLeave={session.clearPreview}
              onRotate={(face, rotation) => void session.rotate(face, rotation)}
            />

            <section className="toolbar" aria-label="Actions">
              <button
                type="button"
                aria-disabled={busy || !cube.canUndo}
                onClick={() => {
                  if (!busy && cube.canUndo) void session.undo();
                }}
              >
                Undo
              </button>
              <button
                type="button"
                className="secondary"
                aria-disabled={busy}
                onClick={() => {
                  if (!busy) void session.reset();
                }}
              >
                Reset
              </button>
              <button
                type="button"
                className="secondary"
                aria-disabled={busy}
                title="Resets the cube, then applies F R' U B' L D'"
                onClick={() => {
                  if (!busy) void session.resetAndRotate(CHALLENGE_SEQUENCE);
                }}
              >
                Run the brief's sequence
              </button>
            </section>

            <ActivityLog entries={log} />
          </div>
        </div>
      ) : (
        !error && <p className="muted">Setting up your cube...</p>
      )}
    </main>
  );
}
