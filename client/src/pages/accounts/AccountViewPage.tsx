import React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { accountService } from '@/services/apiService';
import { QUERY_KEYS } from '@/config/constants';
import { PageHeader, CardPanel, StatusBadge, AmountDisplay, Button } from '@/components/ui';

export const AccountViewPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data: account, isLoading, error } = useQuery({
    queryKey: [QUERY_KEYS.ACCOUNT, id],
    queryFn: () => accountService.getAccountById(Number(id)),
    enabled: !!id,
  });

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        <span className="ml-3 text-gray-600">Loading account...</span>
      </div>
    );
  }

  if (error || !account) {
    return (
      <div className="bg-red-50 p-4 rounded-lg">
        <p className="text-red-800">Error loading account. Please try again.</p>
        <Button variant="secondary" onClick={() => navigate('/accounts')} className="mt-4">
          Back to Accounts
        </Button>
      </div>
    );
  }

  return (
    <div className="pb-16">
      <PageHeader
        title="View Account"
        subtitle={`Account Number: ${account.accountId}`}
        actions={
          <div className="flex space-x-3">
            <Button variant="secondary" onClick={() => navigate('/accounts')}>
              F3=Exit
            </Button>
            <Button onClick={() => navigate(`/accounts/${id}/edit`)}>
              Update Account
            </Button>
          </div>
        }
      />

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Account Information */}
        <CardPanel title="Account Information">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs text-gray-500 uppercase">Account Number</label>
              <p className="text-lg font-mono font-bold">{account.accountId}</p>
            </div>
            <div>
              <label className="block text-xs text-gray-500 uppercase">Active Y/N</label>
              <StatusBadge active={account.activeStatus === 'Y'} activeText="Y" inactiveText="N" />
            </div>
            <div>
              <label className="block text-xs text-gray-500 uppercase">Opened</label>
              <p className="font-mono">{new Date(account.openDate).toLocaleDateString()}</p>
            </div>
            <div>
              <label className="block text-xs text-gray-500 uppercase">Expiry</label>
              <p className="font-mono">{new Date(account.expirationDate).toLocaleDateString()}</p>
            </div>
          </div>
        </CardPanel>

        {/* Credit Limits */}
        <CardPanel title="Credit Limits">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs text-gray-500 uppercase">Credit Limit</label>
              <AmountDisplay amount={account.creditLimit} />
            </div>
            <div>
              <label className="block text-xs text-gray-500 uppercase">Cash Credit Limit</label>
              <AmountDisplay amount={account.cashCreditLimit} />
            </div>
            <div>
              <label className="block text-xs text-gray-500 uppercase">Available Credit</label>
              <AmountDisplay amount={account.availableCredit} colored />
            </div>
            <div>
              <label className="block text-xs text-gray-500 uppercase">Credit Utilization</label>
              <p className="font-mono">{account.creditUtilization.toFixed(1)}%</p>
            </div>
          </div>
        </CardPanel>

        {/* Balance Information */}
        <CardPanel title="Balance Information">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs text-gray-500 uppercase">Current Balance</label>
              <AmountDisplay amount={account.currentBalance} />
            </div>
            <div>
              <label className="block text-xs text-gray-500 uppercase">Number of Cards</label>
              <p className="font-mono text-lg">{account.numberOfCards}</p>
            </div>
            <div>
              <label className="block text-xs text-gray-500 uppercase">Current Cycle Credit</label>
              <AmountDisplay amount={account.currentCycleCredit} colored />
            </div>
            <div>
              <label className="block text-xs text-gray-500 uppercase">Current Cycle Debit</label>
              <AmountDisplay amount={account.currentCycleDebit} colored />
            </div>
          </div>
        </CardPanel>

        {/* Customer Details */}
        <CardPanel title="Customer Details">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs text-gray-500 uppercase">Customer ID</label>
              <p className="font-mono">{account.customerId}</p>
            </div>
            <div>
              <label className="block text-xs text-gray-500 uppercase">Customer Name</label>
              <p className="font-medium">{account.customerName}</p>
            </div>
          </div>
          <div className="mt-4">
            <Button
              variant="secondary"
              size="sm"
              onClick={() => navigate(`/transactions?accountId=${account.accountId}`)}
            >
              View Transactions
            </Button>
            <Button
              variant="secondary"
              size="sm"
              className="ml-2"
              onClick={() => navigate(`/cards?accountId=${account.accountId}`)}
            >
              View Cards
            </Button>
          </div>
        </CardPanel>
      </div>
    </div>
  );
};
