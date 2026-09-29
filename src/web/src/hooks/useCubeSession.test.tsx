import { StrictMode, type ReactNode } from 'react';
import { act, renderHook, waitFor } from '@testing-library/react';
import { ApiError, type CubeApi } from '../api/cubeApi';
import { useCubeSession } from './useCubeSession';
import { CHALLENGE_SEQUENCE, type RotationPreview } from '../domain/types';
import { CHALLENGE_RESULT, FRONT_PREVIEW, LOG, SOLVED } from '../test/fixtures';

function fakeApi(overrides: Partial<CubeApi> = {}): CubeApi {
  return {
    create: vi.fn().mockResolvedValue(SOLVED),
    get: vi.fn().mockResolvedValue(SOLVED),
    preview: vi.fn().mockResolvedValue(FRONT_PREVIEW),
    rotate: vi.fn().mockResolvedValue(CHALLENGE_RESULT),
    undo: vi.fn().mockResolvedValue(SOLVED),
    reset: vi.fn().mockResolvedValue(SOLVED),
    log: vi.fn().mockResolvedValue(LOG),
    ...overrides,
  };
}

async function ready(api: CubeApi) {
  const hook = renderHook(() => useCubeSession(api));
  await waitFor(() => expect(hook.result.current.busy).toBe(false));
  return hook;
}

describe('useCubeSession', () => {
  it('creates a session on first visit and remembers it', async () => {
    const api = fakeApi();

    const { result } = await ready(api);

    expect(result.current.cube).toEqual(SOLVED);
    expect(result.current.log).toEqual(LOG);
    expect(api.create).toHaveBeenCalledTimes(1);
    expect(localStorage.getItem('pivot.sessionId')).toBe(SOLVED.id);
  });

  it('resumes the remembered session instead of creating one', async () => {
    localStorage.setItem('pivot.sessionId', CHALLENGE_RESULT.id);
    const api = fakeApi({ get: vi.fn().mockResolvedValue(CHALLENGE_RESULT) });

    const { result } = await ready(api);

    expect(api.get).toHaveBeenCalledWith(CHALLENGE_RESULT.id);
    expect(api.create).not.toHaveBeenCalled();
    expect(result.current.cube).toEqual(CHALLENGE_RESULT);
  });

  it('starts a new session when the remembered one is gone', async () => {
    localStorage.setItem('pivot.sessionId', 'gone');
    const api = fakeApi({ get: vi.fn().mockRejectedValue(new ApiError('Not found', 404)) });

    await ready(api);

    expect(api.create).toHaveBeenCalledTimes(1);
  });

  it('shows one session under StrictMode', async () => {
    const api = fakeApi();
    const wrapper = ({ children }: { children: ReactNode }) => <StrictMode>{children}</StrictMode>;
    const { result } = renderHook(() => useCubeSession(api), { wrapper });

    await waitFor(() => expect(result.current.busy).toBe(false));

    expect(result.current.cube).toEqual(SOLVED);
    expect(api.log).toHaveBeenLastCalledWith(SOLVED.id);
  });

  it('rotates, refreshes the log and records the turn for the animation', async () => {
    const api = fakeApi();
    const { result } = await ready(api);

    await act(() => result.current.rotate('Front', 'Clockwise'));

    expect(api.rotate).toHaveBeenCalledWith(SOLVED.id, 'Front', 'Clockwise');
    expect(result.current.cube).toEqual(CHALLENGE_RESULT);
    expect(result.current.lastTurn).toEqual({ face: 'Front', rotation: 'Clockwise' });
    expect(result.current.turnCounts).toEqual({ Front: 1 });
  });

  it('resets before running a sequence', async () => {
    const api = fakeApi();
    const { result } = await ready(api);

    await act(() => result.current.resetAndRotate(CHALLENGE_SEQUENCE));

    expect(api.reset).toHaveBeenCalledTimes(1);
    expect(api.rotate).toHaveBeenCalledTimes(6);
    expect(api.rotate).toHaveBeenNthCalledWith(2, SOLVED.id, 'Right', 'AntiClockwise');
  });

  it('shows and clears a preview without changing the cube', async () => {
    const api = fakeApi();
    const { result } = await ready(api);

    await act(() => result.current.showPreview('Front', 'Clockwise'));
    expect(result.current.preview).toEqual(FRONT_PREVIEW);
    expect(result.current.cube).toEqual(SOLVED);

    act(() => result.current.clearPreview());
    expect(result.current.preview).toBeNull();
  });

  it('drops a preview that arrives after a turn', async () => {
    let answer: (value: RotationPreview) => void = () => {};
    const api = fakeApi({ preview: vi.fn(() => new Promise<RotationPreview>((resolve) => (answer = resolve))) });
    const { result } = await ready(api);

    let pending: Promise<void> = Promise.resolve();
    act(() => {
      pending = result.current.showPreview('Front', 'Clockwise');
    });
    await act(() => result.current.rotate('Front', 'Clockwise'));
    await act(async () => {
      answer(FRONT_PREVIEW);
      await pending;
    });

    expect(result.current.preview).toBeNull();
  });

  it('shows the error and re-reads the server state when an action fails', async () => {
    const api = fakeApi({
      rotate: vi.fn().mockRejectedValue(new Error('Something broke')),
      get: vi.fn().mockResolvedValue(CHALLENGE_RESULT),
    });
    const { result } = await ready(api);

    await act(() => result.current.rotate('Front', 'Clockwise'));

    expect(result.current.error).toBe('Something broke');
    expect(api.get).toHaveBeenCalledWith(SOLVED.id);
    expect(result.current.cube).toEqual(CHALLENGE_RESULT);
  });
});
