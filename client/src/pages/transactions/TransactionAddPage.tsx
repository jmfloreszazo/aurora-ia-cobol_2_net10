import React, { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { PageHeader, CardPanel, FormField, Button, Alert, AmountDisplay } from '@/components/ui';
import { accountService, cardService, QUERY_KEYS } from '@/services/apiService';

interface TransactionForm {
  accountId: string;
  cardNumber: string;
  transactionType: string;
  categoryCode: string;
  transactionSource: string;
  description: string;
  amount: string;
  transactionDate: string;
  merchantId: string;
  merchantName: string;
  merchantCity: string;
  merchantZip: string;
}

const initialForm: TransactionForm = {
  accountId: '',
  cardNumber: '',
  transactionType: 'PURCHASE',
  categoryCode: '5411',
  transactionSource: 'POS',
  description: '',
  amount: '',
  transactionDate: new Date().toISOString().split('T')[0],
  merchantId: '',
  merchantName: '',
  merchantCity: '',
  merchantZip: '',
};

const transactionTypes = [
  { value: 'PURCHASE', label: 'Purchase' },
  { value: 'RETURN', label: 'Return' },
  { value: 'PAYMENT', label: 'Payment' },
  { value: 'CASH_ADVANCE', label: 'Cash Advance' },
  { value: 'BALANCE_TRANSFER', label: 'Balance Transfer' },
  { value: 'FEE', label: 'Fee' },
  { value: 'INTEREST', label: 'Interest Charge' },
  { value: 'ADJUSTMENT', label: 'Adjustment' },
];

const transactionSources = [
  { value: 'POS', label: 'Point of Sale' },
  { value: 'ONLINE', label: 'Online' },
  { value: 'ATM', label: 'ATM' },
  { value: 'RECURRING', label: 'Recurring' },
  { value: 'MANUAL', label: 'Manual Entry' },
];

const categoryOptions = [
  { value: '5411', label: '5411 - Grocery Stores' },
  { value: '5541', label: '5541 - Service Stations' },
  { value: '5812', label: '5812 - Restaurants' },
  { value: '5999', label: '5999 - Miscellaneous Retail' },
  { value: '6011', label: '6011 - ATM Withdrawals' },
  { value: '7011', label: '7011 - Hotels' },
  { value: '4111', label: '4111 - Transportation' },
  { value: '5311', label: '5311 - Department Stores' },
];

export const TransactionAddPage: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const preselectedAccount = searchParams.get('accountId');
  
  const [form, setForm] = useState<TransactionForm>({
    ...initialForm,
    accountId: preselectedAccount || '',
  });
  const [errors, setErrors] = useState<Partial<TransactionForm>>({});
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showConfirmation, setShowConfirmation] = useState(false);

  // Fetch accounts for dropdown
  const { data: accountsData } = useQuery({
    queryKey: [QUERY_KEYS.ACCOUNTS],
    queryFn: () => accountService.getAccounts(1, 100),
  });

  // Fetch cards for selected account
  const { data: cardsData } = useQuery({
    queryKey: [QUERY_KEYS.CARDS, form.accountId],
    queryFn: () => cardService.getCardsByAccount(Number(form.accountId), 1, 100),
    enabled: !!form.accountId,
  });

  const handleChange = (field: keyof TransactionForm, value: string) => {
    setForm(prev => ({ ...prev, [field]: value }));
    setErrors(prev => ({ ...prev, [field]: undefined }));
    
    // Reset card when account changes
    if (field === 'accountId') {
      setForm(prev => ({ ...prev, cardNumber: '' }));
    }
  };

  const validate = (): boolean => {
    const newErrors: Partial<TransactionForm> = {};

    if (!form.accountId) newErrors.accountId = 'Account is required';
    if (!form.cardNumber) newErrors.cardNumber = 'Card is required';
    if (!form.description.trim()) newErrors.description = 'Description is required';
    if (!form.amount) {
      newErrors.amount = 'Amount is required';
    } else {
      const amount = parseFloat(form.amount);
      if (isNaN(amount) || amount === 0) {
        newErrors.amount = 'Enter a valid non-zero amount';
      }
    }
    if (!form.transactionDate) newErrors.transactionDate = 'Date is required';

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitError(null);

    if (!validate()) return;

    setShowConfirmation(true);
  };

  const handleConfirm = async () => {
    setIsSubmitting(true);
    
    try {
      // TODO: API call to create transaction
      // await transactionService.createTransaction({...form});
      
      // Simulate API delay
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      navigate(`/accounts/${form.accountId}`);
    } catch (error) {
      setSubmitError('Failed to create transaction. Please try again.');
      setShowConfirmation(false);
    } finally {
      setIsSubmitting(false);
    }
  };

  const accounts = accountsData?.items || [];
  const cards = cardsData?.items || [];
  const parsedAmount = parseFloat(form.amount) || 0;

  // Confirmation Dialog
  if (showConfirmation) {
    return (
      <div className="pb-16">
        <PageHeader
          title="Confirm Transaction"
          subtitle="Please review the transaction details"
        />
        
        <CardPanel title="Transaction Summary">
          <div className="space-y-6">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs text-gray-500 uppercase">Card</label>
                <p className="font-mono text-sm">{form.cardNumber}</p>
              </div>
              <div>
                <label className="block text-xs text-gray-500 uppercase">Type</label>
                <p className="text-sm">{transactionTypes.find(t => t.value === form.transactionType)?.label}</p>
              </div>
            </div>
            
            <div>
              <label className="block text-xs text-gray-500 uppercase">Description</label>
              <p className="text-sm">{form.description}</p>
            </div>
            
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs text-gray-500 uppercase">Amount</label>
                <div className="text-lg">
                  <AmountDisplay amount={parsedAmount} colored />
                </div>
              </div>
              <div>
                <label className="block text-xs text-gray-500 uppercase">Date</label>
                <p className="text-sm">{new Date(form.transactionDate).toLocaleDateString()}</p>
              </div>
            </div>

            {form.merchantName && (
              <div>
                <label className="block text-xs text-gray-500 uppercase">Merchant</label>
                <p className="text-sm">
                  {form.merchantName}
                  {form.merchantCity && `, ${form.merchantCity}`}
                  {form.merchantZip && ` ${form.merchantZip}`}
                </p>
              </div>
            )}

            <div className="border-t pt-4 flex justify-end space-x-3">
              <Button 
                variant="secondary" 
                onClick={() => setShowConfirmation(false)}
                disabled={isSubmitting}
              >
                F3=Cancel
              </Button>
              <Button 
                onClick={handleConfirm}
                disabled={isSubmitting}
              >
                {isSubmitting ? 'Processing...' : 'F5=Confirm'}
              </Button>
            </div>
          </div>
        </CardPanel>
      </div>
    );
  }

  return (
    <div className="pb-16">
      <PageHeader
        title="Add Transaction"
        subtitle="Enter new transaction details"
        actions={
          <div className="flex space-x-3">
            <Button variant="secondary" onClick={() => navigate('/transactions')}>
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
          {/* Account & Card Selection */}
          <CardPanel title="Account & Card">
            <div className="space-y-4">
              <div>
                <FormField
                  label="Account"
                  name="accountId"
                  required
                  error={errors.accountId}
                >
                  <select
                    className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    value={form.accountId}
                    onChange={(e) => handleChange('accountId', e.target.value)}
                  >
                    <option value="">Select Account</option>
                    {accounts.map((account: any) => (
                      <option key={account.accountId} value={account.accountId}>
                        {account.accountId} - Balance: ${account.currentBalance?.toFixed(2) || '0.00'}
                      </option>
                    ))}
                  </select>
                </FormField>
              </div>

              <div>
                <FormField
                  label="Card"
                  name="cardNumber"
                  required
                  error={errors.cardNumber}
                >
                  <select
                    className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    value={form.cardNumber}
                    onChange={(e) => handleChange('cardNumber', e.target.value)}
                    disabled={!form.accountId}
                  >
                    <option value="">Select Card</option>
                    {cards.map((card: any) => (
                      <option key={card.cardNumber} value={card.cardNumber}>
                        {card.cardNumber.replace(/(\d{4})(\d{4})(\d{4})(\d{4})/, '$1-****-****-$4')}
                      </option>
                    ))}
                  </select>
                </FormField>
              </div>
            </div>
          </CardPanel>

          {/* Transaction Type & Category */}
          <CardPanel title="Transaction Type">
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <FormField label="Type" name="transactionType" required>
                  <select
                    className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    value={form.transactionType}
                    onChange={(e) => handleChange('transactionType', e.target.value)}
                  >
                    {transactionTypes.map((type) => (
                      <option key={type.value} value={type.value}>
                        {type.label}
                      </option>
                    ))}
                  </select>
                </FormField>

                <FormField label="Source" name="transactionSource" required>
                  <select
                    className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    value={form.transactionSource}
                    onChange={(e) => handleChange('transactionSource', e.target.value)}
                  >
                    {transactionSources.map((source) => (
                      <option key={source.value} value={source.value}>
                        {source.label}
                      </option>
                    ))}
                  </select>
                </FormField>
              </div>

              <FormField label="Category" name="categoryCode" required>
                <select
                  className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  value={form.categoryCode}
                  onChange={(e) => handleChange('categoryCode', e.target.value)}
                >
                  {categoryOptions.map((cat) => (
                    <option key={cat.value} value={cat.value}>
                      {cat.label}
                    </option>
                  ))}
                </select>
              </FormField>
            </div>
          </CardPanel>

          {/* Transaction Details */}
          <CardPanel title="Transaction Details">
            <div className="space-y-4">
              <FormField
                label="Description"
                name="description"
                required
                error={errors.description}
              >
                <input
                  type="text"
                  className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  value={form.description}
                  onChange={(e) => handleChange('description', e.target.value)}
                  maxLength={50}
                  placeholder="Transaction description"
                />
              </FormField>

              <div className="grid grid-cols-2 gap-4">
                <FormField
                  label="Amount"
                  name="amount"
                  required
                  error={errors.amount}
                >
                  <input
                    type="number"
                    step="0.01"
                    className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    value={form.amount}
                    onChange={(e) => handleChange('amount', e.target.value)}
                    placeholder="-100.00 (negative for charges)"
                  />
                </FormField>

                <FormField
                  label="Date"
                  name="transactionDate"
                  required
                  error={errors.transactionDate}
                >
                  <input
                    type="date"
                    className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    value={form.transactionDate}
                    onChange={(e) => handleChange('transactionDate', e.target.value)}
                  />
                </FormField>
              </div>
            </div>
          </CardPanel>

          {/* Merchant Information */}
          <CardPanel title="Merchant Information (Optional)">
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <FormField label="Merchant ID" name="merchantId">
                  <input
                    type="text"
                    className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    value={form.merchantId}
                    onChange={(e) => handleChange('merchantId', e.target.value)}
                    maxLength={10}
                  />
                </FormField>

                <FormField label="Merchant Name" name="merchantName">
                  <input
                    type="text"
                    className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    value={form.merchantName}
                    onChange={(e) => handleChange('merchantName', e.target.value)}
                    maxLength={30}
                  />
                </FormField>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <FormField label="City" name="merchantCity">
                  <input
                    type="text"
                    className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    value={form.merchantCity}
                    onChange={(e) => handleChange('merchantCity', e.target.value)}
                    maxLength={30}
                  />
                </FormField>

                <FormField label="Zip Code" name="merchantZip">
                  <input
                    type="text"
                    className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    value={form.merchantZip}
                    onChange={(e) => handleChange('merchantZip', e.target.value)}
                    maxLength={10}
                  />
                </FormField>
              </div>
            </div>
          </CardPanel>
        </div>

        {/* Submit Button */}
        <div className="mt-6 flex justify-end">
          <Button type="submit" size="lg">
            F5=Submit Transaction
          </Button>
        </div>
      </form>
    </div>
  );
};
