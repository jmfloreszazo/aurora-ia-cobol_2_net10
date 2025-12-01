import React from 'react';
import { useAuth } from '@/contexts/AuthContext';
import { useQuery } from '@tanstack/react-query';
import { customerService } from '@/services/apiService';
import { QUERY_KEYS } from '@/config/constants';

export const DashboardPage: React.FC = () => {
  const { user, logout } = useAuth();
  
  const { data: customersData, isLoading } = useQuery({
    queryKey: [QUERY_KEYS.CUSTOMERS, 1],
    queryFn: () => customerService.getAllCustomers(1, 10)
  });

  const handleLogout = () => {
    logout();
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <h1 className="text-xl font-bold text-gray-900">CardDemo</h1>
            </div>
            <div className="flex items-center space-x-4">
              <span className="text-sm text-gray-700">
                Welcome, <span className="font-medium">{user?.fullName}</span>
              </span>
              <span className="px-2 py-1 text-xs font-medium rounded-full bg-blue-100 text-blue-800">
                {user?.userType}
              </span>
              <button
                onClick={handleLogout}
                className="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-md hover:bg-red-700"
              >
                Logout
              </button>
            </div>
          </div>
        </div>
      </nav>

      <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="px-4 py-6 sm:px-0">
          <h2 className="text-2xl font-bold text-gray-900 mb-6">Dashboard</h2>
          
          <div className="bg-white shadow rounded-lg p-6">
            <h3 className="text-lg font-medium text-gray-900 mb-4">Customers</h3>
            
            {isLoading ? (
              <p className="text-gray-500">Loading customers...</p>
            ) : customersData && customersData.items.length > 0 ? (
              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Customer ID
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Name
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        FICO Score
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Accounts
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Phone
                      </th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {customersData.items.map((customer: any) => (
                      <tr key={customer.customerId}>
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                          {customer.customerId}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                          {customer.fullName}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                          {customer.ficoScore}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                          {customer.numberOfAccounts}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                          {customer.phoneNumber1}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                
                <div className="mt-4 flex items-center justify-between border-t border-gray-200 pt-4">
                  <p className="text-sm text-gray-700">
                    Showing <span className="font-medium">{customersData.items.length}</span> of{' '}
                    <span className="font-medium">{customersData.totalCount}</span> customers
                  </p>
                  <div className="flex space-x-2">
                    <button
                      disabled={!customersData.hasPreviousPage}
                      className="px-3 py-1 text-sm font-medium rounded-md border border-gray-300 bg-white text-gray-700 hover:bg-gray-50 disabled:opacity-50"
                    >
                      Previous
                    </button>
                    <button
                      disabled={!customersData.hasNextPage}
                      className="px-3 py-1 text-sm font-medium rounded-md border border-gray-300 bg-white text-gray-700 hover:bg-gray-50 disabled:opacity-50"
                    >
                      Next
                    </button>
                  </div>
                </div>
              </div>
            ) : (
              <p className="text-gray-500">No customers found.</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};
