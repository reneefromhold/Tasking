import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import {
  getDueDateInputError,
  getMaxDueDateInputValue,
  getTodayDateInputValue,
  isPastDueDate,
  isValidDueDateInput,
} from './types';

describe('due date validation', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2026-06-22T12:00:00'));
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('accepts an empty due date', () => {
    expect(getDueDateInputError('')).toBeNull();
    expect(isValidDueDateInput('')).toBe(true);
    expect(isPastDueDate('')).toBe(false);
  });

  it('accepts today and future valid dates', () => {
    expect(getDueDateInputError('2026-06-22')).toBeNull();
    expect(getDueDateInputError('2026-12-31')).toBeNull();
    expect(getDueDateInputError('2126-01-01')).toBeNull();
  });

  it('rejects malformed values', () => {
    expect(getDueDateInputError('2026-6-22')).toBe('Enter a valid due date.');
    expect(getDueDateInputError('06/22/2026')).toBe('Enter a valid due date.');
    expect(getDueDateInputError('2026-06-2')).toBe('Enter a valid due date.');
  });

  it('rejects impossible calendar dates', () => {
    expect(getDueDateInputError('2026-02-30')).toBe('Enter a valid due date.');
    expect(getDueDateInputError('2026-13-01')).toBe('Enter a valid due date.');
  });

  it('rejects past dates', () => {
    expect(getDueDateInputError('2026-06-21')).toBe(
      'Due date cannot be in the past.',
    );
    expect(getDueDateInputError('2026-01-01')).toBe(
      'Due date cannot be in the past.',
    );
  });

  it('rejects out-of-range years', () => {
    expect(getDueDateInputError('2025-12-31')).toBe(
      'Due date year must be between 2026 and 2126.',
    );
    expect(getDueDateInputError('2127-01-01')).toBe(
      'Due date year must be between 2026 and 2126.',
    );
    expect(getDueDateInputError('9999-01-01')).toBe(
      'Due date year must be between 2026 and 2126.',
    );
  });

  it('exposes today and max input bounds', () => {
    expect(getTodayDateInputValue()).toBe('2026-06-22');
    expect(getMaxDueDateInputValue()).toBe('2126-12-31');
  });
});
