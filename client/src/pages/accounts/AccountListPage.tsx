import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { accountService, customerService } from '@/services/apiService';
import { QUERY_KEYS } from '@/config/constants';
import { DataTable, PageHeader, StatusBadge, AmountDisplay, Button } from '@/components/ui';
import type { Account, Customer } from '@/types';

export const AccountListPage: React.FC = () => {
  const navigate = useNavigate();
  const [selectedCustomerId, setSelectedCustomerId] = useState<number | null>(null);

  // Get customers for dropdown
  const { data: customersData } = useQuery({
    queryKey: [QUERY_KEYS.CUSTOMERS],
    queryFn: () => customerService.getAllCustomers(1, 100),
  });

  // Get accounts for selected customer
  const { data: accounts, isLoading: accountsLoading } = useQuery({
    queryKey: [QUERY_KEYS.ACCOUNTS, selectedCustomerId],
    queryFn: () => accountService.getAccountsByCustomer(selectedCustomerId!),
    enabled: !!selectedCustomerId,
  });

  const columns = [
    { key: 'accountId', header: 'Account #' },
    { key: 'customerName', header: 'Customer Name' },
    {
      key: 'activeStatus',
      header: 'Status',
      render: (account: Account) => (
        <StatusBadge active={account.activeStatus === 'Y'} activeText="Y" inactiveText="N" />
      ),
    },
    {
      key: 'currentBalance',
      header: 'Current Balance',
      render: (account: Account) => <AmountDisplay amount={account.currentBalance} />,
    },
    {
      key: 'creditLimit',
      header: 'Credit Limit',
      render: (account: Account) => <AmountDisplay amount={account.creditLimit} />,
    },
    {
      key: 'availableCredit',
      header: 'Available Credit',
      render: (account: Account) => <AmountDisplay amount={account.availableCredit} colored />,
    },
    { key: 'numberOfCards', header: 'Cards' },
  ];

  return (
    <div className="pb-16">
      <PageHeader
        title="Account List"
        subtitle="View and manage credit card accounts"
      />

      {/* Search Section */}
      <div className="bg-white shadow rounded-lg p-6 mb-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Select Customer:
            </label>
            <select
              value={selectedCustomerId || ''}
              onChange={(e) => setSelectedCustomerId(e.target.value ? Number(e.target.value) : null)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="">-- Select a Customer --</option>
              {customersData?.items.map((customer: Customer) => (
                <option key={customer.customerId} value={customer.customerId}>
                  {customer.customerId} - {customer.fullName}
                </option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* Accounts Table */}
      <div className="bg-white shadow rounded-lg overflow-hidden">
        <div className="bg-gray-800 text-white px-4 py-3">
          <h3 className="text-sm font-medium">Accounts</h3>
        </div>
        
        {selectedCustomerId ? (
          <>
            <DataTable
              columns={columns}
              data={accounts || []}
              keyExtractor={(account) => account.accountId}
              isLoading={accountsLoading}
              emptyMessage="No accounts found for this customer."
              onRowClick={(account) => navigate(`/accounts/${account.accountId}`)}
              actions={(account) => (
                <div className="flex space-x-2">
                  <Button
                    size="sm"
                    onClick={(e) => {
                      e.stopPropagation();
                      navigate(`/accounts/${account.accountId}`);
                    }}
                  >
                    View
                  </Button>
                  <Button
                    size="sm"
                    variant="secondary"
                    onClick={(e) => {
                      e.stopPropagation();
                      navigate(`/accounts/${account.accountId}/edit`);
                    }}
                  >
                    Update
                  </Button>
                </div>
              )}
            />
          </>
        ) : (
          <div className="p-8 text-center text-gray-500">
            Please select a customer to view their accounts.
          </div>
        )}
      </div>
    </div>
  );
};
