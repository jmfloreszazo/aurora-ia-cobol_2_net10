import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { PageHeader, CardPanel, FormField, Button, Alert, StatusBadge } from '@/components/ui';

// Mock user data
const mockUser = {
  userId: 'USER001',
  firstName: 'John',
  lastName: 'Smith',
  userType: 'USER',
  status: 'A',
  lastLogin: '2024-01-15T09:45:00',
  createdDate: '2023-06-15',
  updatedDate: '2024-01-10',
};

interface UserForm {
  firstName: string;
  lastName: string;
  userType: string;
  status: string;
  newPassword: string;
  confirmPassword: string;
}

export const UserEditPage: React.FC = () => {
  const { userId } = useParams<{ userId: string }>();
  const navigate = useNavigate();
  
  const [form, setForm] = useState<UserForm>({
    firstName: mockUser.firstName,
    lastName: mockUser.lastName,
    userType: mockUser.userType,
    status: mockUser.status,
    newPassword: '',
    confirmPassword: '',
  });
  const [errors, setErrors] = useState<Partial<UserForm>>({});
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [changePassword, setChangePassword] = useState(false);

  const handleChange = (field: keyof UserForm, value: string) => {
    setForm(prev => ({ ...prev, [field]: value }));
    setErrors(prev => ({ ...prev, [field]: undefined }));
  };

  const validate = (): boolean => {
    const newErrors: Partial<UserForm> = {};

    if (!form.firstName.trim()) {
      newErrors.firstName = 'First name is required';
    } else if (form.firstName.length > 20) {
      newErrors.firstName = 'First name cannot exceed 20 characters';
    }

    if (!form.lastName.trim()) {
      newErrors.lastName = 'Last name is required';
    } else if (form.lastName.length > 20) {
      newErrors.lastName = 'Last name cannot exceed 20 characters';
    }

    if (changePassword) {
      if (!form.newPassword) {
        newErrors.newPassword = 'New password is required';
      } else if (form.newPassword.length < 8) {
        newErrors.newPassword = 'Password must be at least 8 characters';
      }

      if (form.newPassword !== form.confirmPassword) {
        newErrors.confirmPassword = 'Passwords do not match';
      }
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitError(null);

    if (!validate()) return;

    setIsSubmitting(true);
    
    try {
      // TODO: API call to update user
      // await userService.updateUser(userId, {...form});
      
      // Simulate API delay
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      navigate('/admin/users', { state: { message: 'User updated successfully' } });
    } catch (error) {
      setSubmitError('Failed to update user. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="pb-16">
      <PageHeader
        title="Update User"
        subtitle={`Security Administration - Edit User: ${userId}`}
        actions={
          <div className="flex space-x-3">
            <Button variant="secondary" onClick={() => navigate('/admin/users')}>
              F3=Cancel
            </Button>
            <Button variant="secondary" onClick={() => setForm({
              firstName: mockUser.firstName,
              lastName: mockUser.lastName,
              userType: mockUser.userType,
              status: mockUser.status,
              newPassword: '',
              confirmPassword: '',
            })}>
              F4=Reset
            </Button>
          </div>
        }
      />

      {submitError && (
        <div className="mb-4">
          <Alert type="error">{submitError}</Alert>
        </div>
      )}

      <form onSubmit={handleSubmit}>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* User Information */}
          <CardPanel title="User Information">
            <div className="space-y-4">
              <div>
                <label className="block text-xs text-gray-500 uppercase mb-1">User ID</label>
                <p className="font-mono text-sm bg-gray-100 px-3 py-2 rounded">{userId}</p>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <FormField
                  label="First Name"
                  name="firstName"
                  required
                  error={errors.firstName}
                >
                  <input
                    type="text"
                    className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    value={form.firstName}
                    onChange={(e) => handleChange('firstName', e.target.value)}
                    maxLength={20}
                  />
                </FormField>

                <FormField
                  label="Last Name"
                  name="lastName"
                  required
                  error={errors.lastName}
                >
                  <input
                    type="text"
                    className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    value={form.lastName}
                    onChange={(e) => handleChange('lastName', e.target.value)}
                    maxLength={20}
                  />
                </FormField>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <FormField
                  label="User Type"
                  name="userType"
                  required
                >
                  <select
                    className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    value={form.userType}
                    onChange={(e) => handleChange('userType', e.target.value)}
                  >
                    <option value="USER">User</option>
                    <option value="MANAGER">Manager</option>
                    <option value="ADMIN">Administrator</option>
                  </select>
                </FormField>

                <FormField
                  label="Status"
                  name="status"
                  required
                >
                  <select
                    className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    value={form.status}
                    onChange={(e) => handleChange('status', e.target.value)}
                  >
                    <option value="A">Active</option>
                    <option value="I">Inactive</option>
                  </select>
                </FormField>
              </div>
            </div>
          </CardPanel>

          {/* Audit Information */}
          <CardPanel title="Audit Information">
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs text-gray-500 uppercase mb-1">Status</label>
                  <StatusBadge 
                    status={mockUser.status === 'A' ? 'Active' : 'Inactive'} 
                    color={mockUser.status === 'A' ? 'green' : 'red'} 
                  />
                </div>
                <div>
                  <label className="block text-xs text-gray-500 uppercase mb-1">User Type</label>
                  <StatusBadge status={mockUser.userType} color="blue" />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs text-gray-500 uppercase mb-1">Created Date</label>
                  <p className="text-sm">{new Date(mockUser.createdDate).toLocaleDateString()}</p>
                </div>
                <div>
                  <label className="block text-xs text-gray-500 uppercase mb-1">Last Updated</label>
                  <p className="text-sm">{new Date(mockUser.updatedDate).toLocaleDateString()}</p>
                </div>
              </div>

              <div>
                <label className="block text-xs text-gray-500 uppercase mb-1">Last Login</label>
                <p className="text-sm">{new Date(mockUser.lastLogin).toLocaleString()}</p>
              </div>
            </div>
          </CardPanel>

          {/* Password Change */}
          <CardPanel title="Change Password">
            <div className="space-y-4">
              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="changePassword"
                  className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                  checked={changePassword}
                  onChange={(e) => {
                    setChangePassword(e.target.checked);
                    if (!e.target.checked) {
                      setForm(prev => ({ ...prev, newPassword: '', confirmPassword: '' }));
                      setErrors(prev => ({ ...prev, newPassword: undefined, confirmPassword: undefined }));
                    }
                  }}
                />
                <label htmlFor="changePassword" className="ml-2 text-sm text-gray-700">
                  Reset user password
                </label>
              </div>

              {changePassword && (
                <>
                  <FormField
                    label="New Password"
                    name="newPassword"
                    required
                    error={errors.newPassword}
                  >
                    <input
                      type="password"
                      className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                      value={form.newPassword}
                      onChange={(e) => handleChange('newPassword', e.target.value)}
                      placeholder="Minimum 8 characters"
                    />
                  </FormField>

                  <FormField
                    label="Confirm New Password"
                    name="confirmPassword"
                    required
                    error={errors.confirmPassword}
                  >
                    <input
                      type="password"
                      className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                      value={form.confirmPassword}
                      onChange={(e) => handleChange('confirmPassword', e.target.value)}
                      placeholder="Re-enter new password"
                    />
                  </FormField>
                </>
              )}
            </div>
          </CardPanel>
        </div>

        {/* Submit Button */}
        <div className="mt-6 flex justify-end">
          <Button type="submit" size="lg" disabled={isSubmitting}>
            {isSubmitting ? 'Saving...' : 'F5=Update'}
          </Button>
        </div>
      </form>
    </div>
  );
};
