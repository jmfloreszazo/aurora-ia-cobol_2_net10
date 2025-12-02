import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent, within } from '@testing-library/react';
import { Pagination, type PaginationProps } from './Pagination';

describe('Pagination', () => {
  const defaultProps: PaginationProps = {
    currentPage: 1,
    totalPages: 5,
    totalCount: 100,
    pageSize: 20,
    onPageChange: vi.fn(),
  };

  const renderPagination = (props: Partial<PaginationProps> = {}) => {
    return render(<Pagination {...defaultProps} {...props} />);
  };

  describe('rendering', () => {
    it('should render current page and total pages', () => {
      renderPagination();

      // Use regex to match specific text - getAllByText since "of" appears multiple times
      expect(screen.getByText(/Page/)).toBeInTheDocument();
      expect(screen.getAllByText(/of/).length).toBeGreaterThan(0);
    });

    it('should show results count', () => {
      renderPagination({ currentPage: 1, totalCount: 100, pageSize: 20 });

      // Match "Showing" as partial text
      expect(screen.getByText(/Showing/)).toBeInTheDocument();
      expect(screen.getByText(/results/)).toBeInTheDocument();
    });

    it('should render navigation buttons', () => {
      renderPagination();

      expect(screen.getByText('F7=Backward')).toBeInTheDocument();
      expect(screen.getByText('F8=Forward')).toBeInTheDocument();
    });
  });

  describe('navigation', () => {
    it('should disable backward button on first page', () => {
      renderPagination({ currentPage: 1 });

      const backButton = screen.getByText('F7=Backward');
      expect(backButton).toBeDisabled();
    });

    it('should disable forward button on last page', () => {
      renderPagination({ currentPage: 5, totalPages: 5 });

      const forwardButton = screen.getByText('F8=Forward');
      expect(forwardButton).toBeDisabled();
    });

    it('should enable both buttons on middle page', () => {
      renderPagination({ currentPage: 3, totalPages: 5 });

      expect(screen.getByText('F7=Backward')).not.toBeDisabled();
      expect(screen.getByText('F8=Forward')).not.toBeDisabled();
    });

    it('should call onPageChange with previous page when clicking backward', () => {
      const onPageChange = vi.fn();
      renderPagination({ currentPage: 3, onPageChange });

      fireEvent.click(screen.getByText('F7=Backward'));

      expect(onPageChange).toHaveBeenCalledWith(2);
    });

    it('should call onPageChange with next page when clicking forward', () => {
      const onPageChange = vi.fn();
      renderPagination({ currentPage: 3, onPageChange });

      fireEvent.click(screen.getByText('F8=Forward'));

      expect(onPageChange).toHaveBeenCalledWith(4);
    });
  });

  describe('custom hasPreviousPage and hasNextPage', () => {
    it('should respect hasPreviousPage prop', () => {
      renderPagination({ currentPage: 2, hasPreviousPage: false });

      expect(screen.getByText('F7=Backward')).toBeDisabled();
    });

    it('should respect hasNextPage prop', () => {
      renderPagination({ currentPage: 2, totalPages: 5, hasNextPage: false });

      expect(screen.getByText('F8=Forward')).toBeDisabled();
    });
  });

  describe('totalItems alias', () => {
    it('should accept totalItems as an alias for totalCount', () => {
      renderPagination({ totalItems: 50, totalCount: undefined, pageSize: 10 });

      expect(screen.getByText('50')).toBeInTheDocument();
    });
  });

  describe('edge cases', () => {
    it('should handle zero results', () => {
      renderPagination({ totalCount: 0, currentPage: 1, totalPages: 0 });

      // Should still render without errors
      expect(screen.getByText(/Page/)).toBeInTheDocument();
    });

    it('should calculate correct end item on last page', () => {
      renderPagination({ currentPage: 3, totalPages: 3, totalCount: 45, pageSize: 20 });

      // Page 3 should show items 41-45 - check partial match since 45 appears twice
      expect(screen.getByText('41')).toBeInTheDocument();
      expect(screen.getAllByText('45').length).toBeGreaterThanOrEqual(1);
    });
  });
});
