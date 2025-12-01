import React from 'react';

export interface Column<T> {
  key: keyof T | string;
  header: string;
  width?: string;
  render?: (value: any, item: T) => React.ReactNode;
  className?: string;
}

export interface DataTableProps<T> {
  columns: Column<T>[];
  data: T[];
  keyExtractor?: (item: T) => string | number;
  keyField?: keyof T; // Alternative to keyExtractor
  onRowClick?: (item: T) => void;
  isLoading?: boolean;
  emptyMessage?: string;
  selectable?: boolean;
  selectedKey?: string | number | null;
  actions?: (item: T) => React.ReactNode;
}

export function DataTable<T extends Record<string, any>>({
  columns,
  data,
  keyExtractor,
  keyField,
  onRowClick,
  isLoading,
  emptyMessage = 'No data found.',
  selectable,
  selectedKey,
  actions,
}: DataTableProps<T>) {
  // Use keyExtractor if provided, otherwise use keyField
  const getKey = (item: T, index: number): string | number => {
    if (keyExtractor) return keyExtractor(item);
    if (keyField) return String(item[keyField]);
    return index;
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-48">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        <span className="ml-3 text-gray-600">Loading...</span>
      </div>
    );
  }

  if (data.length === 0) {
    return (
      <div className="flex items-center justify-center h-48 text-gray-500">
        {emptyMessage}
      </div>
    );
  }

  return (
    <div className="overflow-x-auto">
      <table className="min-w-full divide-y divide-gray-200">
        <thead className="bg-gray-800 text-white">
          <tr>
            {selectable && (
              <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider w-16">
                Sel
              </th>
            )}
            {columns.map((column) => (
              <th
                key={String(column.key)}
                className={`px-4 py-3 text-left text-xs font-medium uppercase tracking-wider ${column.className || ''}`}
                style={column.width ? { width: column.width } : undefined}
              >
                {column.header}
              </th>
            ))}
            {actions && (
              <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider">
                Actions
              </th>
            )}
          </tr>
        </thead>
        <tbody className="bg-white divide-y divide-gray-200">
          {data.map((item, index) => {
            const key = getKey(item, index);
            const isSelected = selectedKey != null && selectedKey === key;
            return (
              <tr
                key={key}
                onClick={() => onRowClick?.(item)}
                className={`
                  ${onRowClick ? 'cursor-pointer hover:bg-blue-50' : ''}
                  ${isSelected ? 'bg-blue-100' : index % 2 === 0 ? 'bg-white' : 'bg-gray-50'}
                  transition-colors
                `}
              >
                {selectable && (
                  <td className="px-4 py-3 whitespace-nowrap">
                    <input
                      type="radio"
                      checked={isSelected}
                      onChange={() => {}}
                      className="h-4 w-4 text-blue-600 focus:ring-blue-500"
                    />
                  </td>
                )}
                {columns.map((column) => {
                  const value = item[column.key as keyof T];
                  return (
                    <td
                      key={String(column.key)}
                      className={`px-4 py-3 whitespace-nowrap text-sm text-gray-900 ${column.className || ''}`}
                      style={column.width ? { width: column.width } : undefined}
                    >
                      {column.render
                        ? column.render(value, item)
                        : String(value ?? '')}
                    </td>
                  );
                })}
                {actions && (
                  <td className="px-4 py-3 whitespace-nowrap text-sm">
                    {actions(item)}
                  </td>
                )}
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
