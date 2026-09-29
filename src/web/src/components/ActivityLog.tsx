import type { LogEntry } from '../domain/types';
import { ROTATION_LABELS } from '../domain/types';

interface ActivityLogProps {
  entries: readonly LogEntry[];
}

function describe(entry: LogEntry): string {
  switch (entry.kind) {
    case 'Rotation':
      return `${entry.face} ${entry.rotation ? ROTATION_LABELS[entry.rotation] : ''} (${entry.move})`;
    case 'Undo':
      return `Undo ${entry.move}`;
    case 'Reset':
      return 'Reset to solved';
  }
}

function time(iso: string): string {
  return new Date(iso).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' });
}

/** Everything done to this cube, newest first. */
export function ActivityLog({ entries }: ActivityLogProps) {
  return (
    <section className="log" aria-label="Activity log">
      <h2>On the record</h2>
      {entries.length === 0 ? (
        <p className="muted">No turns yet. Suspiciously tidy.</p>
      ) : (
        <ol>
          {[...entries].reverse().map((entry) => (
            <li key={entry.sequence} className={`log__entry log__entry--${entry.kind.toLowerCase()}`}>
              <span className="log__what">{describe(entry)}</span>
              <time className="log__when" dateTime={entry.occurredAtUtc}>
                {time(entry.occurredAtUtc)}
              </time>
            </li>
          ))}
        </ol>
      )}
    </section>
  );
}
