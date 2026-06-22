import { useEffect, useState } from 'react';
import { SearchableSelect } from './SearchableSelect';
import type { Category, TaskFormData, UserSummary } from '../types';
import {
  formatUserName,
  getDueDateInputError,
  getMaxDueDateInputValue,
  getTodayDateInputValue,
} from '../types';

interface TaskFormProps {
  initialValues?: TaskFormData;
  users: UserSummary[];
  categories: Category[];
  submitLabel: string;
  onSubmit: (values: TaskFormData) => Promise<void>;
  onCancel?: () => void;
}

const emptyValues: TaskFormData = {
  categoryId: null,
  title: '',
  description: '',
  assignee: null,
  dueDate: '',
};

export function TaskForm({
  initialValues,
  users,
  categories,
  submitLabel,
  onSubmit,
  onCancel,
}: TaskFormProps) {
  const [values, setValues] = useState<TaskFormData>(initialValues ?? emptyValues);
  const [error, setError] = useState<string | null>(null);
  const [dueDateError, setDueDateError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    setValues(initialValues ?? emptyValues);
    setDueDateError(null);
    setError(null);
  }, [initialValues]);

  function handleDueDateChange(value: string) {
    setValues((current) => ({ ...current, dueDate: value }));
    setDueDateError(getDueDateInputError(value));
  }

  function handleDueDateBlur() {
    setDueDateError(getDueDateInputError(values.dueDate));
  }

  const dueDateValidationError = getDueDateInputError(values.dueDate);
  const canSubmit = !submitting && !dueDateValidationError;

  const categoryOptions = categories.map((category) => ({
    id: category.id,
    label: category.category,
  }));

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault();
    setError(null);

    const validationError = getDueDateInputError(values.dueDate);
    if (validationError) {
      setDueDateError(validationError);
      return;
    }

    setDueDateError(null);

    setSubmitting(true);

    try {
      await onSubmit(values);
      if (!initialValues) {
        setValues(emptyValues);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Something went wrong.');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form className="task-form" onSubmit={handleSubmit}>
      <SearchableSelect
        id="category"
        label="Category"
        options={categoryOptions}
        value={values.categoryId}
        onChange={(categoryId) =>
          setValues((current) => ({ ...current, categoryId }))
        }
        placeholder="Search categories..."
      />

      <div className="field">
        <label htmlFor="title">Title</label>
        <input
          id="title"
          value={values.title}
          onChange={(event) =>
            setValues((current) => ({ ...current, title: event.target.value }))
          }
          required
          maxLength={30}
        />
      </div>

      <div className="field">
        <label htmlFor="description">Description</label>
        <textarea
          id="description"
          value={values.description}
          onChange={(event) =>
            setValues((current) => ({
              ...current,
              description: event.target.value,
            }))
          }
          rows={3}
          maxLength={500}
        />
      </div>

      <div className="field">
        <label htmlFor="assignee">Assignee</label>
        <select
          id="assignee"
          value={values.assignee ?? ''}
          onChange={(event) =>
            setValues((current) => ({
              ...current,
              assignee: event.target.value || null,
            }))
          }
        >
          <option value="">Unassigned</option>
          {users.map((user) => (
            <option key={user.id} value={user.id}>
              {formatUserName(user)} ({user.email})
            </option>
          ))}
        </select>
      </div>

      <div className="field">
        <label htmlFor="dueDate">Due Date</label>
        <input
          id="dueDate"
          type="date"
          value={values.dueDate}
          min={getTodayDateInputValue()}
          max={getMaxDueDateInputValue()}
          onChange={(event) => handleDueDateChange(event.target.value)}
          onBlur={handleDueDateBlur}
          aria-invalid={dueDateError ? true : undefined}
          aria-describedby={dueDateError ? 'dueDate-error' : undefined}
        />
        {dueDateError && (
          <p className="error" id="dueDate-error">
            {dueDateError}
          </p>
        )}
      </div>

      {error && <p className="error">{error}</p>}

      <div className="form-actions">
        <button type="submit" disabled={!canSubmit}>
          {submitting ? 'Saving...' : submitLabel}
        </button>
        {onCancel && (
          <button type="button" className="secondary" onClick={onCancel}>
            Cancel
          </button>
        )}
      </div>
    </form>
  );
}
