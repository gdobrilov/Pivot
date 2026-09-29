import { act, renderHook, waitFor } from '@testing-library/react';
import type { CubeApi } from '../api/cubeApi';
import { useCubeSession } from './useCubeSession';
import { CHALLENGE_SEQUENCE } from '../domain/types';
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

describe('useCubeSession', () => {
  it('creates a session on mount and loads its log', async () => {
    const api = fakeApi();
    const { result } = renderHook(() => useCubeSession(api));

    await waitFor(() => expect(result.current.cube).toEqual(SOLVED));
    expect(api.create).toHaveBeenCalledTimes(1);
    expect(result.current.log).toEqual(LOG);
    expect(result.current.busy).toBe(false);
  });

  it('rotates against the created session and refreshes the log', async () => {
    const api = fakeApi();
    const { result } = renderHook(() => useCubeSession(api));
    await waitFor(() => expect(result.current.cube).not.toBeNull());

    await act(() => result.current.rotate('Front', 'Clockwise'));

    expect(api.rotate).toHaveBeenCalledWith(SOLVED.id, 'Front', 'Clockwise');
    expect(result.current.cube).toEqual(CHALLENGE_RESULT);
    expect(api.log).toHaveBeenCalledTimes(2);
  });

  it('runs a sequence as consecutive rotations', async () => {
    const api = fakeApi();
    const { result } = renderHook(() => useCubeSession(api));
    await waitFor(() => expect(result.current.cube).not.toBeNull());

    await act(() => result.current.rotateSequence(CHALLENGE_SEQUENCE));

    expect(api.rotate).toHaveBeenCalledTimes(6);
    expect(api.rotate).toHaveBeenNthCalledWith(2, SOLVED.id, 'Right', 'AntiClockwise');
  });

  it('shows and clears a preview without changing the cube', async () => {
    const api = fakeApi();
    const { result } = renderHook(() => useCubeSession(api));
    await waitFor(() => expect(result.current.cube).not.toBeNull());

    await act(() => result.current.showPreview('Front', 'Clockwise'));
    expect(result.current.preview).toEqual(FRONT_PREVIEW);
    expect(result.current.cube).toEqual(SOLVED);

    act(() => result.current.clearPreview());
    expect(result.current.preview).toBeNull();
  });

  it('exposes API failures as an error message and keeps the last good state', async () => {
    const api = fakeApi({ undo: vi.fn().mockRejectedValue(new Error('There is nothing to undo.')) });
    const { result } = renderHook(() => useCubeSession(api));
    await waitFor(() => expect(result.current.cube).not.toBeNull());

    await act(() => result.current.undo());

    expect(result.current.error).toBe('There is nothing to undo.');
    expect(result.current.cube).toEqual(SOLVED);
  });
});
