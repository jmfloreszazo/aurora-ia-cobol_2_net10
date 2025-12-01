import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { PageHeader, DataTable, Pagination, StatusBadge, Button } from '@/components/ui';

// Mock users data
const mockUsers = [
  { 
    userId: 'ADMIN001', 
    firstName: 'System', 
    lastName: 'Administrator', 
    userType: 'ADMIN',
    status: 'A',
    lastLogin: '2024-01-15T10:30:00',
    createdDate: '2020-01-01'
  },
  { 
    userId: 'USER001', 
    firstName: 'John', 
    lastName: 'Smith', 
    userType: 'USER',
    status: 'A',
    lastLogin: '2024-01-15T09:45:00',
    createdDate: '2023-06-15'
  },
  { 
    userId: 'USER002', 
    firstName: 'Jane', 
    lastName: 'Doe', 
    userType: 'USER',
    status: 'A',
    lastLogin: '2024-01-14T16:20:00',
    createdDate: '2023-07-22'
  },
  { 
    userId: 'USER003', 
    firstName: 'Bob', 
    lastName: 'Wilson', 
    userType: 'USER',
    status: 'I',
    lastLogin: '2023-12-01T08:00:00',
    createdDate: '2022-11-10'
  },
  { 
    userId: 'MANAGER01', 
    firstName: 'Sarah', 
    lastName: 'Johnson', 
    userType: 'MANAGER',
    status: 'A',
    lastLogin: '2024-01-15T11:15:00',
    createdDate: '2021-03-05'
  },
];

const columns = [
  { 
    key: 'userId', 
    header: 'User ID', 
    width: '120px',
    render: (value: string) => <span className="font-mono">{value}</span>
  },
  { 
    key: 'firstName', 
    header: 'First Name', 
    width: '150px' 
  },
  { 
    key: 'lastName', 
    header: 'Last Name', 
    width: '150px' 
  },
  { 
    key: 'userType', 
    header: 'Type', 
    width: '100px',
    render: (value: string) => {
      const colors: Record<string, 'blue' | 'green' | 'yellow'> = {
        'ADMIN': 'blue',
        'MANAGER': 'yellow',
        'USER': 'green',
      };
      return <StatusBadge status={value} color={colors[value] || 'gray'} />;
    }
  },
  { 
    key: 'status', 
    header: 'Status', 
    width: '80px',
    render: (value: string) => (
      <StatusBadge 
        status={value === 'A' ? 'Active' : 'Inactive'} 
        color={value === 'A' ? 'green' : 'red'} 
      />
    )
  },
  { 
    key: 'lastLogin', 
    header: 'Last Login', 
    width: '150px',
    render: (value: string) => value ? new Date(value).toLocaleString() : '-'
  },
];

export const UserListPage: React.FC = () => {
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [typeFilter, setTypeFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [selectedUser, setSelectedUser] = useState<string | null>(null);

  const pageSize = 10;
  
  // Filter users
  const filteredUsers = mockUsers.filter(user => {
    const matchesSearch = !search || 
      user.userId.toLowerCase().includes(search.toLowerCase()) ||
      user.firstName.toLowerCase().includes(search.toLowerCase()) ||
      user.lastName.toLowerCase().includes(search.toLowerCase());
    const matchesType = !typeFilter || user.userType === typeFilter;
    const matchesStatus = !statusFilter || user.status === statusFilter;
    return matchesSearch && matchesType && matchesStatus;
  });

  const totalItems = filteredUsers.length;
  const totalPages = Math.ceil(totalItems / pageSize);
  const paginatedUsers = filteredUsers.slice((page - 1) * pageSize, page * pageSize);

  const handleRowClick = (user: typeof mockUsers[0]) => {
    setSelectedUser(user.userId);
  };

  const handleView = () => {
    if (selectedUser) {
      navigate(`/admin/users/${selectedUser}`);
    }
  };

  const handleEdit = () => {
    if (selectedUser) {
      navigate(`/admin/users/${selectedUser}/edit`);
    }
  };

  const handleDelete = () => {
    if (selectedUser) {
      navigate(`/admin/users/${selectedUser}/delete`);
    }
  };

  return (
    <div className="pb-16">
      <PageHeader
        title="User List"
        subtitle="Security Administration - User Management"
        actions={
          <div className="flex space-x-3">
            <Button variant="secondary" onClick={() => navigate('/admin')}>
              F3=Admin Menu
            </Button>
            <Button onClick={() => navigate('/admin/users/new')}>
              F6=Add User
            </Button>
          </div>
        }
      />

      {/* Filters */}
      <div className="bg-white rounded-lg shadow p-4 mb-6">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div>
            <label className="block text-xs text-gray-500 uppercase mb-1">Search</label>
            <input
              type="text"
              className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="User ID, Name..."
              value={search}
              onChange={(e) => {
                setSearch(e.target.value);
                setPage(1);
              }}
            />
          </div>
          <div>
            <label className="block text-xs text-gray-500 uppercase mb-1">User Type</label>
            <select
              className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              value={typeFilter}
              onChange={(e) => {
                setTypeFilter(e.target.value);
                setPage(1);
              }}
            >
              <option value="">All Types</option>
              <option value="ADMIN">Admin</option>
              <option value="MANAGER">Manager</option>
              <option value="USER">User</option>
            </select>
          </div>
          <div>
            <label className="block text-xs text-gray-500 uppercase mb-1">Status</label>
            <select
              className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              value={statusFilter}
              onChange={(e) => {
                setStatusFilter(e.target.value);
                setPage(1);
              }}
            >
              <option value="">All Status</option>
              <option value="A">Active</option>
              <option value="I">Inactive</option>
            </select>
          </div>
          <div className="flex items-end">
            <Button 
              variant="secondary" 
              onClick={() => {
                setSearch('');
                setTypeFilter('');
                setStatusFilter('');
                setPage(1);
              }}
            >
              Clear Filters
            </Button>
          </div>
        </div>
      </div>

      {/* Users Table */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        <DataTable
          data={paginatedUsers}
          columns={columns}
          keyField="userId"
          onRowClick={handleRowClick}
          selectedKey={selectedUser}
          emptyMessage="No users found"
        />

        <div className="border-t p-4">
          <Pagination
            currentPage={page}
            totalPages={totalPages}
            totalItems={totalItems}
            pageSize={pageSize}
            onPageChange={setPage}
          />
        </div>
      </div>

      {/* Action Bar */}
      {selectedUser && (
        <div className="fixed bottom-0 left-0 right-0 bg-gray-800 text-white p-4">
          <div className="max-w-7xl mx-auto flex items-center justify-between">
            <span>
              Selected: <span className="font-mono font-bold">{selectedUser}</span>
            </span>
            <div className="flex space-x-3">
              <Button variant="secondary" onClick={handleView}>
                F4=View
              </Button>
              <Button onClick={handleEdit}>
                F5=Update
              </Button>
              <Button variant="danger" onClick={handleDelete}>
                F10=Delete
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
