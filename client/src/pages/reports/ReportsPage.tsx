import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { PageHeader, CardPanel, FormField, Button, DataTable, Pagination, AmountDisplay } from '@/components/ui';
import { accountService, QUERY_KEYS } from '@/services/apiService';

type ReportType = 'MONTHLY' | 'YEARLY' | 'CUSTOM';
type ConfirmationType = 'LIST' | 'PRINT' | 'BOTH';

// Mock report data
const mockReportData = [
  { month: 'January', year: 2024, transactionCount: 145, totalAmount: -12500.00, avgAmount: -86.21 },
  { month: 'February', year: 2024, transactionCount: 132, totalAmount: -11200.00, avgAmount: -84.85 },
  { month: 'March', year: 2024, transactionCount: 158, totalAmount: -14300.00, avgAmount: -90.51 },
  { month: 'April', year: 2024, transactionCount: 142, totalAmount: -13100.00, avgAmount: -92.25 },
  { month: 'May', year: 2024, transactionCount: 165, totalAmount: -15800.00, avgAmount: -95.76 },
  { month: 'June', year: 2024, transactionCount: 138, totalAmount: -12900.00, avgAmount: -93.48 },
];

const monthlyColumns = [
  { key: 'month', header: 'Month', width: '120px' },
  { key: 'year', header: 'Year', width: '80px' },
  { key: 'transactionCount', header: 'Trans. Count', width: '120px' },
  { 
    key: 'totalAmount', 
    header: 'Total Amount', 
    width: '150px',
    render: (value: number) => <AmountDisplay amount={value} colored />
  },
  { 
    key: 'avgAmount', 
    header: 'Average', 
    width: '120px',
    render: (value: number) => <AmountDisplay amount={value} colored />
  },
];

