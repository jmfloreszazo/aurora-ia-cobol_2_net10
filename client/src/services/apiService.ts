import api from '@/lib/axios';
import { API_ENDPOINTS } from '@/config/constants';
import type { Account, Card, Transaction, Customer, PagedResult } from '@/types';

export const accountService = {
  async getAccountById(id: number): Promise<Account> {
    const { data } = await api.get<Account>(API_ENDPOINTS.ACCOUNT_BY_ID(id));
    return data;
  },

  async getAccountsByCustomer(customerId: number): Promise<Account[]> {
    const { data } = await api.get<Account[]>(API_ENDPOINTS.ACCOUNTS_BY_CUSTOMER(customerId));
    return data;
  }
};

export const cardService = {
  async getCardsByAccount(accountId: number): Promise<Card[]> {
    const { data } = await api.get<Card[]>(API_ENDPOINTS.CARDS_BY_ACCOUNT(accountId));
    return data;
  }
};

export const transactionService = {
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
