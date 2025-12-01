import React, { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { transactionService, accountService, customerService } from '@/services/apiService';
import { QUERY_KEYS } from '@/config/constants';
import { DataTable, Pagination, PageHeader, AmountDisplay, Button } from '@/components/ui';
import type { Transaction, Customer, Account } from '@/types';

export const TransactionListPage: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [selectedCustomerId, setSelectedCustomerId] = useState<number | null>(null);
  const [selectedAccountId, setSelectedAccountId] = useState<number | null>(
    searchParams.get('accountId') ? Number(searchParams.get('accountId')) : null
  );
  const [searchTranId, setSearchTranId] = useState('');
  const [page, setPage] = useState(1);
  const pageSize = 10;

  // Get customers
  const { data: customersData } = useQuery({
    queryKey: [QUERY_KEYS.CUSTOMERS],
    queryFn: () => customerService.getAllCustomers(1, 100),
  });

  // Get accounts for selected customer
  const { data: accounts } = useQuery({
    queryKey: [QUERY_KEYS.ACCOUNTS, selectedCustomerId],
    queryFn: () => accountService.getAccountsByCustomer(selectedCustomerId!),
    enabled: !!selectedCustomerId,
  });

  // Get transactions for selected account
  const { data: transactionsData, isLoading } = useQuery({
    queryKey: [QUERY_KEYS.TRANSACTIONS, selectedAccountId, page],
    queryFn: () => transactionService.getTransactionsByAccount(selectedAccountId!, page, pageSize),
    enabled: !!selectedAccountId,
  });

  const columns = [
    {
      key: 'transactionId',
      header: 'Transaction ID',
      render: (trn: Transaction) => <span className="font-mono text-xs">{trn.transactionId}</span>,
    },
    {
      key: 'transactionDate',
      header: 'Date',
      render: (trn: Transaction) => (
        <span className="font-mono">{new Date(trn.transactionDate).toLocaleDateString()}</span>
      ),
    },
    {
      key: 'description',
      header: 'Description',
      render: (trn: Transaction) => (
        <span className="truncate max-w-[200px] block" title={trn.description}>
          {trn.description}
        </span>
      ),
    },
    {
      key: 'amount',
      header: 'Amount',
      render: (trn: Transaction) => <AmountDisplay amount={trn.amount} colored />,
    },
  ];

  return (
    <div className="pb-16">
      <PageHeader
        title="List Transactions"
        subtitle="View and manage transactions"
        actions={
          <div className="flex items-center space-x-4">
            <span className="text-sm text-gray-500">
              Page: {transactionsData?.pageNumber || 1} / {transactionsData?.totalPages || 1}
            </span>
            <Button onClick={() => navigate('/transactions/add')}>Add Transaction</Button>
          </div>
        }
      />

      {/* Search Section */}
      <div className="bg-white shadow rounded-lg p-6 mb-6">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Select Customer:
            </label>
            <select
              value={selectedCustomerId || ''}
              onChange={(e) => {
                setSelectedCustomerId(e.target.value ? Number(e.target.value) : null);
                setSelectedAccountId(null);
              }}
              className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="">-- Select Customer --</option>
              {customersData?.items.map((customer: Customer) => (
                <option key={customer.customerId} value={customer.customerId}>
                  {customer.customerId} - {customer.fullName}
                </option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Account Number:
            </label>
            <select
              value={selectedAccountId || ''}
              onChange={(e) => setSelectedAccountId(e.target.value ? Number(e.target.value) : null)}
              disabled={!selectedCustomerId}
              className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 disabled:bg-gray-100"
            >
              <option value="">-- Select Account --</option>
              {accounts?.map((account: Account) => (
                <option key={account.accountId} value={account.accountId}>
                  {account.accountId}
                </option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Search Tran ID:
            </label>
            <input
              type="text"
              value={searchTranId}
              onChange={(e) => setSearchTranId(e.target.value)}
              placeholder="Enter transaction ID"
              className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 font-mono"
            />
          </div>
        </div>
      </div>

      {/* Transactions Table */}
      <div className="bg-white shadow rounded-lg overflow-hidden">
        <div className="bg-gray-800 text-white px-4 py-3">
          <div className="flex items-center justify-between">
            <h3 className="text-sm font-medium">Transactions</h3>
            <div className="text-xs text-gray-300">
              Type 'S' to View Transaction details from the list
            </div>
          </div>
        </div>
        
        {selectedAccountId ? (
          <>
            <DataTable
              columns={columns}
              data={transactionsData?.items || []}
              keyExtractor={(trn) => trn.transactionId}
              isLoading={isLoading}
              emptyMessage="No transactions found for this account."
              onRowClick={(trn) => navigate(`/transactions/${trn.transactionId}`)}
              selectable
              actions={(trn) => (
                <Button
                  size="sm"
                  onClick={(e) => {
                    e.stopPropagation();
                    navigate(`/transactions/${trn.transactionId}`);
                  }}
                >
                  S
                </Button>
              )}
            />
            {transactionsData && (
              <Pagination
                currentPage={transactionsData.pageNumber}
                totalPages={transactionsData.totalPages}
                totalCount={transactionsData.totalCount}
                pageSize={transactionsData.pageSize}
                hasPreviousPage={transactionsData.hasPreviousPage}
                hasNextPage={transactionsData.hasNextPage}
                onPageChange={setPage}
              />
            )}
          </>
        ) : (
          <div className="p-8 text-center text-gray-500">
            Please select a customer and account to view transactions.
          </div>
        )}
      </div>
    </div>
  );
};
