import type { CubeState, Face, LogEntry, Rotation, RotationPreview } from '../domain/types';

export class ApiError extends Error {
  constructor(
    message: string,
    public readonly status: number,
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

interface ProblemDetails {
  title?: string;
  detail?: string;
}

/** Thin client over the REST API. Kept free of React so it can be unit tested and swapped. */
export interface CubeApi {
  create(): Promise<CubeState>;
  get(id: string): Promise<CubeState>;
  preview(id: string, face: Face, rotation: Rotation): Promise<RotationPreview>;
  rotate(id: string, face: Face, rotation: Rotation): Promise<CubeState>;
  undo(id: string): Promise<CubeState>;
  reset(id: string): Promise<CubeState>;
  log(id: string): Promise<LogEntry[]>;
}

export function createCubeApi(baseUrl = '/api/cubes', fetchFn: typeof fetch = fetch): CubeApi {
  async function request<T>(path: string, init?: RequestInit): Promise<T> {
    const response = await fetchFn(`${baseUrl}${path}`, {
      headers: { 'Content-Type': 'application/json' },
      ...init,
    });

    if (!response.ok) {
      const problem = (await response.json().catch(() => ({}))) as ProblemDetails;
      throw new ApiError(problem.detail ?? problem.title ?? `Request failed (${response.status})`, response.status);
    }

    return (await response.json()) as T;
  }

  return {
    create: () => request('', { method: 'POST' }),
    get: (id) => request(`/${id}`),
    preview: (id, face, rotation) => request(`/${id}/preview?face=${face}&rotation=${rotation}`),
    rotate: (id, face, rotation) =>
      request(`/${id}/rotations`, { method: 'POST', body: JSON.stringify({ face, rotation }) }),
    undo: (id) => request(`/${id}/rotations/undo`, { method: 'POST' }),
    reset: (id) => request(`/${id}/reset`, { method: 'POST' }),
    log: (id) => request(`/${id}/rotations`),
  };
}
