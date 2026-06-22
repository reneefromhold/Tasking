import type {
  ApiError,
  Category,
  Task,
  TaskFormData,
  UserResponse,
  UserStatusResponse,
  UserSummary,
} from '../types';
import { toApiDueDate } from '../types';

const API_BASE = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5044';

async function request<T>(
  path: string,
  options: RequestInit = {},
  userId?: string,
): Promise<T> {
  const headers = new Headers(options.headers);
  headers.set('Content-Type', 'application/json');

  if (userId !== undefined) {
    headers.set('X-User-Id', userId);
  }

  const response = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers,
  });

  if (!response.ok) {
    let message = `Request failed (${response.status})`;
    try {
      const error = (await response.json()) as ApiError;
      message = error.message ?? message;
    } catch {
      // ignore parse errors
    }
    throw new Error(message);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export function getUserStatus(email: string) {
  return request<UserStatusResponse>(
    `/api/users/${encodeURIComponent(email)}/status`,
  );
}

export function createUser(email: string, firstName: string, lastName: string) {
  return request<UserResponse>('/api/users', {
    method: 'POST',
    body: JSON.stringify({ email, firstName, lastName }),
  });
}

export function getUsers() {
  return request<UserSummary[]>('/api/users');
}

export function getCategories() {
  return request<Category[]>('/api/categories');
}

export function getTasks(userId: string) {
  return request<Task[]>('/api/tasks', {}, userId);
}

export function createTask(userId: string, task: TaskFormData) {
  return request<Task>(
    '/api/tasks',
    {
      method: 'POST',
      body: JSON.stringify({
        categoryId: task.categoryId,
        title: task.title,
        description: task.description || null,
        assignee: task.assignee,
        dueDate: toApiDueDate(task.dueDate),
      }),
    },
    userId,
  );
}

export function updateTask(userId: string, taskId: string, task: TaskFormData) {
  return request<Task>(
    `/api/tasks/${taskId}`,
    {
      method: 'PUT',
      body: JSON.stringify({
        categoryId: task.categoryId,
        title: task.title,
        description: task.description || null,
        assignee: task.assignee,
        dueDate: toApiDueDate(task.dueDate),
      }),
    },
    userId,
  );
}

export function deleteTask(userId: string, taskId: string) {
  return request<void>(`/api/tasks/${taskId}`, { method: 'DELETE' }, userId);
}