export const ReportsPage: React.FC = () => {
  const navigate = useNavigate();
  
  const [reportType, setReportType] = useState<ReportType>('MONTHLY');
  const [selectedAccount, setSelectedAccount] = useState('');
  const [month, setMonth] = useState(new Date().getMonth() + 1);
  const [year, setYear] = useState(new Date().getFullYear());
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [confirmationType, setConfirmationType] = useState<ConfirmationType>('LIST');
  const [showResults, setShowResults] = useState(false);
  const [page, setPage] = useState(1);

  // Fetch accounts for dropdown
  const { data: accountsData } = useQuery({
    queryKey: [QUERY_KEYS.ACCOUNTS],
    queryFn: () => accountService.getAccounts(1, 100),
  });

  const accounts = accountsData?.items || [];
  const reportData = mockReportData;
  const pageSize = 10;
  const totalPages = Math.ceil(reportData.length / pageSize);
  const paginatedData = reportData.slice((page - 1) * pageSize, page * pageSize);

  const handleGenerateReport = (e: React.FormEvent) => {
    e.preventDefault();
    setShowResults(true);
  };

  const handleClear = () => {
    setReportType('MONTHLY');
    setSelectedAccount('');
    setMonth(new Date().getMonth() + 1);
    setYear(new Date().getFullYear());
    setStartDate('');
    setEndDate('');
    setConfirmationType('LIST');
    setShowResults(false);
    setPage(1);
  };

  // Calculate totals
  const totalTransactions = reportData.reduce((sum, row) => sum + row.transactionCount, 0);
  const totalAmount = reportData.reduce((sum, row) => sum + row.totalAmount, 0);

  return (
    <div className="pb-16">
      <PageHeader
        title="Transaction Reports"
        subtitle="Generate transaction reports by period"
        actions={
          <div className="flex space-x-3">
            <Button variant="secondary" onClick={() => navigate('/dashboard')}>
              F3=Exit
            </Button>
            <Button variant="secondary" onClick={handleClear}>
              F4=Clear
            </Button>
          </div>
        }
      />

      {/* Report Options */}
      <form onSubmit={handleGenerateReport}>
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-6">
          {/* Report Type Selection */}
          <CardPanel title="Report Type">
            <div className="space-y-3">
              <label className="flex items-center cursor-pointer">
                <input
                  type="radio"
                  name="reportType"
                  className="h-4 w-4 text-blue-600 focus:ring-blue-500"
                  checked={reportType === 'MONTHLY'}
                  onChange={() => setReportType('MONTHLY')}
                />
                <span className="ml-2 text-sm">Monthly Report</span>
              </label>
              <label className="flex items-center cursor-pointer">
                <input
                  type="radio"
                  name="reportType"
                  className="h-4 w-4 text-blue-600 focus:ring-blue-500"
                  checked={reportType === 'YEARLY'}
                  onChange={() => setReportType('YEARLY')}
                />
                <span className="ml-2 text-sm">Yearly Report</span>
              </label>
              <label className="flex items-center cursor-pointer">
                <input
                  type="radio"
                  name="reportType"
                  className="h-4 w-4 text-blue-600 focus:ring-blue-500"
                  checked={reportType === 'CUSTOM'}
                  onChange={() => setReportType('CUSTOM')}
                />
                <span className="ml-2 text-sm">Custom Date Range</span>
              </label>
            </div>
          </CardPanel>

          {/* Period Selection */}
          <CardPanel title="Period Selection">
            <div className="space-y-4">
              {reportType === 'MONTHLY' && (
                <div className="grid grid-cols-2 gap-4">
                  <FormField label="Month" name="month">
                    <select
                      className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                      value={month}
                      onChange={(e) => setMonth(Number(e.target.value))}
                    >
                      {Array.from({ length: 12 }, (_, i) => (
                        <option key={i + 1} value={i + 1}>
                          {new Date(2024, i).toLocaleString('default', { month: 'long' })}
                        </option>
                      ))}
                    </select>
                  </FormField>
                  <FormField label="Year" name="year">
                    <select
                      className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                      value={year}
                      onChange={(e) => setYear(Number(e.target.value))}
                    >
                      {Array.from({ length: 5 }, (_, i) => {
                        const y = new Date().getFullYear() - i;
                        return <option key={y} value={y}>{y}</option>;
                      })}
                    </select>
                  </FormField>
                </div>
              )}

              {reportType === 'YEARLY' && (
                <FormField label="Year" name="year">
                  <select
                    className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    value={year}
                    onChange={(e) => setYear(Number(e.target.value))}
                  >
                    {Array.from({ length: 5 }, (_, i) => {
                      const y = new Date().getFullYear() - i;
                      return <option key={y} value={y}>{y}</option>;
                    })}
                  </select>
                </FormField>
              )}

              {reportType === 'CUSTOM' && (
                <div className="grid grid-cols-2 gap-4">
                  <FormField label="Start Date" name="startDate">
                    <input
                      type="date"
                      className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                      value={startDate}
                      onChange={(e) => setStartDate(e.target.value)}
                    />
                  </FormField>
                  <FormField label="End Date" name="endDate">
                    <input
                      type="date"
                      className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                      value={endDate}
                      onChange={(e) => setEndDate(e.target.value)}
                    />
                  </FormField>
                </div>
              )}

              <FormField label="Account (Optional)" name="account">
                <select
                  className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  value={selectedAccount}
                  onChange={(e) => setSelectedAccount(e.target.value)}
                >
                  <option value="">All Accounts</option>
                  {accounts.map((account: any) => (
                    <option key={account.accountId} value={account.accountId}>
                      {account.accountId}
                    </option>
                  ))}
                </select>
              </FormField>
            </div>
          </CardPanel>

          {/* Output Options */}
          <CardPanel title="Output Options">
            <div className="space-y-3">
              <label className="flex items-center cursor-pointer">
                <input
                  type="radio"
                  name="confirmationType"
                  className="h-4 w-4 text-blue-600 focus:ring-blue-500"
                  checked={confirmationType === 'LIST'}
                  onChange={() => setConfirmationType('LIST')}
                />
                <span className="ml-2 text-sm">Display on Screen</span>
              </label>
              <label className="flex items-center cursor-pointer">
                <input
                  type="radio"
                  name="confirmationType"
                  className="h-4 w-4 text-blue-600 focus:ring-blue-500"
                  checked={confirmationType === 'PRINT'}
                  onChange={() => setConfirmationType('PRINT')}
                />
                <span className="ml-2 text-sm">Print Report</span>
              </label>
              <label className="flex items-center cursor-pointer">
                <input
                  type="radio"
                  name="confirmationType"
                  className="h-4 w-4 text-blue-600 focus:ring-blue-500"
                  checked={confirmationType === 'BOTH'}
                  onChange={() => setConfirmationType('BOTH')}
                />
                <span className="ml-2 text-sm">Both (Display & Print)</span>
              </label>

              <div className="pt-4 border-t">
                <Button type="submit" className="w-full">
                  F5=Generate Report
                </Button>
              </div>
            </div>
          </CardPanel>
        </div>
      </form>

      {/* Report Results */}
      {showResults && (
        <>
          {/* Summary Cards */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
            <div className="bg-blue-50 rounded-lg p-4">
              <h4 className="text-xs text-blue-600 uppercase font-semibold">Total Transactions</h4>
              <p className="text-2xl font-bold text-blue-900">{totalTransactions.toLocaleString()}</p>
            </div>
            <div className="bg-red-50 rounded-lg p-4">
              <h4 className="text-xs text-red-600 uppercase font-semibold">Total Amount</h4>
              <p className="text-2xl font-bold text-red-900">
                ${Math.abs(totalAmount).toLocaleString('en-US', { minimumFractionDigits: 2 })}
              </p>
            </div>
            <div className="bg-gray-50 rounded-lg p-4">
              <h4 className="text-xs text-gray-600 uppercase font-semibold">Average Per Month</h4>
              <p className="text-2xl font-bold text-gray-900">
                ${Math.abs(totalAmount / reportData.length).toLocaleString('en-US', { minimumFractionDigits: 2 })}
              </p>
            </div>
          </div>

          {/* Report Table */}
          <CardPanel 
            title={`Transaction Report - ${
              reportType === 'MONTHLY' 
                ? `${new Date(year, month - 1).toLocaleString('default', { month: 'long' })} ${year}`
                : reportType === 'YEARLY'
                ? `Year ${year}`
                : `${startDate} to ${endDate}`
            }`}
          >
            <DataTable
              data={paginatedData}
              columns={monthlyColumns}
              keyField="month"
              emptyMessage="No data available for the selected period"
            />

            <div className="border-t p-4">
              <Pagination
                currentPage={page}
                totalPages={totalPages}
                totalItems={reportData.length}
                pageSize={pageSize}
                onPageChange={setPage}
              />
            </div>

            {/* Export Buttons */}
            <div className="border-t p-4 flex justify-end space-x-3">
              <Button variant="secondary" onClick={() => {}}>
                Export CSV
              </Button>
              <Button variant="secondary" onClick={() => {}}>
                Export PDF
              </Button>
              <Button onClick={() => window.print()}>
                Print Report
              </Button>
            </div>
          </CardPanel>
        </>
      )}
    </div>
  );
};
