import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { DataTable, type Column } from './DataTable';

interface TestItem {
  id: number;
  name: string;
  status: string;
}

describe('DataTable', () => {
  const mockData: TestItem[] = [
    { id: 1, name: 'Item 1', status: 'Active' },
    { id: 2, name: 'Item 2', status: 'Inactive' },
    { id: 3, name: 'Item 3', status: 'Active' },
  ];

  const columns: Column<TestItem>[] = [
    { key: 'id', header: 'ID' },
    { key: 'name', header: 'Name' },
    { key: 'status', header: 'Status' },
  ];

  describe('rendering', () => {
    it('should render table headers', () => {
      render(<DataTable columns={columns} data={mockData} keyField="id" />);

      expect(screen.getByText('ID')).toBeInTheDocument();
      expect(screen.getByText('Name')).toBeInTheDocument();
      expect(screen.getByText('Status')).toBeInTheDocument();
    });

    it('should render data rows', () => {
      render(<DataTable columns={columns} data={mockData} keyField="id" />);

      expect(screen.getByText('Item 1')).toBeInTheDocument();
      expect(screen.getByText('Item 2')).toBeInTheDocument();
      expect(screen.getByText('Item 3')).toBeInTheDocument();
    });

    it('should render empty message when no data', () => {
      render(
        <DataTable 
          columns={columns} 
          data={[]} 
          keyField="id" 
          emptyMessage="No items found" 
        />
      );

      expect(screen.getByText('No items found')).toBeInTheDocument();
    });

    it('should show loading state', () => {
      render(
        <DataTable columns={columns} data={[]} keyField="id" isLoading={true} />
      );

      expect(screen.getByText('Loading...')).toBeInTheDocument();
    });
  });

  describe('custom rendering', () => {
    it('should use custom render function for column', () => {
      const columnsWithRender: Column<TestItem>[] = [
        { key: 'id', header: 'ID' },
        { 
          key: 'status', 
          header: 'Status',
          render: (value) => <span data-testid="custom-status">{value.toUpperCase()}</span>
        },
      ];

      render(<DataTable columns={columnsWithRender} data={mockData} keyField="id" />);

      const customElements = screen.getAllByTestId('custom-status');
      expect(customElements[0]).toHaveTextContent('ACTIVE');
    });
  });

  describe('row interaction', () => {
    it('should call onRowClick when row is clicked', () => {
      const onRowClick = vi.fn();
      render(
        <DataTable 
          columns={columns} 
          data={mockData} 
          keyField="id"
          onRowClick={onRowClick} 
        />
      );

      fireEvent.click(screen.getByText('Item 1'));

      expect(onRowClick).toHaveBeenCalledWith(mockData[0]);
    });

    it('should apply hover styles when onRowClick is provided', () => {
      const onRowClick = vi.fn();
      const { container } = render(
        <DataTable 
          columns={columns} 
          data={mockData} 
          keyField="id"
          onRowClick={onRowClick} 
        />
      );

      const row = container.querySelector('tbody tr');
      expect(row).toHaveClass('cursor-pointer');
    });
  });

  describe('selection', () => {
    it('should render selection column when selectable', () => {
      render(
        <DataTable 
          columns={columns} 
          data={mockData} 
          keyField="id"
          selectable={true}
        />
      );

      expect(screen.getByText('Sel')).toBeInTheDocument();
    });

    it('should highlight selected row', () => {
      const { container } = render(
        <DataTable 
          columns={columns} 
          data={mockData} 
          keyExtractor={(item) => item.id}
          selectable={true}
          selectedKey={1}
        />
      );

      const rows = container.querySelectorAll('tbody tr');
      expect(rows[0]).toHaveClass('bg-blue-100');
    });
  });

  describe('actions column', () => {
    it('should render actions column when provided', () => {
      render(
        <DataTable 
          columns={columns} 
          data={mockData} 
          keyField="id"
          actions={(item) => (
            <button data-testid={`action-${item.id}`}>Edit</button>
          )}
        />
      );

      expect(screen.getByText('Actions')).toBeInTheDocument();
      expect(screen.getByTestId('action-1')).toBeInTheDocument();
    });
  });

  describe('keyExtractor', () => {
    it('should use keyExtractor when provided', () => {
      const keyExtractor = vi.fn((item: TestItem) => `key-${item.id}`);
      
      render(
        <DataTable 
          columns={columns} 
          data={mockData} 
          keyExtractor={keyExtractor}
        />
      );

      expect(keyExtractor).toHaveBeenCalledTimes(mockData.length);
    });
  });

  describe('column width', () => {
    it('should apply column width when specified', () => {
      const columnsWithWidth: Column<TestItem>[] = [
        { key: 'id', header: 'ID', width: '100px' },
        { key: 'name', header: 'Name' },
      ];

      const { container } = render(
        <DataTable columns={columnsWithWidth} data={mockData} keyField="id" />
      );

      const headerCell = container.querySelector('th');
      expect(headerCell).toHaveStyle({ width: '100px' });
    });
  });
});
