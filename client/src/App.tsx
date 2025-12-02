import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthProvider } from '@/contexts/AuthContext';
import { ProtectedRoute } from '@/components/ProtectedRoute';
import { MainLayout } from '@/components/Layout/MainLayout';
import { LoginPage } from '@/pages/LoginPage';
import { DashboardPage } from '@/pages/DashboardPage';

// Account pages
import { AccountListPage, AccountViewPage, AccountEditPage } from '@/pages/accounts';

// Card pages
import { CardListPage, CardViewPage, CardEditPage } from '@/pages/cards';

// Transaction pages
import { TransactionListPage, TransactionViewPage, TransactionAddPage } from '@/pages/transactions';

// User Admin pages
import { UserListPage, UserAddPage, UserEditPage, UserDeletePage } from '@/pages/users';

// Reports page
import { ReportsPage } from '@/pages/reports';

// Billing page
import { BillPaymentPage } from '@/pages/billing';

// Batch Jobs page (Admin)
import { BatchJobsPage } from '@/pages/batch';

// Create a client for React Query
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000, // 5 minutes
      retry: 1,
    },
  },
});

// Wrapper component for protected routes with MainLayout
const ProtectedLayout: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <ProtectedRoute>
    <MainLayout>{children}</MainLayout>
  </ProtectedRoute>
);

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AuthProvider>
          <Routes>
            {/* Public routes */}
            <Route path="/login" element={<LoginPage />} />

            {/* Protected routes with MainLayout */}
            <Route
              path="/dashboard"
              element={
                <ProtectedLayout>
                  <DashboardPage />
                </ProtectedLayout>
              }
            />

            {/* Account routes */}
            <Route
              path="/accounts"
              element={
                <ProtectedLayout>
                  <AccountListPage />
                </ProtectedLayout>
              }
            />
            <Route
              path="/accounts/:accountId"
              element={
                <ProtectedLayout>
                  <AccountViewPage />
                </ProtectedLayout>
              }
            />
            <Route
              path="/accounts/:accountId/edit"
              element={
                <ProtectedLayout>
                  <AccountEditPage />
                </ProtectedLayout>
              }
            />

            {/* Card routes */}
            <Route
              path="/cards"
              element={
                <ProtectedLayout>
                  <CardListPage />
                </ProtectedLayout>
              }
            />
            <Route
              path="/cards/:cardNumber"
              element={
                <ProtectedLayout>
                  <CardViewPage />
                </ProtectedLayout>
              }
            />
            <Route
              path="/cards/:cardNumber/edit"
              element={
                <ProtectedLayout>
                  <CardEditPage />
                </ProtectedLayout>
              }
            />

            {/* Transaction routes */}
            <Route
              path="/transactions"
              element={
                <ProtectedLayout>
                  <TransactionListPage />
                </ProtectedLayout>
              }
            />
            <Route
              path="/transactions/:transactionId"
              element={
                <ProtectedLayout>
                  <TransactionViewPage />
                </ProtectedLayout>
              }
            />
            <Route
              path="/transactions/new"
              element={
                <ProtectedLayout>
                  <TransactionAddPage />
                </ProtectedLayout>
              }
            />

            {/* Admin User Management routes */}
            <Route
              path="/admin/users"
              element={
                <ProtectedLayout>
                  <UserListPage />
                </ProtectedLayout>
              }
            />
            <Route
              path="/admin/users/new"
              element={
                <ProtectedLayout>
                  <UserAddPage />
                </ProtectedLayout>
              }
            />
            <Route
              path="/admin/users/:userId/edit"
              element={
                <ProtectedLayout>
                  <UserEditPage />
                </ProtectedLayout>
              }
            />
            <Route
              path="/admin/users/:userId/delete"
              element={
                <ProtectedLayout>
                  <UserDeletePage />
                </ProtectedLayout>
              }
            />

            {/* Reports routes */}
            <Route
              path="/reports"
              element={
                <ProtectedLayout>
                  <ReportsPage />
                </ProtectedLayout>
              }
            />

            {/* Bill Payment routes */}
            <Route
              path="/billing"
              element={
                <ProtectedLayout>
                  <BillPaymentPage />
                </ProtectedLayout>
              }
            />

            {/* Batch Jobs routes (Admin) */}
            <Route
              path="/admin/batch-jobs"
              element={
                <ProtectedLayout>
                  <BatchJobsPage />
                </ProtectedLayout>
              }
            />

            {/* Default redirect */}
            <Route path="/" element={<Navigate to="/dashboard" replace />} />
            
            {/* Catch-all redirect */}
            <Route path="*" element={<Navigate to="/dashboard" replace />} />
          </Routes>
        </AuthProvider>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
