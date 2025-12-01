import api from '@/lib/axios';
import { API_ENDPOINTS, QUERY_KEYS } from '@/config/constants';
import type { Account, Card, Transaction, Customer, PagedResult } from '@/types';

// Re-export QUERY_KEYS for convenience
export { QUERY_KEYS };

// User types (for admin functionality)
export interface User {
  userId: string;
  firstName: string;
  lastName: string;
  userType: 'ADMIN' | 'MANAGER' | 'USER';
  status: 'A' | 'I';
  lastLogin?: string;
  createdDate: string;
  updatedDate?: string;
}

export interface CreateUserRequest {
  userId: string;
  firstName: string;
  lastName: string;
  userType: string;
  password: string;
}

export interface UpdateUserRequest {
  firstName?: string;
  lastName?: string;
  userType?: string;
  status?: string;
  newPassword?: string;
}

// Report types
export interface ReportSummary {
  month?: string;
  year: number;
  transactionCount: number;
  totalAmount: number;
  avgAmount: number;
}

export interface ReportRequest {
  reportType: 'MONTHLY' | 'YEARLY' | 'CUSTOM';
  accountId?: number;
  month?: number;
  year: number;
  startDate?: string;
  endDate?: string;
}

// Payment types
export interface PaymentRequest {
  accountId: number;
  amount: number;
  paymentDate: string;
}

export interface PaymentResult {
  confirmationNumber: string;
  timestamp: string;
  accountId: number;
  amount: number;
  newBalance: number;
}

export const accountService = {
  async getAccounts(pageNumber: number = 1, pageSize: number = 20): Promise<PagedResult<Account>> {
    const { data } = await api.get<PagedResult<Account>>(API_ENDPOINTS.ACCOUNTS, {
      params: { pageNumber, pageSize }
    });
    return data;
  },

  async getAccountById(id: number): Promise<Account> {
    const { data } = await api.get<Account>(API_ENDPOINTS.ACCOUNT_BY_ID(id));
    return data;
  },

  async getAccountsByCustomer(customerId: number): Promise<Account[]> {
    const { data } = await api.get<Account[]>(API_ENDPOINTS.ACCOUNTS_BY_CUSTOMER(customerId));
    return data;
  },

  async updateAccount(id: number, account: Partial<Account>): Promise<Account> {
    const { data } = await api.put<Account>(API_ENDPOINTS.ACCOUNT_BY_ID(id), account);
    return data;
  }
};

export const cardService = {
  async getCards(pageNumber: number = 1, pageSize: number = 20): Promise<PagedResult<Card>> {
    const { data } = await api.get<PagedResult<Card>>(API_ENDPOINTS.CARDS, {
      params: { pageNumber, pageSize }
    });
    return data;
  },

  async getCardByNumber(cardNumber: string): Promise<Card> {
    const { data } = await api.get<Card>(API_ENDPOINTS.CARD_BY_NUMBER(cardNumber));
    return data;
  },

  async getCardsByAccount(accountId: number, pageNumber: number = 1, pageSize: number = 20): Promise<PagedResult<Card>> {
    const { data } = await api.get<PagedResult<Card>>(API_ENDPOINTS.CARDS_BY_ACCOUNT(accountId), {
      params: { pageNumber, pageSize }
    });
    return data;
  },

  async updateCard(cardNumber: string, card: Partial<Card>): Promise<Card> {
    const { data } = await api.put<Card>(API_ENDPOINTS.CARD_BY_NUMBER(cardNumber), card);
    return data;
  }
};

