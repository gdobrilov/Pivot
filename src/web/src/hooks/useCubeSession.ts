import { useCallback, useEffect, useRef, useState } from 'react';
import type { CubeApi } from '../api/cubeApi';
import { inverse, parseMove } from '../domain/notation';
import type { CubeState, Face, LogEntry, Rotation, RotationPreview, Turn } from '../domain/types';

/** A turn that just happened, for the net to animate. `id` changes every time. */
export interface AnimatedTurn extends Turn {
  readonly id: number;
}

export interface CubeSession {
  readonly cube: CubeState | null;
  readonly log: readonly LogEntry[];
  readonly preview: RotationPreview | null;
  readonly lastTurn: AnimatedTurn | null;
  readonly error: string | null;
  readonly busy: boolean;
  rotate(face: Face, rotation: Rotation): Promise<void>;
  rotateSequence(turns: readonly Turn[]): Promise<void>;
  undo(): Promise<void>;
  reset(): Promise<void>;
  showPreview(face: Face, rotation: Rotation): Promise<void>;
  clearPreview(): void;
}

/** One server-side session: state, log, preview and the actions on it. */
export function useCubeSession(api: CubeApi): CubeSession {
  const [cube, setCube] = useState<CubeState | null>(null);
  const [log, setLog] = useState<readonly LogEntry[]>([]);
  const [preview, setPreview] = useState<RotationPreview | null>(null);
  const [lastTurn, setLastTurn] = useState<AnimatedTurn | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(true);
  const mounted = useRef(true);
  const previewRequest = useRef(0);
  const turnCounter = useRef(0);

  const run = useCallback(
    async (action: () => Promise<CubeState>, animate?: Turn) => {
      setBusy(true);
      setError(null);
      setPreview(null);
      try {
        const next = await action();
        if (!mounted.current) return;
        setCube(next);
        setLastTurn(animate ? { ...animate, id: ++turnCounter.current } : null);
        setLog(await api.log(next.id));
      } catch (e) {
        if (mounted.current) setError(e instanceof Error ? e.message : 'Something went wrong');
      } finally {
        if (mounted.current) setBusy(false);
      }
    },
    [api],
  );

  useEffect(() => {
    mounted.current = true;
    void run(() => api.create());
    return () => {
      mounted.current = false;
    };
  }, [api, run]);

  const rotate = useCallback(
    async (face: Face, rotation: Rotation) => {
      if (cube) await run(() => api.rotate(cube.id, face, rotation), { face, rotation });
    },
    [api, cube, run],
  );

  const rotateSequence = useCallback(
    async (turns: readonly Turn[]) => {
      if (!cube) return;
      await run(async () => {
        let state = cube;
        for (const turn of turns) {
          state = await api.rotate(state.id, turn.face, turn.rotation);
        }
        return state;
      });
    },
    [api, cube, run],
  );

  const undo = useCallback(async () => {
    if (!cube) return;
    const last = parseMove(cube.effectiveMoves[cube.effectiveMoves.length - 1] ?? '');
    await run(() => api.undo(cube.id), last ? inverse(last) : undefined);
  }, [api, cube, run]);

  const reset = useCallback(async () => {
    if (cube) await run(() => api.reset(cube.id));
  }, [api, cube, run]);

  const showPreview = useCallback(
    async (face: Face, rotation: Rotation) => {
      if (!cube) return;
      const request = ++previewRequest.current;
      try {
        const next = await api.preview(cube.id, face, rotation);
        // Ignore a reply that arrives after a newer preview was requested or cleared.
        if (mounted.current && request === previewRequest.current) setPreview(next);
      } catch {
        if (mounted.current && request === previewRequest.current) setPreview(null);
      }
    },
    [api, cube],
  );

  const clearPreview = useCallback(() => {
    previewRequest.current++;
    setPreview(null);
  }, []);

  return { cube, log, preview, lastTurn, error, busy, rotate, rotateSequence, undo, reset, showPreview, clearPreview };
}
