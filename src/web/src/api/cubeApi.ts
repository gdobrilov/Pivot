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

/** REST client, no React inside so it can be tested and swapped. */
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
  async function request<T>(path: string, method = 'GET', body?: unknown): Promise<T> {
    const response = await fetchFn(
      `${baseUrl}${path}`,
      body === undefined
        ? { method }
        : { method, headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) },
    );

    if (!response.ok) {
      const problem = (await response.json().catch(() => ({}))) as ProblemDetails;
      throw new ApiError(problem.detail ?? problem.title ?? `Request failed (${response.status})`, response.status);
    }

    return (await response.json()) as T;
  }

  return {
    create: () => request('', 'POST'),
    get: (id) => request(`/${id}`),
    preview: (id, face, rotation) => request(`/${id}/preview?face=${face}&rotation=${rotation}`),
    rotate: (id, face, rotation) => request(`/${id}/rotations`, 'POST', { face, rotation }),
    undo: (id) => request(`/${id}/undo`, 'POST'),
    reset: (id) => request(`/${id}/reset`, 'POST'),
    log: (id) => request(`/${id}/rotations`),
  };
}