export const transactionService = {
  async getTransactions(pageNumber: number = 1, pageSize: number = 20): Promise<PagedResult<Transaction>> {
    const { data } = await api.get<PagedResult<Transaction>>(API_ENDPOINTS.TRANSACTIONS, {
      params: { pageNumber, pageSize }
    });
    return data;
  },

  async getTransactionById(id: string): Promise<Transaction> {
    const { data } = await api.get<Transaction>(API_ENDPOINTS.TRANSACTION_BY_ID(id));
    return data;
  },

  async getTransactionsByAccount(
    accountId: number,
    pageNumber: number = 1,
    pageSize: number = 20
  ): Promise<PagedResult<Transaction>> {
    const { data } = await api.get<PagedResult<Transaction>>(
      API_ENDPOINTS.TRANSACTIONS_BY_ACCOUNT(accountId),
      { params: { pageNumber, pageSize } }
    );
    return data;
  },

  async getTransactionsByCard(
    cardNumber: string,
    pageNumber: number = 1,
    pageSize: number = 20
  ): Promise<PagedResult<Transaction>> {
    const { data } = await api.get<PagedResult<Transaction>>(
      API_ENDPOINTS.TRANSACTIONS_BY_CARD(cardNumber),
      { params: { pageNumber, pageSize } }
    );
    return data;
  },

  async createTransaction(transaction: Partial<Transaction>): Promise<Transaction> {
    const { data } = await api.post<Transaction>(API_ENDPOINTS.TRANSACTIONS, transaction);
    return data;
  }
};

export const customerService = {
  async getAllCustomers(pageNumber: number = 1, pageSize: number = 20): Promise<PagedResult<Customer>> {
    const { data } = await api.get<PagedResult<Customer>>(API_ENDPOINTS.CUSTOMERS, {
      params: { pageNumber, pageSize }
    });
    return data;
  },

  async getCustomerById(id: number): Promise<Customer> {
    const { data } = await api.get<Customer>(API_ENDPOINTS.CUSTOMER_BY_ID(id));
    return data;
  }
};

export const userService = {
  async getUsers(pageNumber: number = 1, pageSize: number = 20): Promise<PagedResult<User>> {
    const { data } = await api.get<PagedResult<User>>(API_ENDPOINTS.USERS, {
      params: { pageNumber, pageSize }
    });
    return data;
  },

  async getUserById(userId: string): Promise<User> {
    const { data } = await api.get<User>(API_ENDPOINTS.USER_BY_ID(userId));
    return data;
  },

  async createUser(user: CreateUserRequest): Promise<User> {
    const { data } = await api.post<User>(API_ENDPOINTS.USERS, user);
    return data;
  },

  async updateUser(userId: string, user: UpdateUserRequest): Promise<User> {
    const { data } = await api.put<User>(API_ENDPOINTS.USER_BY_ID(userId), user);
    return data;
  },

  async deleteUser(userId: string): Promise<void> {
    await api.delete(API_ENDPOINTS.USER_BY_ID(userId));
  }
};

export const reportService = {
  async getMonthlyReport(year: number, month: number, accountId?: number): Promise<ReportSummary[]> {
    const { data } = await api.get<ReportSummary[]>(API_ENDPOINTS.REPORT_MONTHLY, {
      params: { year, month, accountId }
    });
    return data;
  },

  async getYearlyReport(year: number, accountId?: number): Promise<ReportSummary[]> {
    const { data } = await api.get<ReportSummary[]>(API_ENDPOINTS.REPORT_YEARLY, {
      params: { year, accountId }
    });
    return data;
  },

  async getCustomReport(startDate: string, endDate: string, accountId?: number): Promise<ReportSummary[]> {
    const { data } = await api.get<ReportSummary[]>(API_ENDPOINTS.REPORT_CUSTOM, {
      params: { startDate, endDate, accountId }
    });
    return data;
  }
};

export const paymentService = {
  async makePayment(payment: PaymentRequest): Promise<PaymentResult> {
    const { data } = await api.post<PaymentResult>(API_ENDPOINTS.PAYMENTS, payment);
    return data;
  },

  async getPaymentsByAccount(accountId: number, pageNumber: number = 1, pageSize: number = 20): Promise<PagedResult<PaymentResult>> {
    const { data } = await api.get<PagedResult<PaymentResult>>(
      API_ENDPOINTS.PAYMENT_BY_ACCOUNT(accountId),
      { params: { pageNumber, pageSize } }
    );
    return data;
  }
};
