import React, { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { cardService, accountService, customerService } from '@/services/apiService';
import { QUERY_KEYS } from '@/config/constants';
import { DataTable, PageHeader, StatusBadge, Button } from '@/components/ui';
import type { Card, Customer, Account, PagedResult } from '@/types';

export const CardListPage: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [selectedCustomerId, setSelectedCustomerId] = useState<number | null>(null);
  const [selectedAccountId, setSelectedAccountId] = useState<number | null>(
    searchParams.get('accountId') ? Number(searchParams.get('accountId')) : null
  );
  const page = 1;

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

  // Get cards for selected account
  const { data: cardsData, isLoading: cardsLoading } = useQuery<PagedResult<Card>>({
    queryKey: [QUERY_KEYS.CARDS, selectedAccountId],
    queryFn: () => cardService.getCardsByAccount(selectedAccountId!),
    enabled: !!selectedAccountId,
  });

  const cards = cardsData?.items || [];

  const columns = [
    {
      key: 'maskedCardNumber',
      header: 'Card Number',
      render: (card: Card) => <span className="font-mono">{card.maskedCardNumber}</span>,
    },
    { key: 'accountId', header: 'Account #' },
    { key: 'embossedName', header: 'Name on Card' },
    { key: 'cardType', header: 'Type' },
    {
      key: 'expirationDate',
      header: 'Expiry Date',
      render: (card: Card) => {
        const date = new Date(card.expirationDate);
        return <span className="font-mono">{`${(date.getMonth() + 1).toString().padStart(2, '0')}/${date.getFullYear()}`}</span>;
      },
    },
    {
      key: 'isActive',
      header: 'Active',
      render: (card: Card) => <StatusBadge active={card.isActive} activeText="Y" inactiveText="N" />,
    },
  ];

  return (
    <div className="pb-16">
      <PageHeader
        title="List Credit Cards"
        subtitle="View and manage credit cards"
        actions={
          <span className="text-sm text-gray-500">Page {page}</span>
        }
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
              onChange={(e) => {
                setSelectedCustomerId(e.target.value ? Number(e.target.value) : null);
                setSelectedAccountId(null);
              }}
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
              <option value="">-- Select an Account --</option>
              {accounts?.map((account: Account) => (
                <option key={account.accountId} value={account.accountId}>
                  {account.accountId}
                </option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* Cards Table */}
      <div className="bg-white shadow rounded-lg overflow-hidden">
        <div className="bg-gray-800 text-white px-4 py-3 flex items-center justify-between">
          <h3 className="text-sm font-medium">Credit Cards</h3>
          <div className="flex space-x-2 text-xs text-gray-300">
            <span>Select</span>
            <span>Account Number</span>
            <span>Card Number</span>
            <span>Active</span>
          </div>
        </div>
        
        {selectedAccountId ? (
          <DataTable<Card>
            columns={columns}
            data={cards}
            keyExtractor={(card) => card.cardNumber}
            isLoading={cardsLoading}
            emptyMessage="No cards found for this account."
            onRowClick={(card) => navigate(`/cards/${card.cardNumber}`)}
            selectable
            actions={(card) => (
              <div className="flex space-x-2">
                <Button
                  size="sm"
                  onClick={(e) => {
                    e.stopPropagation();
                    navigate(`/cards/${card.cardNumber}`);
                  }}
                >
                  View
                </Button>
                <Button
                  size="sm"
                  variant="secondary"
                  onClick={(e) => {
                    e.stopPropagation();
                    navigate(`/cards/${card.cardNumber}/edit`);
                  }}
                >
                  Update
                </Button>
              </div>
            )}
          />
        ) : (
          <div className="p-8 text-center text-gray-500">
            Please select a customer and account to view cards.
          </div>
        )}
      </div>
    </div>
  );
};
