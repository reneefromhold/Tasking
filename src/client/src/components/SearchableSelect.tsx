import { useEffect, useId, useRef, useState } from 'react';

export interface SearchableSelectOption {
  id: string;
  label: string;
}

interface SearchableSelectProps {
  id?: string;
  label: string;
  options: SearchableSelectOption[];
  value: string | null;
  onChange: (value: string | null) => void;
  placeholder?: string;
}

export function SearchableSelect({
  id,
  label,
  options,
  value,
  onChange,
  placeholder = 'Search...',
}: SearchableSelectProps) {
  const listId = useId();
  const containerRef = useRef<HTMLDivElement>(null);
  const [open, setOpen] = useState(false);
  const [query, setQuery] = useState('');

  const selected = options.find((option) => option.id === value);

  useEffect(() => {
    if (!open) {
      setQuery(selected?.label ?? '');
    }
  }, [open, selected]);

  const filteredOptions = options.filter((option) =>
    option.label.toLowerCase().includes(query.trim().toLowerCase()),
  );

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (
        containerRef.current &&
        !containerRef.current.contains(event.target as Node)
      ) {
        setOpen(false);
      }
    }

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  function handleSelect(option: SearchableSelectOption) {
    onChange(option.id);
    setQuery(option.label);
    setOpen(false);
  }

  function handleClear() {
    onChange(null);
    setQuery('');
    setOpen(false);
  }

  return (
    <div className="field searchable-select" ref={containerRef}>
      <label htmlFor={id}>{label}</label>
      <div className="searchable-select-control">
        <input
          id={id}
          type="text"
          role="combobox"
          aria-expanded={open}
          aria-controls={listId}
          aria-autocomplete="list"
          value={query}
          placeholder={placeholder}
          onChange={(event) => {
            setQuery(event.target.value);
            setOpen(true);
            if (value && event.target.value !== selected?.label) {
              onChange(null);
            }
          }}
          onFocus={() => setOpen(true)}
        />
        {value && (
          <button
            type="button"
            className="searchable-select-clear secondary"
            onClick={handleClear}
            aria-label="Clear selection"
          >
            ×
          </button>
        )}
      </div>

      {open && (
        <ul id={listId} className="searchable-select-list" role="listbox">
          {filteredOptions.length === 0 ? (
            <li className="searchable-select-empty">No matches found</li>
          ) : (
            filteredOptions.map((option) => (
              <li key={option.id}>
                <button
                  type="button"
                  role="option"
                  aria-selected={option.id === value}
                  className={option.id === value ? 'selected' : undefined}
                  onClick={() => handleSelect(option)}
                >
                  {option.label}
                </button>
              </li>
            ))
          )}
        </ul>
      )}
    </div>
  );
}
