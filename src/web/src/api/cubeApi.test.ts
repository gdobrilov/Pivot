import { ApiError, createCubeApi } from './cubeApi';
import { FRONT_PREVIEW, SOLVED } from '../test/fixtures';

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } });
}

describe('cubeApi', () => {
  it('posts a structured rotation and returns the new state', async () => {
    const fetchFn = vi.fn().mockResolvedValue(jsonResponse(SOLVED));
    const api = createCubeApi('/api/cubes', fetchFn);

    const state = await api.rotate('abc', 'Right', 'AntiClockwise');

    expect(fetchFn).toHaveBeenCalledWith(
      '/api/cubes/abc/rotations',
      expect.objectContaining({ method: 'POST', body: JSON.stringify({ face: 'Right', rotation: 'AntiClockwise' }) }),
    );
    expect(state).toEqual(SOLVED);
  });

  it('requests a preview with query parameters', async () => {
    const fetchFn = vi.fn().mockResolvedValue(jsonResponse(FRONT_PREVIEW));
    const api = createCubeApi('/api/cubes', fetchFn);

    const preview = await api.preview('abc', 'Front', 'Clockwise');

    expect(fetchFn).toHaveBeenCalledWith('/api/cubes/abc/preview?face=Front&rotation=Clockwise', expect.anything());
    expect(preview.changes).toHaveLength(12);
  });

  it('surfaces problem details as an ApiError', async () => {
    const fetchFn = vi.fn().mockResolvedValue(jsonResponse({ title: 'Invalid request', detail: 'There is nothing to undo.' }, 400));
    const api = createCubeApi('/api/cubes', fetchFn);

    await expect(api.undo('abc')).rejects.toThrow(new ApiError('There is nothing to undo.', 400));
  });
});
