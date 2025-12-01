import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { accountService } from '@/services/apiService';
import { QUERY_KEYS } from '@/config/constants';
import { PageHeader, CardPanel, FormField, Button, Alert } from '@/components/ui';

export const AccountEditPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [isSaving, setIsSaving] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  const { data: account, isLoading } = useQuery({
    queryKey: [QUERY_KEYS.ACCOUNT, id],
    queryFn: () => accountService.getAccountById(Number(id)),
    enabled: !!id,
  });

  const [formData, setFormData] = useState({
    activeStatus: 'Y',
    creditLimit: 0,
    cashCreditLimit: 0,
    currentBalance: 0,
    currentCycleCredit: 0,
    currentCycleDebit: 0,
    expirationYear: '',
    expirationMonth: '',
    expirationDay: '',
  });

  useEffect(() => {
    if (account) {
      const expDate = new Date(account.expirationDate);
      setFormData({
        activeStatus: account.activeStatus,
        creditLimit: account.creditLimit,
        cashCreditLimit: account.cashCreditLimit,
        currentBalance: account.currentBalance,
        currentCycleCredit: account.currentCycleCredit,
        currentCycleDebit: account.currentCycleDebit,
        expirationYear: expDate.getFullYear().toString(),
        expirationMonth: (expDate.getMonth() + 1).toString().padStart(2, '0'),
        expirationDay: expDate.getDate().toString().padStart(2, '0'),
      });
    }
  }, [account]);

  const handleChange = (field: string, value: string | number) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handleSave = async () => {
    setIsSaving(true);
    setMessage(null);
    
    try {
      // In a real app, this would call an API to update the account
      // await accountService.updateAccount(Number(id), formData);
      
      // Simulate API call
      await new Promise((resolve) => setTimeout(resolve, 1000));
      
      setMessage({ type: 'success', text: 'Account updated successfully!' });
      setTimeout(() => navigate(`/accounts/${id}`), 1500);
    } catch (error) {
      setMessage({ type: 'error', text: 'Failed to update account. Please try again.' });
    } finally {
      setIsSaving(false);
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        <span className="ml-3 text-gray-600">Loading account...</span>
      </div>
    );
  }

  if (!account) {
    return (
      <div className="bg-red-50 p-4 rounded-lg">
        <p className="text-red-800">Account not found.</p>
        <Button variant="secondary" onClick={() => navigate('/accounts')} className="mt-4">
          Back to Accounts
        </Button>
      </div>
    );
  }

  return (
    <div className="pb-16">
      <PageHeader
        title="Update Account"
        subtitle={`Account Number: ${account.accountId}`}
        actions={
          <div className="flex space-x-3">
            <Button variant="secondary" onClick={() => navigate(`/accounts/${id}`)}>
              F12=Cancel
            </Button>
            <Button onClick={handleSave} isLoading={isSaving}>
              F5=Save
            </Button>
          </div>
        }
      />

      {message && (
        <Alert type={message.type} message={message.text} onClose={() => setMessage(null)} />
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Account Settings */}
        <CardPanel title="Account Settings">
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Account Number">
              <input
                type="text"
                value={account.accountId}
                disabled
                className="w-full px-3 py-2 bg-gray-100 border border-gray-300 rounded-md text-gray-500"
              />
            </FormField>
            <FormField label="Active Y/N" required>
              <select
                value={formData.activeStatus}
                onChange={(e) => handleChange('activeStatus', e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500"
              >
                <option value="Y">Y - Active</option>
                <option value="N">N - Inactive</option>
              </select>
            </FormField>
          </div>

          <div className="grid grid-cols-3 gap-2 mt-4">
            <FormField label="Expiry Year">
              <input
                type="text"
                value={formData.expirationYear}
                onChange={(e) => handleChange('expirationYear', e.target.value)}
                maxLength={4}
                className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 font-mono"
              />
            </FormField>
            <FormField label="Month">
              <input
                type="text"
                value={formData.expirationMonth}
                onChange={(e) => handleChange('expirationMonth', e.target.value)}
                maxLength={2}
                className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 font-mono"
              />
            </FormField>
            <FormField label="Day">
              <input
                type="text"
                value={formData.expirationDay}
                onChange={(e) => handleChange('expirationDay', e.target.value)}
                maxLength={2}
                className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 font-mono"
              />
            </FormField>
          </div>
        </CardPanel>

        {/* Credit Limits */}
        <CardPanel title="Credit Limits">
          <FormField label="Credit Limit" required>
            <input
              type="number"
              value={formData.creditLimit}
              onChange={(e) => handleChange('creditLimit', Number(e.target.value))}
              className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 font-mono"
            />
          </FormField>
          <FormField label="Cash Credit Limit" required>
            <input
              type="number"
              value={formData.cashCreditLimit}
              onChange={(e) => handleChange('cashCreditLimit', Number(e.target.value))}
              className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 font-mono"
            />
          </FormField>
        </CardPanel>

        {/* Balance Information */}
        <CardPanel title="Balance Information">
          <FormField label="Current Balance">
            <input
              type="number"
              value={formData.currentBalance}
              onChange={(e) => handleChange('currentBalance', Number(e.target.value))}
              className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 font-mono"
            />
          </FormField>
          <FormField label="Current Cycle Credit">
            <input
              type="number"
              value={formData.currentCycleCredit}
              onChange={(e) => handleChange('currentCycleCredit', Number(e.target.value))}
              className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 font-mono"
            />
          </FormField>
          <FormField label="Current Cycle Debit">
            <input
              type="number"
              value={formData.currentCycleDebit}
              onChange={(e) => handleChange('currentCycleDebit', Number(e.target.value))}
              className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 font-mono"
            />
          </FormField>
        </CardPanel>

        {/* Customer Details (Read-only) */}
        <CardPanel title="Customer Details">
          <FormField label="Customer ID">
            <input
              type="text"
              value={account.customerId}
              disabled
              className="w-full px-3 py-2 bg-gray-100 border border-gray-300 rounded-md text-gray-500 font-mono"
            />
          </FormField>
          <FormField label="Customer Name">
            <input
              type="text"
              value={account.customerName}
              disabled
              className="w-full px-3 py-2 bg-gray-100 border border-gray-300 rounded-md text-gray-500"
            />
          </FormField>
        </CardPanel>
      </div>
    </div>
  );
};
