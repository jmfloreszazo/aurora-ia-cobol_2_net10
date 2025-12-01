import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { PageHeader, CardPanel, FormField, Button, Alert } from '@/components/ui';

export const CardEditPage: React.FC = () => {
  const { cardNumber } = useParams<{ cardNumber: string }>();
  const navigate = useNavigate();
  const [isSaving, setIsSaving] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  const [formData, setFormData] = useState({
    accountId: '1',
    cardNumber: cardNumber || '',
    embossedName: 'JOHN DOE',
    activeStatus: 'Y',
    expirationMonth: '12',
    expirationYear: '2025',
  });

  const handleChange = (field: string, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handleSave = async () => {
    setIsSaving(true);
    setMessage(null);
    
    try {
      // Simulate API call
      await new Promise((resolve) => setTimeout(resolve, 1000));
      
      setMessage({ type: 'success', text: 'Card updated successfully!' });
      setTimeout(() => navigate(`/cards/${cardNumber}`), 1500);
    } catch (error) {
      setMessage({ type: 'error', text: 'Failed to update card. Please try again.' });
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="pb-16">
      <PageHeader
        title="Update Credit Card Details"
        subtitle={`Card Number: ${cardNumber}`}
        actions={
          <div className="flex space-x-3">
            <Button variant="secondary" onClick={() => navigate(`/cards/${cardNumber}`)}>
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

      <div className="max-w-2xl">
        <CardPanel title="Card Details">
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <FormField label="Account Number">
                <input
                  type="text"
                  value={formData.accountId}
                  disabled
                  className="w-full px-3 py-2 bg-gray-100 border border-gray-300 rounded-md text-gray-500 font-mono"
                />
              </FormField>
              <FormField label="Card Number">
                <input
                  type="text"
                  value={formData.cardNumber}
                  disabled
                  className="w-full px-3 py-2 bg-gray-100 border border-gray-300 rounded-md text-gray-500 font-mono"
                />
              </FormField>
            </div>

            <FormField label="Name on Card" required>
              <input
                type="text"
                value={formData.embossedName}
                onChange={(e) => handleChange('embossedName', e.target.value.toUpperCase())}
                maxLength={50}
                className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 uppercase"
              />
            </FormField>

            <FormField label="Card Active Y/N" required>
              <select
                value={formData.activeStatus}
                onChange={(e) => handleChange('activeStatus', e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500"
              >
                <option value="Y">Y - Active</option>
                <option value="N">N - Inactive</option>
              </select>
            </FormField>

            <div className="grid grid-cols-2 gap-4">
              <FormField label="Expiry Month" required>
                <select
                  value={formData.expirationMonth}
                  onChange={(e) => handleChange('expirationMonth', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 font-mono"
                >
                  {Array.from({ length: 12 }, (_, i) => (
                    <option key={i + 1} value={(i + 1).toString().padStart(2, '0')}>
                      {(i + 1).toString().padStart(2, '0')}
                    </option>
                  ))}
                </select>
              </FormField>
              <FormField label="Expiry Year" required>
                <input
                  type="text"
                  value={formData.expirationYear}
                  onChange={(e) => handleChange('expirationYear', e.target.value)}
                  maxLength={4}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 font-mono"
                />
              </FormField>
            </div>
          </div>
        </CardPanel>
      </div>
    </div>
  );
};
