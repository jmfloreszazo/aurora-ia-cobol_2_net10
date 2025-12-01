// API Base URL
export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5299';

// API Endpoints
export const API_ENDPOINTS = {
  // Auth
  LOGIN: '/api/Auth/login',
  REGISTER: '/api/Auth/register',
  
  // Accounts
  ACCOUNTS: '/api/Accounts',
  ACCOUNT_BY_ID: (id: number) => `/api/Accounts/${id}`,
  ACCOUNTS_BY_CUSTOMER: (customerId: number) => `/api/Accounts/customer/${customerId}`,
  
  // Transactions
  TRANSACTIONS: '/api/Transactions',
  TRANSACTION_BY_ID: (id: string) => `/api/Transactions/${id}`,
  TRANSACTIONS_BY_ACCOUNT: (accountId: number) => `/api/Transactions/account/${accountId}`,
  TRANSACTIONS_BY_CARD: (cardNumber: string) => `/api/Transactions/card/${cardNumber}`,
  
  // Cards
  CARDS: '/api/Cards',
  CARD_BY_NUMBER: (cardNumber: string) => `/api/Cards/${cardNumber}`,
  CARDS_BY_ACCOUNT: (accountId: number) => `/api/Cards/account/${accountId}`,
  
  // Customers
  CUSTOMERS: '/api/Customers',
  CUSTOMER_BY_ID: (id: number) => `/api/Customers/${id}`,
  
  // Users (Admin)
  USERS: '/api/Users',
  USER_BY_ID: (userId: string) => `/api/Users/${userId}`,
  
  // Reports
  REPORTS: '/api/Reports',
  REPORT_MONTHLY: '/api/Reports/monthly',
  REPORT_YEARLY: '/api/Reports/yearly',
  REPORT_CUSTOM: '/api/Reports/custom',
  
  // Payments/Billing
  PAYMENTS: '/api/Payments',
  PAYMENT_BY_ACCOUNT: (accountId: number) => `/api/Payments/account/${accountId}`,
};

// Query Keys
export const QUERY_KEYS = {
  ACCOUNTS: 'accounts',
  ACCOUNT: 'account',
  TRANSACTIONS: 'transactions',
  TRANSACTION: 'transaction',
  CARDS: 'cards',
  CARD: 'card',
  CUSTOMERS: 'customers',
  CUSTOMER: 'customer',
  USERS: 'users',
  USER: 'user',
  REPORTS: 'reports',
  PAYMENTS: 'payments',
};
