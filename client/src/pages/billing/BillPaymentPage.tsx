import React, { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { PageHeader, CardPanel, FormField, Button, Alert, AmountDisplay } from '@/components/ui';
import { accountService, QUERY_KEYS } from '@/services/apiService';

interface PaymentForm {
  accountId: string;
  paymentAmount: string;
  paymentDate: string;
  confirmationCode: string;
}

const initialForm: PaymentForm = {
  accountId: '',
  paymentAmount: '',
  paymentDate: new Date().toISOString().split('T')[0],
  confirmationCode: '',
};

// Mock account details
const mockAccountDetails = {
  accountId: 1,
  customerId: 1,
  currentBalance: 5234.56,
  creditLimit: 10000.00,
  availableCredit: 4765.44,
  minimumPayment: 156.25,
  dueDate: '2024-02-15',
  lastPaymentDate: '2024-01-10',
  lastPaymentAmount: 500.00,
  status: 'Active',
};

type PaymentStep = 'select' | 'confirm' | 'success';

export const BillPaymentPage: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const preselectedAccount = searchParams.get('accountId');

  const [step, setStep] = useState<PaymentStep>('select');
  const [form, setForm] = useState<PaymentForm>({
    ...initialForm,
    accountId: preselectedAccount || '',
  });
  const [errors, setErrors] = useState<Partial<PaymentForm>>({});
  const [isProcessing, setIsProcessing] = useState(false);
  const [paymentResult, setPaymentResult] = useState<{ confirmationNumber: string; timestamp: string } | null>(null);

  // Fetch accounts for dropdown
  const { data: accountsData } = useQuery({
    queryKey: [QUERY_KEYS.ACCOUNTS],
    queryFn: () => accountService.getAccounts(1, 100),
  });

  const accounts = accountsData?.items || [];
  const selectedAccountDetails = mockAccountDetails;

  const handleChange = (field: keyof PaymentForm, value: string) => {
    setForm(prev => ({ ...prev, [field]: value }));
    setErrors(prev => ({ ...prev, [field]: undefined }));
  };

  const validate = (): boolean => {
    const newErrors: Partial<PaymentForm> = {};

    if (!form.accountId) {
      newErrors.accountId = 'Please select an account';
    }

    if (!form.paymentAmount) {
      newErrors.paymentAmount = 'Payment amount is required';
    } else {
      const amount = parseFloat(form.paymentAmount);
      if (isNaN(amount) || amount <= 0) {
        newErrors.paymentAmount = 'Enter a valid positive amount';
      } else if (amount > selectedAccountDetails.currentBalance) {
        newErrors.paymentAmount = 'Amount exceeds current balance';
      }
    }

    if (!form.paymentDate) {
      newErrors.paymentDate = 'Payment date is required';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleProceed = (e: React.FormEvent) => {
    e.preventDefault();
    if (validate()) {
      setStep('confirm');
    }
  };

  const handleConfirm = async () => {
    setIsProcessing(true);
    
    try {
      // TODO: API call to process payment
      // await paymentService.makePayment({...form});
      
      // Simulate API delay
      await new Promise(resolve => setTimeout(resolve, 2000));
      
      setPaymentResult({
        confirmationNumber: `CONF-${Date.now().toString(36).toUpperCase()}`,
        timestamp: new Date().toISOString(),
      });
      setStep('success');
    } catch (error) {
      setStep('select');
      setErrors({ paymentAmount: 'Payment failed. Please try again.' });
    } finally {
      setIsProcessing(false);
    }
  };

  const handleNewPayment = () => {
    setForm(initialForm);
    setPaymentResult(null);
    setStep('select');
  };

  const paymentAmount = parseFloat(form.paymentAmount) || 0;

  // Success Step
  if (step === 'success' && paymentResult) {
    return (
      <div className="pb-16">
        <PageHeader
          title="Payment Successful"
          subtitle="Your payment has been processed"
        />

        <div className="max-w-2xl mx-auto">
          <Alert type="success">
            <div className="font-semibold mb-2">Payment Confirmed!</div>
            <p>Your payment has been successfully processed and applied to your account.</p>
          </Alert>

          <CardPanel title="Payment Receipt" className="mt-6">
            <div className="space-y-4">
              <div className="text-center py-4 border-b">
                <p className="text-xs text-gray-500 uppercase">Confirmation Number</p>
                <p className="text-2xl font-mono font-bold text-green-600">{paymentResult.confirmationNumber}</p>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs text-gray-500 uppercase mb-1">Account</label>
                  <p className="font-mono text-sm">{form.accountId}</p>
                </div>
                <div>
                  <label className="block text-xs text-gray-500 uppercase mb-1">Payment Date</label>
                  <p className="text-sm">{new Date(form.paymentDate).toLocaleDateString()}</p>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs text-gray-500 uppercase mb-1">Amount Paid</label>
                  <div className="text-lg">
                    <AmountDisplay amount={paymentAmount} />
                  </div>
                </div>
                <div>
                  <label className="block text-xs text-gray-500 uppercase mb-1">Transaction Time</label>
                  <p className="text-sm">{new Date(paymentResult.timestamp).toLocaleString()}</p>
                </div>
              </div>

              <div className="border-t pt-4 flex justify-end space-x-3">
                <Button variant="secondary" onClick={() => window.print()}>
                  Print Receipt
                </Button>
                <Button variant="secondary" onClick={handleNewPayment}>
                  Make Another Payment
                </Button>
                <Button onClick={() => navigate('/accounts')}>
                  Return to Accounts
                </Button>
              </div>
            </div>
          </CardPanel>
        </div>
      </div>
    );
  }

  // Confirmation Step
  if (step === 'confirm') {
    return (
      <div className="pb-16">
        <PageHeader
          title="Confirm Payment"
          subtitle="Review and confirm your payment"
        />

        <div className="max-w-2xl mx-auto">
          <Alert type="warning">
            Please review the payment details below before confirming. This action cannot be undone.
          </Alert>

          <CardPanel title="Payment Summary" className="mt-6">
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs text-gray-500 uppercase mb-1">Account</label>
                  <p className="font-mono text-sm">{form.accountId}</p>
                </div>
                <div>
                  <label className="block text-xs text-gray-500 uppercase mb-1">Current Balance</label>
                  <div>
                    <AmountDisplay amount={-selectedAccountDetails.currentBalance} colored />
                  </div>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs text-gray-500 uppercase mb-1">Payment Amount</label>
                  <div className="text-lg font-bold text-green-600">
                    <AmountDisplay amount={paymentAmount} />
                  </div>
                </div>
                <div>
                  <label className="block text-xs text-gray-500 uppercase mb-1">Payment Date</label>
                  <p className="text-sm">{new Date(form.paymentDate).toLocaleDateString()}</p>
                </div>
              </div>

              <div className="bg-gray-50 p-4 rounded-lg">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-xs text-gray-500 uppercase mb-1">Balance After Payment</label>
                    <div className="text-lg font-bold">
                      <AmountDisplay 
                        amount={-(selectedAccountDetails.currentBalance - paymentAmount)} 
                        colored 
                      />
                    </div>
                  </div>
                  <div>
                    <label className="block text-xs text-gray-500 uppercase mb-1">New Available Credit</label>
                    <div className="text-lg font-bold text-green-600">
                      ${(selectedAccountDetails.availableCredit + paymentAmount).toLocaleString('en-US', { minimumFractionDigits: 2 })}
                    </div>
                  </div>
                </div>
              </div>

              <div className="border-t pt-4 flex justify-end space-x-3">
                <Button 
                  variant="secondary" 
                  onClick={() => setStep('select')}
                  disabled={isProcessing}
                >
                  F3=Back
                </Button>
                <Button 
                  onClick={handleConfirm}
                  disabled={isProcessing}
                >
                  {isProcessing ? (
                    <>
                      <span className="animate-spin inline-block w-4 h-4 border-2 border-white border-t-transparent rounded-full mr-2"></span>
                      Processing...
                    </>
                  ) : (
                    'F5=Confirm Payment'
                  )}
                </Button>
              </div>
            </div>
          </CardPanel>
        </div>
      </div>
    );
  }

  // Selection Step (default)
  return (
    <div className="pb-16">
      <PageHeader
        title="Bill Payment"
        subtitle="Make a payment to your account"
        actions={
          <div className="flex space-x-3">
            <Button variant="secondary" onClick={() => navigate('/accounts')}>
              F3=Exit
            </Button>
            <Button variant="secondary" onClick={() => setForm(initialForm)}>
              F4=Clear
            </Button>
          </div>
        }
      />

      <form onSubmit={handleProceed}>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Account Selection */}
          <CardPanel title="Select Account">
            <div className="space-y-4">
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
                      Account {account.accountId} - Balance: ${account.currentBalance?.toFixed(2) || '0.00'}
                    </option>
                  ))}
                </select>
              </FormField>

              {form.accountId && (
                <div className="bg-gray-50 p-4 rounded-lg space-y-3">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-xs text-gray-500 uppercase">Current Balance</label>
                      <p className="text-lg font-bold text-red-600">
                        ${selectedAccountDetails.currentBalance.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                      </p>
                    </div>
                    <div>
                      <label className="block text-xs text-gray-500 uppercase">Minimum Payment</label>
                      <p className="text-lg font-bold">
                        ${selectedAccountDetails.minimumPayment.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                      </p>
                    </div>
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-xs text-gray-500 uppercase">Due Date</label>
                      <p className="text-sm">{new Date(selectedAccountDetails.dueDate).toLocaleDateString()}</p>
                    </div>
                    <div>
                      <label className="block text-xs text-gray-500 uppercase">Available Credit</label>
                      <p className="text-sm text-green-600">
                        ${selectedAccountDetails.availableCredit.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                      </p>
                    </div>
                  </div>
                </div>
              )}
            </div>
          </CardPanel>

          {/* Payment Details */}
          <CardPanel title="Payment Details">
            <div className="space-y-4">
              <FormField
                label="Payment Amount"
                name="paymentAmount"
                required
                error={errors.paymentAmount}
              >
                <div className="relative">
                  <span className="absolute left-3 top-2 text-gray-500">$</span>
                  <input
                    type="number"
                    step="0.01"
                    className="w-full pl-7 pr-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    value={form.paymentAmount}
                    onChange={(e) => handleChange('paymentAmount', e.target.value)}
                    placeholder="0.00"
                  />
                </div>
              </FormField>

              {form.accountId && (
                <div className="flex flex-wrap gap-2">
                  <button
                    type="button"
                    className="px-3 py-1 text-sm bg-gray-100 hover:bg-gray-200 rounded-md"
                    onClick={() => handleChange('paymentAmount', selectedAccountDetails.minimumPayment.toString())}
                  >
                    Minimum (${selectedAccountDetails.minimumPayment.toFixed(2)})
                  </button>
                  <button
                    type="button"
                    className="px-3 py-1 text-sm bg-gray-100 hover:bg-gray-200 rounded-md"
                    onClick={() => handleChange('paymentAmount', selectedAccountDetails.currentBalance.toString())}
                  >
                    Full Balance (${selectedAccountDetails.currentBalance.toFixed(2)})
                  </button>
                  <button
                    type="button"
                    className="px-3 py-1 text-sm bg-gray-100 hover:bg-gray-200 rounded-md"
                    onClick={() => handleChange('paymentAmount', (selectedAccountDetails.currentBalance / 2).toFixed(2))}
                  >
                    Half Balance
                  </button>
                </div>
              )}

              <FormField
                label="Payment Date"
                name="paymentDate"
                required
                error={errors.paymentDate}
              >
                <input
                  type="date"
                  className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  value={form.paymentDate}
                  onChange={(e) => handleChange('paymentDate', e.target.value)}
                  min={new Date().toISOString().split('T')[0]}
                />
              </FormField>

              <div className="pt-4 border-t">
                <Button type="submit" className="w-full" size="lg">
                  F5=Proceed to Payment
                </Button>
              </div>
            </div>
          </CardPanel>
        </div>
      </form>

      {/* Last Payment Info */}
      {form.accountId && selectedAccountDetails.lastPaymentDate && (
        <CardPanel title="Last Payment" className="mt-6">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs text-gray-500 uppercase">Last Payment Date</label>
              <p className="text-sm">{new Date(selectedAccountDetails.lastPaymentDate).toLocaleDateString()}</p>
            </div>
            <div>
              <label className="block text-xs text-gray-500 uppercase">Last Payment Amount</label>
              <p className="text-sm">${selectedAccountDetails.lastPaymentAmount.toLocaleString('en-US', { minimumFractionDigits: 2 })}</p>
            </div>
          </div>
        </CardPanel>
      )}
    </div>
  );
};
