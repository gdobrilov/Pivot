import { ApiError, createCubeApi } from './cubeApi';
import { FRONT_PREVIEW, SOLVED } from '../test/fixtures';

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } });
}

describe('cubeApi', () => {
  it('posts a structured rotation as JSON and returns the new state', async () => {
    const fetchFn = vi.fn().mockResolvedValue(jsonResponse(SOLVED));
    const api = createCubeApi('/api/cubes', fetchFn);

    const state = await api.rotate('abc', 'Right', 'AntiClockwise');

    expect(fetchFn).toHaveBeenCalledWith('/api/cubes/abc/rotations', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ face: 'Right', rotation: 'AntiClockwise' }),
    });
    expect(state).toEqual(SOLVED);
  });

  it('sends bodyless requests without a content type', async () => {
    const fetchFn = vi.fn(() => Promise.resolve(jsonResponse(FRONT_PREVIEW)));
    const api = createCubeApi('/api/cubes', fetchFn);

    await api.preview('abc', 'Front', 'Clockwise');
    await api.undo('abc');

    expect(fetchFn).toHaveBeenNthCalledWith(1, '/api/cubes/abc/preview?face=Front&rotation=Clockwise', { method: 'GET' });
    expect(fetchFn).toHaveBeenNthCalledWith(2, '/api/cubes/abc/undo', { method: 'POST' });
  });

  it('surfaces problem details as an ApiError', async () => {
    const fetchFn = vi.fn().mockResolvedValue(jsonResponse({ title: 'Conflict', detail: 'There is nothing to undo.' }, 409));
    const api = createCubeApi('/api/cubes', fetchFn);

    await expect(api.undo('abc')).rejects.toThrow(new ApiError('There is nothing to undo.', 409));
  });
});
