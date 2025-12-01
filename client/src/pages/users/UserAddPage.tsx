import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { PageHeader, CardPanel, FormField, Button, Alert } from '@/components/ui';

interface UserForm {
  userId: string;
  firstName: string;
  lastName: string;
  userType: string;
  password: string;
  confirmPassword: string;
}

const initialForm: UserForm = {
  userId: '',
  firstName: '',
  lastName: '',
  userType: 'USER',
  password: '',
  confirmPassword: '',
};

export const UserAddPage: React.FC = () => {
  const navigate = useNavigate();
  const [form, setForm] = useState<UserForm>(initialForm);
  const [errors, setErrors] = useState<Partial<UserForm>>({});
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleChange = (field: keyof UserForm, value: string) => {
    setForm(prev => ({ ...prev, [field]: value }));
    setErrors(prev => ({ ...prev, [field]: undefined }));
  };

  const validate = (): boolean => {
    const newErrors: Partial<UserForm> = {};

    if (!form.userId.trim()) {
      newErrors.userId = 'User ID is required';
    } else if (form.userId.length < 3 || form.userId.length > 8) {
      newErrors.userId = 'User ID must be 3-8 characters';
    } else if (!/^[A-Z0-9]+$/.test(form.userId.toUpperCase())) {
      newErrors.userId = 'User ID must be alphanumeric only';
    }

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

    if (!form.password) {
      newErrors.password = 'Password is required';
    } else if (form.password.length < 8) {
      newErrors.password = 'Password must be at least 8 characters';
    }

    if (form.password !== form.confirmPassword) {
      newErrors.confirmPassword = 'Passwords do not match';
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
      // TODO: API call to create user
      // await userService.createUser({...form});
      
      // Simulate API delay
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      navigate('/admin/users', { state: { message: 'User created successfully' } });
    } catch (error) {
      setSubmitError('Failed to create user. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="pb-16">
      <PageHeader
        title="Add User"
        subtitle="Security Administration - Create New User"
        actions={
          <div className="flex space-x-3">
            <Button variant="secondary" onClick={() => navigate('/admin/users')}>
              F3=Cancel
            </Button>
            <Button variant="secondary" onClick={() => setForm(initialForm)}>
              F4=Clear
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
              <FormField
                label="User ID"
                name="userId"
                required
                error={errors.userId}
              >
                <input
                  type="text"
                  className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 font-mono uppercase"
                  value={form.userId}
                  onChange={(e) => handleChange('userId', e.target.value.toUpperCase())}
                  maxLength={8}
                  placeholder="e.g., USER001"
                />
              </FormField>

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
                    placeholder="John"
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
                    placeholder="Smith"
                  />
                </FormField>
              </div>

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
            </div>
          </CardPanel>

          {/* Password */}
          <CardPanel title="Password">
            <div className="space-y-4">
              <FormField
                label="Password"
                name="password"
                required
                error={errors.password}
              >
                <input
                  type="password"
                  className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  value={form.password}
                  onChange={(e) => handleChange('password', e.target.value)}
                  placeholder="Minimum 8 characters"
                />
              </FormField>

              <FormField
                label="Confirm Password"
                name="confirmPassword"
                required
                error={errors.confirmPassword}
              >
                <input
                  type="password"
                  className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  value={form.confirmPassword}
                  onChange={(e) => handleChange('confirmPassword', e.target.value)}
                  placeholder="Re-enter password"
                />
              </FormField>

              <div className="text-sm text-gray-500">
                <p className="font-semibold mb-2">Password Requirements:</p>
                <ul className="list-disc list-inside space-y-1">
                  <li>Minimum 8 characters</li>
                  <li>Case sensitive</li>
                </ul>
              </div>
            </div>
          </CardPanel>
        </div>

        {/* Submit Button */}
        <div className="mt-6 flex justify-end">
          <Button type="submit" size="lg" disabled={isSubmitting}>
            {isSubmitting ? 'Creating...' : 'F5=Submit'}
          </Button>
        </div>
      </form>
    </div>
  );
};
