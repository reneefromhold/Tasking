export interface UserStatusResponse {
  exists: boolean;
  status: string;
  id?: string | null;
}

export interface UserResponse {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
}

export interface UserSummary {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
}

export function formatUserName(user: Pick<UserSummary, 'firstName' | 'lastName'>) {
  return `${user.firstName} ${user.lastName}`.trim();
}

export function getUserNameById(
  users: UserSummary[],
  userId: string | null | undefined,
) {
  if (!userId) {
    return 'Unassigned';
  }

  const user = users.find((entry) => entry.id === userId);
  return user ? formatUserName(user) : userId;
}

export interface Category {
  id: string;
  category: string;
}

export interface Task {
  id: string;
  categoryId: string | null;
  categoryName: string | null;
  title: string;
  description: string | null;
  creator: string | null;
  assignee: string | null;
  createDate: string;
  dueDate: string | null;
}

export interface TaskFormData {
  categoryId: string | null;
  title: string;
  description: string;
  assignee: string | null;
  dueDate: string;
}

export interface AuthUser {
  id: string;
  email: string;
}

export interface ApiError {
  message: string;
  detail?: string | null;
}

export function toDateInputValue(value: string | null | undefined) {
  if (!value) {
    return '';
  }

  return value.slice(0, 10);
}

const DATE_INPUT_PATTERN = /^\d{4}-\d{2}-\d{2}$/;
const MAX_DUE_DATE_YEARS_AHEAD = 100;

export function getTodayDateInputValue() {
  const today = new Date();
  const year = today.getFullYear();
  const month = String(today.getMonth() + 1).padStart(2, '0');
  const day = String(today.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

export function getMaxDueDateInputValue() {
  const maxYear = new Date().getFullYear() + MAX_DUE_DATE_YEARS_AHEAD;
  return `${maxYear}-12-31`;
}

export function getDueDateInputError(value: string) {
  if (!value) {
    return null;
  }

  if (!DATE_INPUT_PATTERN.test(value)) {
    return 'Enter a valid due date.';
  }

  const year = Number(value.slice(0, 4));
  const month = Number(value.slice(5, 7));
  const day = Number(value.slice(8, 10));
  const currentYear = new Date().getFullYear();
  const maxYear = currentYear + MAX_DUE_DATE_YEARS_AHEAD;

  if (year < currentYear || year > maxYear) {
    return `Due date year must be between ${currentYear} and ${maxYear}.`;
  }

  const parsed = new Date(year, month - 1, day);
  if (
    parsed.getFullYear() !== year ||
    parsed.getMonth() !== month - 1 ||
    parsed.getDate() !== day
  ) {
    return 'Enter a valid due date.';
  }

  if (value < getTodayDateInputValue()) {
    return 'Due date cannot be in the past.';
  }

  return null;
}

export function isValidDueDateInput(value: string) {
  return getDueDateInputError(value) === null;
}

export function isPastDueDate(value: string) {
  return getDueDateInputError(value) !== null;
}

export function toApiDueDate(value: string) {
  if (!value) {
    return null;
  }

  return `${value}T00:00:00Z`;
}

export function formatDisplayDate(value: string | null | undefined) {
  if (!value) {
    return '—';
  }

  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) {
    return value;
  }

  return parsed.toLocaleDateString();
}
