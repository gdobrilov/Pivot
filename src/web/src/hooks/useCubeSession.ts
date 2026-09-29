import { useCallback, useEffect, useRef, useState } from 'react';
import { ApiError, type CubeApi } from '../api/cubeApi';
import { inverse, parseMove } from '../domain/notation';
import type { CubeState, Face, LogEntry, Rotation, RotationPreview, Turn } from '../domain/types';

const STORAGE_KEY = 'pivot.sessionId';

/** A turn that just happened, for the net to animate. */
export type AnimatedTurn = Turn;

/** How many times each face has turned; a face's animation key. */
export type TurnCounts = Readonly<Partial<Record<Face, number>>>;

export interface CubeSession {
  readonly cube: CubeState | null;
  readonly log: readonly LogEntry[];
  readonly preview: RotationPreview | null;
  readonly lastTurn: AnimatedTurn | null;
  readonly turnCounts: TurnCounts;
  readonly error: string | null;
  readonly busy: boolean;
  rotate(face: Face, rotation: Rotation): Promise<void>;
  resetAndRotate(turns: readonly Turn[]): Promise<void>;
  undo(): Promise<void>;
  reset(): Promise<void>;
  showPreview(face: Face, rotation: Rotation): Promise<void>;
  clearPreview(): void;
}

function messageOf(error: unknown): string {
  return error instanceof Error ? error.message : 'Something went wrong';
}

function readStoredId(): string | null {
  try {
    return localStorage.getItem(STORAGE_KEY);
  } catch {
    return null;
  }
}

function storeId(id: string): void {
  try {
    localStorage.setItem(STORAGE_KEY, id);
  } catch {
    // Private mode or blocked storage: the cube just won't survive a reload.
  }
}

/** Picks up the cube from the last visit if the API still has it, otherwise starts a new one. */
async function resumeOrCreate(api: CubeApi): Promise<CubeState> {
  const storedId = readStoredId();
  if (storedId) {
    try {
      return await api.get(storedId);
    } catch (error) {
      if (!(error instanceof ApiError && error.status === 404)) throw error;
    }
  }
  return api.create();
}

/** One server-side session: state, log, preview and the actions on it. */
export function useCubeSession(api: CubeApi): CubeSession {
  const [cube, setCube] = useState<CubeState | null>(null);
  const [log, setLog] = useState<readonly LogEntry[]>([]);
  const [preview, setPreview] = useState<RotationPreview | null>(null);
  const [lastTurn, setLastTurn] = useState<AnimatedTurn | null>(null);
  const [turnCounts, setTurnCounts] = useState<TurnCounts>({});
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(true);
  const mounted = useRef(true);
  const previewRequest = useRef(0);

  const show = useCallback((next: CubeState, nextLog: readonly LogEntry[], turn?: Turn) => {
    setCube(next);
    setLog(nextLog);
    setPreview(null);
    setLastTurn(turn ?? null);
    if (turn) setTurnCounts((counts) => ({ ...counts, [turn.face]: (counts[turn.face] ?? 0) + 1 }));
    storeId(next.id);
  }, []);

  useEffect(() => {
    // A flag per effect run, so the extra run React does in development cannot apply a stale result.
    let cancelled = false;
    mounted.current = true;
    void (async () => {
      try {
        const next = await resumeOrCreate(api);
        const nextLog = await api.log(next.id);
        if (!cancelled) show(next, nextLog);
      } catch (e) {
        if (!cancelled) setError(messageOf(e));
      } finally {
        if (!cancelled) setBusy(false);
      }
    })();
    return () => {
      cancelled = true;
      mounted.current = false;
    };
  }, [api, show]);

  const run = useCallback(
    async (action: (id: string) => Promise<CubeState>, turn?: Turn) => {
      if (!cube) return;
      previewRequest.current++;
      setBusy(true);
      setError(null);
      try {
        const next = await action(cube.id);
        const nextLog = await api.log(next.id);
        if (mounted.current) show(next, nextLog, turn);
      } catch (e) {
        if (!mounted.current) return;
        setError(messageOf(e));
        // Part of a sequence may have been applied, so show what the server has now.
        try {
          const current = await api.get(cube.id);
          const currentLog = await api.log(cube.id);
          if (mounted.current) show(current, currentLog);
        } catch {
          if (mounted.current) setPreview(null);
        }
      } finally {
        if (mounted.current) setBusy(false);
      }
    },
    [api, cube, show],
  );

  const rotate = useCallback(
    (face: Face, rotation: Rotation) => run((id) => api.rotate(id, face, rotation), { face, rotation }),
    [api, run],
  );

  const resetAndRotate = useCallback(
    (turns: readonly Turn[]) =>
      run(async (id) => {
        let state = await api.reset(id);
        for (const turn of turns) {
          state = await api.rotate(id, turn.face, turn.rotation);
        }
        return state;
      }),
    [api, run],
  );

  const undo = useCallback(() => {
    const last = parseMove(cube?.effectiveMoves.at(-1) ?? '');
    return run((id) => api.undo(id), last ? inverse(last) : undefined);
  }, [api, cube, run]);

  const reset = useCallback(() => run((id) => api.reset(id)), [api, run]);

  const showPreview = useCallback(
    async (face: Face, rotation: Rotation) => {
      if (!cube || busy) return;
      const request = ++previewRequest.current;
      try {
        const next = await api.preview(cube.id, face, rotation);
        // Ignore a reply that arrives after a newer preview, a clear, or a turn.
        if (mounted.current && request === previewRequest.current) setPreview(next);
      } catch {
        if (mounted.current && request === previewRequest.current) setPreview(null);
      }
    },
    [api, busy, cube],
  );

  const clearPreview = useCallback(() => {
    previewRequest.current++;
    setPreview(null);
  }, []);

  return { cube, log, preview, lastTurn, turnCounts, error, busy, rotate, resetAndRotate, undo, reset, showPreview, clearPreview };
}
