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
  TRANSACTIONS_BY_ACCOUNT: (accountId: number) => `/api/Transactions/account/${accountId}`,
  TRANSACTIONS_BY_CARD: (cardNumber: string) => `/api/Transactions/card/${cardNumber}`,
  
  // Cards
  CARDS_BY_ACCOUNT: (accountId: number) => `/api/Cards/account/${accountId}`,
  
  // Customers
  CUSTOMERS: '/api/Customers',
  CUSTOMER_BY_ID: (id: number) => `/api/Customers/${id}`
};

// Query Keys
export const QUERY_KEYS = {
  ACCOUNTS: 'accounts',
  ACCOUNT: 'account',
  TRANSACTIONS: 'transactions',
  CARDS: 'cards',
  CUSTOMERS: 'customers',
  CUSTOMER: 'customer'
};
