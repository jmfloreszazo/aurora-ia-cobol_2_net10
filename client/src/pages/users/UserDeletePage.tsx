import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { PageHeader, CardPanel, Button, Alert, StatusBadge } from '@/components/ui';

// Mock user data
const mockUser = {
  userId: 'USER003',
  firstName: 'Bob',
  lastName: 'Wilson',
  userType: 'USER',
  status: 'I',
  lastLogin: '2023-12-01T08:00:00',
  createdDate: '2022-11-10',
};

export const UserDeletePage: React.FC = () => {
  const { userId } = useParams<{ userId: string }>();
  const navigate = useNavigate();
  
  const [confirmText, setConfirmText] = useState('');
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  const user = mockUser;
  const canDelete = confirmText.toUpperCase() === 'DELETE';

  const handleDelete = async () => {
    if (!canDelete) return;

    setIsDeleting(true);
    setSubmitError(null);
    
    try {
      // TODO: API call to delete user
      // await userService.deleteUser(userId);
      
      // Simulate API delay
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      navigate('/admin/users', { state: { message: 'User deleted successfully' } });
    } catch (error) {
      setSubmitError('Failed to delete user. Please try again.');
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <div className="pb-16">
      <PageHeader
        title="Delete User"
        subtitle={`Security Administration - Remove User: ${userId}`}
        actions={
          <Button variant="secondary" onClick={() => navigate('/admin/users')}>
            F3=Cancel
          </Button>
        }
      />

      <div className="max-w-2xl mx-auto">
        {/* Warning Alert */}
        <Alert type="warning">
          <div className="font-semibold mb-2">Warning: This action cannot be undone!</div>
          <p>You are about to permanently delete this user account. This will:</p>
          <ul className="list-disc list-inside mt-2 space-y-1">
            <li>Remove all access for this user</li>
            <li>Delete all user preferences and settings</li>
            <li>Remove the user from all assigned roles</li>
          </ul>
        </Alert>

        {submitError && (
          <div className="mt-4">
            <Alert type="error">{submitError}</Alert>
          </div>
        )}

        {/* User Details */}
        <CardPanel title="User to be Deleted" className="mt-6">
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs text-gray-500 uppercase mb-1">User ID</label>
                <p className="font-mono text-lg font-bold text-red-600">{userId}</p>
              </div>
              <div>
                <label className="block text-xs text-gray-500 uppercase mb-1">Status</label>
                <StatusBadge 
                  status={user.status === 'A' ? 'Active' : 'Inactive'} 
                  color={user.status === 'A' ? 'green' : 'red'} 
                />
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs text-gray-500 uppercase mb-1">First Name</label>
                <p className="text-sm">{user.firstName}</p>
              </div>
              <div>
                <label className="block text-xs text-gray-500 uppercase mb-1">Last Name</label>
                <p className="text-sm">{user.lastName}</p>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs text-gray-500 uppercase mb-1">User Type</label>
                <StatusBadge status={user.userType} color="blue" />
              </div>
              <div>
                <label className="block text-xs text-gray-500 uppercase mb-1">Last Login</label>
                <p className="text-sm">{new Date(user.lastLogin).toLocaleString()}</p>
              </div>
            </div>

            <div>
              <label className="block text-xs text-gray-500 uppercase mb-1">Account Created</label>
              <p className="text-sm">{new Date(user.createdDate).toLocaleDateString()}</p>
            </div>
          </div>
        </CardPanel>

        {/* Confirmation */}
        <CardPanel title="Confirmation Required" className="mt-6">
          <div className="space-y-4">
            <p className="text-sm text-gray-600">
              To confirm deletion, please type <span className="font-mono font-bold text-red-600">DELETE</span> in the field below:
            </p>

            <input
              type="text"
              className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-red-500 text-center font-mono uppercase"
              value={confirmText}
              onChange={(e) => setConfirmText(e.target.value)}
              placeholder="Type DELETE to confirm"
            />

            <div className="flex justify-end space-x-3 pt-4 border-t">
              <Button variant="secondary" onClick={() => navigate('/admin/users')}>
                F3=Cancel
              </Button>
              <Button 
                variant="danger" 
                onClick={handleDelete}
                disabled={!canDelete || isDeleting}
              >
                {isDeleting ? 'Deleting...' : 'F10=Confirm Delete'}
              </Button>
            </div>
          </div>
        </CardPanel>
      </div>
    </div>
  );
};
