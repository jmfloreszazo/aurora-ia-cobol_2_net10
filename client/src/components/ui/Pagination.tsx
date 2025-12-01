import React from 'react';

export interface PaginationProps {
  currentPage: number;
  totalPages: number;
  totalCount?: number;
  totalItems?: number; // Alias for totalCount
  pageSize: number;
  hasPreviousPage?: boolean;
  hasNextPage?: boolean;
  onPageChange: (page: number) => void;
}

export const Pagination: React.FC<PaginationProps> = ({
  currentPage,
  totalPages,
  totalCount,
  totalItems,
  pageSize,
  hasPreviousPage,
  hasNextPage,
  onPageChange,
}) => {
  // Support both totalCount and totalItems
  const total = totalCount ?? totalItems ?? 0;
  
  // Calculate hasPrevious/hasNext if not provided
  const canGoPrevious = hasPreviousPage ?? currentPage > 1;
  const canGoNext = hasNextPage ?? currentPage < totalPages;
  
  const startItem = total > 0 ? (currentPage - 1) * pageSize + 1 : 0;
  const endItem = Math.min(currentPage * pageSize, total);

  return (
    <div className="flex items-center justify-between border-t border-gray-200 bg-gray-50 px-4 py-3">
      <div className="text-sm text-gray-700">
        Page <span className="font-medium">{currentPage}</span> of{' '}
        <span className="font-medium">{totalPages}</span>
        {total > 0 && (
          <span className="ml-4">
            Showing <span className="font-medium">{startItem}</span> to{' '}
            <span className="font-medium">{endItem}</span> of{' '}
            <span className="font-medium">{total}</span> results
          </span>
        )}
      </div>
      <div className="flex space-x-2">
        <button
          onClick={() => onPageChange(currentPage - 1)}
          disabled={!canGoPrevious}
          className="px-4 py-2 text-sm font-medium rounded-md border border-gray-300 bg-white text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          F7=Backward
        </button>
        <button
          onClick={() => onPageChange(currentPage + 1)}
          disabled={!canGoNext}
          className="px-4 py-2 text-sm font-medium rounded-md border border-gray-300 bg-white text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          F8=Forward
        </button>
      </div>
    </div>
  );
};
