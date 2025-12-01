import React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { PageHeader, CardPanel, AmountDisplay, Button } from '@/components/ui';

// Mock data for demo
const mockTransaction = {
  transactionId: 'TRN-2024-000001',
  accountId: 1,
  cardNumber: '4111111111111111',
  maskedCardNumber: '4111-****-****-1111',
  transactionType: 'PURCHASE',
  transactionTypeDescription: 'Purchase',
  categoryCode: 5411,
  categoryDescription: 'Grocery Stores',
  transactionSource: 'POS',
  description: 'WHOLE FOODS MARKET #10847',
  amount: -125.50,
  merchantId: 'MERCH001',
  merchantName: 'Whole Foods Market',
  merchantCity: 'San Francisco',
  merchantZip: '94102',
  transactionDate: '2024-01-15',
  processedDate: '2024-01-15',
  processedFlag: 'Y',
  isProcessed: true,
};

export const TransactionViewPage: React.FC = () => {
  const { transactionId } = useParams<{ transactionId: string }>();
  const navigate = useNavigate();

  const transaction = mockTransaction;
  const isLoading = false;

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        <span className="ml-3 text-gray-600">Loading transaction...</span>
      </div>
    );
  }

  return (
    <div className="pb-16">
      <PageHeader
        title="View Transaction"
        subtitle={`Transaction ID: ${transactionId}`}
        actions={
          <div className="flex space-x-3">
            <Button variant="secondary" onClick={() => navigate('/transactions')}>
              F3=Back
            </Button>
            <Button variant="secondary" onClick={() => {}}>
              F4=Clear
            </Button>
            <Button onClick={() => navigate('/transactions')}>
              F5=Browse Tran.
            </Button>
          </div>
        }
      />

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Transaction Details */}
        <CardPanel title="Transaction Details">
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs text-gray-500 uppercase">Transaction ID</label>
                <p className="font-mono text-sm bg-gray-100 px-3 py-2 rounded">{transaction.transactionId}</p>
              </div>
              <div>
                <label className="block text-xs text-gray-500 uppercase">Card Number</label>
                <p className="font-mono text-sm bg-gray-100 px-3 py-2 rounded">{transaction.maskedCardNumber}</p>
              </div>
            </div>

            <div className="grid grid-cols-3 gap-4">
              <div>
                <label className="block text-xs text-gray-500 uppercase">Type CD</label>
                <p className="font-mono text-sm bg-gray-100 px-3 py-2 rounded">{transaction.transactionType}</p>
              </div>
              <div>
                <label className="block text-xs text-gray-500 uppercase">Category CD</label>
                <p className="font-mono text-sm bg-gray-100 px-3 py-2 rounded">{transaction.categoryCode}</p>
              </div>
              <div>
                <label className="block text-xs text-gray-500 uppercase">Source</label>
                <p className="font-mono text-sm bg-gray-100 px-3 py-2 rounded">{transaction.transactionSource}</p>
              </div>
            </div>

            <div>
              <label className="block text-xs text-gray-500 uppercase">Description</label>
              <p className="text-sm bg-gray-100 px-3 py-2 rounded">{transaction.description}</p>
            </div>

            <div className="grid grid-cols-3 gap-4">
              <div>
                <label className="block text-xs text-gray-500 uppercase">Amount</label>
                <div className="bg-gray-100 px-3 py-2 rounded">
                  <AmountDisplay amount={transaction.amount} colored />
                </div>
              </div>
              <div>
                <label className="block text-xs text-gray-500 uppercase">Orig Date</label>
                <p className="font-mono text-sm bg-gray-100 px-3 py-2 rounded">
                  {new Date(transaction.transactionDate).toLocaleDateString()}
                </p>
              </div>
              <div>
                <label className="block text-xs text-gray-500 uppercase">Proc Date</label>
                <p className="font-mono text-sm bg-gray-100 px-3 py-2 rounded">
                  {new Date(transaction.processedDate).toLocaleDateString()}
                </p>
              </div>
            </div>
          </div>
        </CardPanel>

        {/* Merchant Information */}
        <CardPanel title="Merchant Information">
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs text-gray-500 uppercase">Merchant ID</label>
                <p className="font-mono text-sm bg-gray-100 px-3 py-2 rounded">{transaction.merchantId || '-'}</p>
              </div>
              <div>
                <label className="block text-xs text-gray-500 uppercase">Merchant Name</label>
                <p className="text-sm bg-gray-100 px-3 py-2 rounded">{transaction.merchantName || '-'}</p>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs text-gray-500 uppercase">Merchant City</label>
                <p className="text-sm bg-gray-100 px-3 py-2 rounded">{transaction.merchantCity || '-'}</p>
              </div>
              <div>
                <label className="block text-xs text-gray-500 uppercase">Merchant Zip</label>
                <p className="font-mono text-sm bg-gray-100 px-3 py-2 rounded">{transaction.merchantZip || '-'}</p>
              </div>
            </div>
          </div>
        </CardPanel>

        {/* Category Information */}
        <CardPanel title="Category Information">
          <div className="space-y-4">
            <div>
              <label className="block text-xs text-gray-500 uppercase">Transaction Type</label>
              <p className="text-sm bg-gray-100 px-3 py-2 rounded">{transaction.transactionTypeDescription}</p>
            </div>
            <div>
              <label className="block text-xs text-gray-500 uppercase">Category</label>
              <p className="text-sm bg-gray-100 px-3 py-2 rounded">{transaction.categoryDescription}</p>
            </div>
          </div>
        </CardPanel>
      </div>
    </div>
  );
};
