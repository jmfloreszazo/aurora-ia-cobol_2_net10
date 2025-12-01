import React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { PageHeader, CardPanel, StatusBadge, Button } from '@/components/ui';

// Mock data for demo - in production this would come from API
const mockCard = {
  cardNumber: '4111111111111111',
  maskedCardNumber: '4111-****-****-1111',
  accountId: 1,
  cardType: 'VISA',
  embossedName: 'JOHN DOE',
  expirationDate: '2025-12-31',
  activeStatus: 'Y',
  isActive: true,
};

export const CardViewPage: React.FC = () => {
  const { cardNumber } = useParams<{ cardNumber: string }>();
  const navigate = useNavigate();

  // In production, use useQuery to fetch card details
  const card = mockCard;
  const isLoading = false;

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        <span className="ml-3 text-gray-600">Loading card...</span>
      </div>
    );
  }

  const expDate = new Date(card.expirationDate);

  return (
    <div className="pb-16">
      <PageHeader
        title="View Credit Card Detail"
        subtitle={`Card Number: ${card.maskedCardNumber}`}
        actions={
          <div className="flex space-x-3">
            <Button variant="secondary" onClick={() => navigate('/cards')}>
              F3=Exit
            </Button>
            <Button onClick={() => navigate(`/cards/${cardNumber}/edit`)}>
              Update Card
            </Button>
          </div>
        }
      />

      <div className="max-w-2xl">
        <CardPanel title="Credit Card Details">
          <div className="space-y-6">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs text-gray-500 uppercase mb-1">Account Number</label>
                <p className="text-lg font-mono bg-gray-100 px-3 py-2 rounded">{card.accountId}</p>
              </div>
              <div>
                <label className="block text-xs text-gray-500 uppercase mb-1">Card Number</label>
                <p className="text-lg font-mono bg-gray-100 px-3 py-2 rounded">{card.maskedCardNumber}</p>
              </div>
            </div>

            <div>
              <label className="block text-xs text-gray-500 uppercase mb-1">Name on Card</label>
              <p className="text-lg font-medium bg-gray-100 px-3 py-2 rounded">{card.embossedName}</p>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs text-gray-500 uppercase mb-1">Card Active Y/N</label>
                <div className="py-2">
                  <StatusBadge active={card.isActive} activeText="Y" inactiveText="N" />
                </div>
              </div>
              <div>
                <label className="block text-xs text-gray-500 uppercase mb-1">Card Type</label>
                <p className="text-lg font-mono bg-gray-100 px-3 py-2 rounded">{card.cardType}</p>
              </div>
            </div>

            <div>
              <label className="block text-xs text-gray-500 uppercase mb-1">Expiry Date</label>
              <p className="text-lg font-mono bg-gray-100 px-3 py-2 rounded">
                {(expDate.getMonth() + 1).toString().padStart(2, '0')}/{expDate.getFullYear()}
              </p>
            </div>
          </div>
        </CardPanel>

        {/* Visual Card Representation */}
        <div className="mt-6">
          <div className="bg-gradient-to-r from-blue-800 to-blue-600 rounded-xl p-6 text-white shadow-lg">
            <div className="flex justify-between items-start mb-8">
              <div className="text-sm opacity-80">Credit Card</div>
              <div className="text-xl font-bold">{card.cardType}</div>
            </div>
            <div className="mb-6">
              <div className="text-xl font-mono tracking-widest">{card.maskedCardNumber}</div>
            </div>
            <div className="flex justify-between items-end">
              <div>
                <div className="text-xs opacity-80 uppercase">Card Holder</div>
                <div className="font-medium">{card.embossedName}</div>
              </div>
              <div className="text-right">
                <div className="text-xs opacity-80 uppercase">Expires</div>
                <div className="font-mono">
                  {(expDate.getMonth() + 1).toString().padStart(2, '0')}/{expDate.getFullYear().toString().slice(-2)}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
