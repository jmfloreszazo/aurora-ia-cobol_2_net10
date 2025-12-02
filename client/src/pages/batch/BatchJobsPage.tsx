import { useState } from 'react';
import { useAuth } from '@/contexts/AuthContext';
import api from '@/lib/axios';

interface BatchJobResult {
  jobId: string;
  jobName: string;
  startTime: string;
  endTime?: string;
  status: string;
  recordsProcessed: number;
  recordsSucceeded: number;
  recordsFailed: number;
  errors: string[];
  outputFilePath?: string;
}

interface NightlyBatchResult {
  startTime: string;
  endTime: string;
  duration: string;
  transactionPosting?: BatchJobResult;
  interestCalculation?: BatchJobResult;
  statementGeneration?: BatchJobResult;
  allSucceeded: boolean;
}

export default function BatchJobsPage() {
  const { user } = useAuth();
  const [loading, setLoading] = useState<string | null>(null);
  const [lastResult, setLastResult] = useState<BatchJobResult | NightlyBatchResult | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [jobHistory, setJobHistory] = useState<BatchJobResult[]>([]);
  const [showHistory, setShowHistory] = useState(false);

  // Only allow admins
  if (user?.userType !== 'ADMIN') {
    return (
      <div className="p-6">
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <p className="text-red-800">Access denied. Admin privileges required.</p>
        </div>
      </div>
    );
  }

  const runJob = async (endpoint: string, jobName: string) => {
    setLoading(jobName);
    setError(null);
    try {
      const response = await api.post(`/BatchJobs/${endpoint}`);
      setLastResult(response.data);
    } catch (err: any) {
      setError(err.response?.data?.message || `Failed to run ${jobName}`);
    } finally {
      setLoading(null);
    }
  };

  const loadHistory = async () => {
    try {
      const response = await api.get('/BatchJobs/history?limit=20');
      setJobHistory(response.data);
      setShowHistory(true);
    } catch (err: any) {
      setError('Failed to load job history');
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Completed': return 'bg-green-100 text-green-800';
      case 'CompletedWithErrors': return 'bg-yellow-100 text-yellow-800';
      case 'Failed': return 'bg-red-100 text-red-800';
      case 'Running': return 'bg-blue-100 text-blue-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  return (
    <div className="p-6 max-w-6xl mx-auto">
      <h1 className="text-2xl font-bold mb-6">Batch Job Administration</h1>
      <p className="text-gray-600 mb-6">
        Execute COBOL batch program equivalents. These jobs process transactions, calculate interest, 
        and generate reports similar to mainframe JCL batch jobs.
      </p>

      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
          <p className="text-red-800">{error}</p>
        </div>
      )}

      {/* Job Categories */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
        {/* Transaction Posting */}
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-lg font-semibold mb-2">Transaction Posting</h2>
          <p className="text-sm text-gray-600 mb-4">
            CBTRN01C/CBTRN02C equivalent. Posts unprocessed transactions, validates cards/accounts, 
            updates balances.
          </p>
          <button
            onClick={() => runJob('post-transactions', 'Transaction Posting')}
            disabled={loading !== null}
            className="w-full bg-blue-600 text-white py-2 px-4 rounded hover:bg-blue-700 disabled:opacity-50"
          >
            {loading === 'Transaction Posting' ? 'Running...' : 'Run Job'}
          </button>
        </div>

        {/* Interest Calculation */}
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-lg font-semibold mb-2">Interest Calculation</h2>
          <p className="text-sm text-gray-600 mb-4">
            CBACT02C equivalent. Calculates and applies daily interest to all accounts with balances.
          </p>
          <button
            onClick={() => runJob('calculate-interest', 'Interest Calculation')}
            disabled={loading !== null}
            className="w-full bg-blue-600 text-white py-2 px-4 rounded hover:bg-blue-700 disabled:opacity-50"
          >
            {loading === 'Interest Calculation' ? 'Running...' : 'Run Job'}
          </button>
        </div>

        {/* Statement Generation */}
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-lg font-semibold mb-2">Statement Generation</h2>
          <p className="text-sm text-gray-600 mb-4">
            CBSTM03A/CBSTM03B equivalent. Generates monthly account statements for all active accounts.
          </p>
          <button
            onClick={() => runJob('generate-statements', 'Statement Generation')}
            disabled={loading !== null}
            className="w-full bg-blue-600 text-white py-2 px-4 rounded hover:bg-blue-700 disabled:opacity-50"
          >
            {loading === 'Statement Generation' ? 'Running...' : 'Run Job'}
          </button>
        </div>

        {/* Export Accounts */}
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-lg font-semibold mb-2">Export Accounts</h2>
          <p className="text-sm text-gray-600 mb-4">
            CBEXPORT equivalent. Exports ACCTDAT file in COBOL fixed-width format.
          </p>
          <button
            onClick={() => runJob('export/accounts', 'Export Accounts')}
            disabled={loading !== null}
            className="w-full bg-green-600 text-white py-2 px-4 rounded hover:bg-green-700 disabled:opacity-50"
          >
            {loading === 'Export Accounts' ? 'Running...' : 'Export'}
          </button>
        </div>

        {/* Export Transactions */}
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-lg font-semibold mb-2">Export Transactions</h2>
          <p className="text-sm text-gray-600 mb-4">
            CBEXPORT equivalent. Exports TRANSACT file in COBOL fixed-width format.
          </p>
          <button
            onClick={() => runJob('export/transactions', 'Export Transactions')}
            disabled={loading !== null}
            className="w-full bg-green-600 text-white py-2 px-4 rounded hover:bg-green-700 disabled:opacity-50"
          >
            {loading === 'Export Transactions' ? 'Running...' : 'Export'}
          </button>
        </div>

        {/* Export Customers */}
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-lg font-semibold mb-2">Export Customers</h2>
          <p className="text-sm text-gray-600 mb-4">
            CBEXPORT equivalent. Exports CUSTDAT file in COBOL fixed-width format.
          </p>
          <button
            onClick={() => runJob('export/customers', 'Export Customers')}
            disabled={loading !== null}
            className="w-full bg-green-600 text-white py-2 px-4 rounded hover:bg-green-700 disabled:opacity-50"
          >
            {loading === 'Export Customers' ? 'Running...' : 'Export'}
          </button>
        </div>
      </div>

      {/* Run Full Nightly Batch */}
      <div className="bg-gradient-to-r from-purple-600 to-indigo-600 rounded-lg shadow p-6 mb-8 text-white">
        <h2 className="text-xl font-bold mb-2">Run Full Nightly Batch Cycle</h2>
        <p className="mb-4 opacity-90">
          Executes all batch jobs in sequence: Transaction Posting → Interest Calculation → Statement Generation (if end of month).
          This simulates the complete mainframe nightly batch cycle.
        </p>
        <button
          onClick={() => runJob('run-nightly-batch', 'Nightly Batch')}
          disabled={loading !== null}
          className="bg-white text-purple-600 py-2 px-6 rounded font-semibold hover:bg-gray-100 disabled:opacity-50"
        >
          {loading === 'Nightly Batch' ? 'Running Full Batch...' : 'Run Nightly Batch'}
        </button>
      </div>

      {/* Last Result */}
      {lastResult && (
        <div className="bg-white rounded-lg shadow p-6 mb-8">
          <h2 className="text-lg font-semibold mb-4">Last Job Result</h2>
          {'jobId' in lastResult ? (
            // Single job result
            <div>
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-4">
                <div>
                  <p className="text-sm text-gray-600">Job ID</p>
                  <p className="font-mono text-sm">{lastResult.jobId}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600">Job Name</p>
                  <p className="font-semibold">{lastResult.jobName}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600">Status</p>
                  <span className={`px-2 py-1 rounded text-sm ${getStatusColor(lastResult.status)}`}>
                    {lastResult.status}
                  </span>
                </div>
                <div>
                  <p className="text-sm text-gray-600">Records</p>
                  <p>
                    <span className="text-green-600">{lastResult.recordsSucceeded}</span>
                    {' / '}
                    <span>{lastResult.recordsProcessed}</span>
                    {lastResult.recordsFailed > 0 && (
                      <span className="text-red-600"> ({lastResult.recordsFailed} failed)</span>
                    )}
                  </p>
                </div>
              </div>
              {lastResult.outputFilePath && (
                <p className="text-sm text-gray-600">
                  Output: <span className="font-mono">{lastResult.outputFilePath}</span>
                </p>
              )}
              {lastResult.errors.length > 0 && (
                <div className="mt-4">
                  <p className="text-sm font-semibold text-red-600">Errors:</p>
                  <ul className="list-disc list-inside text-sm text-red-600">
                    {lastResult.errors.slice(0, 5).map((err, i) => (
                      <li key={i}>{err}</li>
                    ))}
                    {lastResult.errors.length > 5 && (
                      <li>...and {lastResult.errors.length - 5} more</li>
                    )}
                  </ul>
                </div>
              )}
            </div>
          ) : (
            // Nightly batch result
            <div>
              <p className="mb-2">
                <span className="font-semibold">Duration:</span> {(lastResult as NightlyBatchResult).duration}
              </p>
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                {(lastResult as NightlyBatchResult).transactionPosting && (
                  <div className="border rounded p-3">
                    <p className="font-semibold">Transaction Posting</p>
                    <span className={`px-2 py-1 rounded text-xs ${getStatusColor((lastResult as NightlyBatchResult).transactionPosting!.status)}`}>
                      {(lastResult as NightlyBatchResult).transactionPosting!.status}
                    </span>
                  </div>
                )}
                {(lastResult as NightlyBatchResult).interestCalculation && (
                  <div className="border rounded p-3">
                    <p className="font-semibold">Interest Calculation</p>
                    <span className={`px-2 py-1 rounded text-xs ${getStatusColor((lastResult as NightlyBatchResult).interestCalculation!.status)}`}>
                      {(lastResult as NightlyBatchResult).interestCalculation!.status}
                    </span>
                  </div>
                )}
                {(lastResult as NightlyBatchResult).statementGeneration && (
                  <div className="border rounded p-3">
                    <p className="font-semibold">Statement Generation</p>
                    <span className={`px-2 py-1 rounded text-xs ${getStatusColor((lastResult as NightlyBatchResult).statementGeneration!.status)}`}>
                      {(lastResult as NightlyBatchResult).statementGeneration!.status}
                    </span>
                  </div>
                )}
              </div>
            </div>
          )}
        </div>
      )}

      {/* Job History */}
      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex justify-between items-center mb-4">
          <h2 className="text-lg font-semibold">Job History</h2>
          <button
            onClick={loadHistory}
            className="text-blue-600 hover:text-blue-800 text-sm"
          >
            {showHistory ? 'Refresh' : 'Load History'}
          </button>
        </div>
        
        {showHistory && (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b">
                  <th className="text-left py-2">Job ID</th>
                  <th className="text-left py-2">Job Name</th>
                  <th className="text-left py-2">Start Time</th>
                  <th className="text-left py-2">Status</th>
                  <th className="text-right py-2">Records</th>
                </tr>
              </thead>
              <tbody>
                {jobHistory.map((job) => (
                  <tr key={job.jobId} className="border-b hover:bg-gray-50">
                    <td className="py-2 font-mono text-xs">{job.jobId}</td>
                    <td className="py-2">{job.jobName}</td>
                    <td className="py-2">{new Date(job.startTime).toLocaleString()}</td>
                    <td className="py-2">
                      <span className={`px-2 py-1 rounded text-xs ${getStatusColor(job.status)}`}>
                        {job.status}
                      </span>
                    </td>
                    <td className="py-2 text-right">
                      {job.recordsSucceeded}/{job.recordsProcessed}
                    </td>
                  </tr>
                ))}
                {jobHistory.length === 0 && (
                  <tr>
                    <td colSpan={5} className="py-4 text-center text-gray-500">
                      No job history available
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
