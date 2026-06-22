import { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  createTask,
  deleteTask,
  getCategories,
  getTasks,
  getUsers,
  updateTask,
} from '../api/client';
import { useAuth } from '../auth/AuthContext';
import { Modal } from '../components/Modal';
import { TaskForm } from '../components/TaskForm';
import type { Category, Task, TaskFormData, UserSummary } from '../types';
import {
  formatDisplayDate,
  getUserNameById,
  toDateInputValue,
} from '../types';

function toFormData(task: Task): TaskFormData {
  return {
    categoryId: task.categoryId,
    title: task.title,
    description: task.description ?? '',
    assignee: task.assignee,
    dueDate: toDateInputValue(task.dueDate),
  };
}

export function TasksPage() {
  const navigate = useNavigate();
  const { user, logout } = useAuth();
  const [tasks, setTasks] = useState<Task[]>([]);
  const [users, setUsers] = useState<UserSummary[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [editingTask, setEditingTask] = useState<Task | null>(null);
  const [showAddModal, setShowAddModal] = useState(false);

  const loadData = useCallback(async () => {
    if (!user) {
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const [taskList, userList, categoryList] = await Promise.all([
        getTasks(user.id),
        getUsers(),
        getCategories(),
      ]);
      setTasks(taskList);
      setUsers(userList);
      setCategories(categoryList);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load tasks.');
    } finally {
      setLoading(false);
    }
  }, [user]);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  function closeModal() {
    setShowAddModal(false);
    setEditingTask(null);
  }

  async function handleCreate(values: TaskFormData) {
    if (!user) {
      return;
    }

    const created = await createTask(user.id, values);
    setTasks((current) => [created, ...current]);
    closeModal();
  }

  async function handleUpdate(values: TaskFormData) {
    if (!user || !editingTask) {
      return;
    }

    const updated = await updateTask(user.id, editingTask.id, values);
    setTasks((current) =>
      current.map((task) => (task.id === updated.id ? updated : task)),
    );
    closeModal();
  }

  async function handleDelete(taskId: string) {
    if (!user) {
      return;
    }

    await deleteTask(user.id, taskId);
    setTasks((current) => current.filter((task) => task.id !== taskId));
    if (editingTask?.id === taskId) {
      closeModal();
    }
  }

  function handleLogout() {
    logout();
    navigate('/login');
  }

  if (!user) {
    return null;
  }

  const modalOpen = showAddModal || editingTask !== null;

  return (
    <div className="page tasks-page">
      <header className="page-header">
        <div>
          <h1>My Tasks</h1>
          <p className="subtitle">Signed in as {user.email}</p>
        </div>
        <button type="button" className="secondary" onClick={handleLogout}>
          Log out
        </button>
      </header>

      {error && <p className="error banner">{error}</p>}

      <section className="panel">
        <div className="panel-header">
          <h2>Task List</h2>
          <button type="button" onClick={() => setShowAddModal(true)}>
            Add Task
          </button>
        </div>

        {loading ? (
          <p>Loading tasks...</p>
        ) : tasks.length === 0 ? (
          <p className="empty-state">
            No tasks yet. Click Add Task to create one.
          </p>
        ) : (
          <div className="task-list">
            {tasks.map((task) => (
              <article key={task.id} className="task-card">
                <div className="task-card-header">
                  <h3>{task.title}</h3>
                  <div className="task-actions">
                    <button
                      type="button"
                      className="secondary"
                      onClick={() => setEditingTask(task)}
                    >
                      Edit
                    </button>
                    <button
                      type="button"
                      className="danger"
                      onClick={() => void handleDelete(task.id)}
                    >
                      Delete
                    </button>
                  </div>
                </div>

                {task.description && <p>{task.description}</p>}

                <dl className="task-meta">
                  <div>
                    <dt>Category</dt>
                    <dd>{task.categoryName ?? '—'}</dd>
                  </div>
                  <div>
                    <dt>Assignee</dt>
                    <dd>{getUserNameById(users, task.assignee)}</dd>
                  </div>
                  <div>
                    <dt>Due Date</dt>
                    <dd>{formatDisplayDate(task.dueDate)}</dd>
                  </div>
                  <div>
                    <dt>Created</dt>
                    <dd>{formatDisplayDate(task.createDate)}</dd>
                  </div>
                </dl>
              </article>
            ))}
          </div>
        )}
      </section>

      {modalOpen && (
        <Modal
          title={editingTask ? 'Edit Task' : 'Add Task'}
          onClose={closeModal}
        >
          <TaskForm
            key={editingTask?.id ?? 'new'}
            initialValues={editingTask ? toFormData(editingTask) : undefined}
            users={users}
            categories={categories}
            submitLabel={editingTask ? 'Save Changes' : 'Add Task'}
            onSubmit={editingTask ? handleUpdate : handleCreate}
            onCancel={closeModal}
          />
        </Modal>
      )}
    </div>
  );
}
