import { describe, it, expect, vi, beforeEach } from 'vitest';
import {
  accountService,
  cardService,
  transactionService,
  customerService,
  userService,
  paymentService,
} from '@/services/apiService';

// Mock axios
vi.mock('@/lib/axios', () => ({
  default: {
    post: vi.fn(),
    get: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

describe('apiService', () => {
  let mockAxios: any;

  beforeEach(async () => {
    vi.clearAllMocks();
    mockAxios = await import('@/lib/axios');
  });

  describe('accountService', () => {
    const mockAccount = {
      accountId: 1,
      customerId: 1,
      customerName: 'John Doe',
      activeStatus: 'Y',
      currentBalance: 1500.00,
      creditLimit: 5000.00,
    };

    const mockPagedResult = {
      items: [mockAccount],
      totalCount: 1,
      pageNumber: 1,
      pageSize: 20,
      totalPages: 1,
      hasPreviousPage: false,
      hasNextPage: false,
    };

    describe('getAccounts', () => {
      it('should fetch accounts with pagination', async () => {
        (mockAxios.default.get as any).mockResolvedValue({ data: mockPagedResult });

        const result = await accountService.getAccounts(1, 20);

        expect(mockAxios.default.get).toHaveBeenCalledWith('/api/Accounts', {
          params: { pageNumber: 1, pageSize: 20 },
        });
        expect(result).toEqual(mockPagedResult);
      });

      it('should use default pagination values', async () => {
        (mockAxios.default.get as any).mockResolvedValue({ data: mockPagedResult });

        await accountService.getAccounts();

        expect(mockAxios.default.get).toHaveBeenCalledWith('/api/Accounts', {
          params: { pageNumber: 1, pageSize: 20 },
        });
      });
    });

    describe('getAccountById', () => {
      it('should fetch account by ID', async () => {
        (mockAxios.default.get as any).mockResolvedValue({ data: mockAccount });

        const result = await accountService.getAccountById(1);

        expect(mockAxios.default.get).toHaveBeenCalledWith('/api/Accounts/1');
        expect(result).toEqual(mockAccount);
      });
    });

    describe('getAccountsByCustomer', () => {
      it('should fetch accounts by customer ID', async () => {
        (mockAxios.default.get as any).mockResolvedValue({ data: [mockAccount] });

        const result = await accountService.getAccountsByCustomer(1);

        expect(mockAxios.default.get).toHaveBeenCalledWith('/api/Accounts/customer/1');
        expect(result).toEqual([mockAccount]);
      });
    });

    describe('updateAccount', () => {
      it('should update account', async () => {
        const updateData = { creditLimit: 7500.00 };
        (mockAxios.default.put as any).mockResolvedValue({ data: { ...mockAccount, ...updateData } });

        const result = await accountService.updateAccount(1, updateData);

        expect(mockAxios.default.put).toHaveBeenCalledWith('/api/Accounts/1', updateData);
        expect(result.creditLimit).toBe(7500.00);
      });
    });
  });

  describe('cardService', () => {
    const mockCard = {
      cardNumber: '4000123456789012',
      maskedCardNumber: '****9012',
      accountId: 1,
      cardType: 'GOLD',
      embossedName: 'JOHN DOE',
      expirationDate: '12/2027',
      activeStatus: 'Y',
      isActive: true,
    };

    const mockPagedResult = {
      items: [mockCard],
      totalCount: 1,
      pageNumber: 1,
      pageSize: 20,
      totalPages: 1,
      hasPreviousPage: false,
      hasNextPage: false,
    };

    describe('getCards', () => {
      it('should fetch cards with pagination', async () => {
        (mockAxios.default.get as any).mockResolvedValue({ data: mockPagedResult });

        const result = await cardService.getCards(1, 10);

        expect(mockAxios.default.get).toHaveBeenCalledWith('/api/Cards', {
          params: { pageNumber: 1, pageSize: 10 },
        });
        expect(result).toEqual(mockPagedResult);
      });
    });

    describe('getCardByNumber', () => {
      it('should fetch card by number', async () => {
        (mockAxios.default.get as any).mockResolvedValue({ data: mockCard });

        const result = await cardService.getCardByNumber('4000123456789012');

        expect(mockAxios.default.get).toHaveBeenCalledWith('/api/Cards/4000123456789012');
        expect(result).toEqual(mockCard);
      });
    });

    describe('updateCard', () => {
      it('should update card', async () => {
        const updateData = { activeStatus: 'N' };
        (mockAxios.default.put as any).mockResolvedValue({ data: { ...mockCard, ...updateData } });

        const result = await cardService.updateCard('4000123456789012', updateData);

        expect(mockAxios.default.put).toHaveBeenCalledWith('/api/Cards/4000123456789012', updateData);
        expect(result.activeStatus).toBe('N');
      });
    });
  });

  describe('transactionService', () => {
    const mockTransaction = {
      transactionId: 'TXN001',
      accountId: 1,
      cardNumber: '4000123456789012',
      transactionType: 'PU',
      amount: 150.00,
      description: 'Test purchase',
      transactionDate: '2024-01-15',
    };

    describe('getTransactions', () => {
      it('should fetch transactions with pagination', async () => {
        const mockPagedResult = {
          items: [mockTransaction],
          totalCount: 1,
          pageNumber: 1,
          pageSize: 20,
          totalPages: 1,
        };
        (mockAxios.default.get as any).mockResolvedValue({ data: mockPagedResult });

        const result = await transactionService.getTransactions(1, 20);

        expect(mockAxios.default.get).toHaveBeenCalledWith('/api/Transactions', {
          params: { pageNumber: 1, pageSize: 20 },
        });
        expect(result.items).toHaveLength(1);
      });
    });

    describe('getTransactionById', () => {
      it('should fetch transaction by ID', async () => {
        (mockAxios.default.get as any).mockResolvedValue({ data: mockTransaction });

        const result = await transactionService.getTransactionById('TXN001');

        expect(mockAxios.default.get).toHaveBeenCalledWith('/api/Transactions/TXN001');
        expect(result.transactionId).toBe('TXN001');
      });
    });

    describe('createTransaction', () => {
      it('should create transaction', async () => {
        const newTransaction = {
          accountId: 1,
          cardNumber: '4000123456789012',
          amount: 200.00,
          transactionType: 'PU',
        };
        (mockAxios.default.post as any).mockResolvedValue({ 
          data: { ...mockTransaction, ...newTransaction } 
        });

        const result = await transactionService.createTransaction(newTransaction);

        expect(mockAxios.default.post).toHaveBeenCalledWith('/api/Transactions', newTransaction);
        expect(result.amount).toBe(200.00);
      });
    });
  });

  describe('customerService', () => {
    const mockCustomer = {
      customerId: 1,
      firstName: 'John',
      lastName: 'Doe',
      fullName: 'John Doe',
      addressLine1: '123 Main St',
      stateCode: 'CA',
      zipCode: '90210',
    };

    describe('getAllCustomers', () => {
      it('should fetch all customers with pagination', async () => {
        const mockPagedResult = { items: [mockCustomer], totalCount: 1 };
        (mockAxios.default.get as any).mockResolvedValue({ data: mockPagedResult });

        const result = await customerService.getAllCustomers(1, 10);

        expect(mockAxios.default.get).toHaveBeenCalledWith('/api/Customers', {
          params: { pageNumber: 1, pageSize: 10 },
        });
        expect(result.items).toHaveLength(1);
      });
    });

    describe('getCustomerById', () => {
      it('should fetch customer by ID', async () => {
        (mockAxios.default.get as any).mockResolvedValue({ data: mockCustomer });

        const result = await customerService.getCustomerById(1);

        expect(mockAxios.default.get).toHaveBeenCalledWith('/api/Customers/1');
        expect(result.firstName).toBe('John');
      });
    });
  });

  describe('userService', () => {
    const mockUser = {
      userId: 'ADMIN',
      firstName: 'Admin',
      lastName: 'User',
      userType: 'ADMIN' as const,
      status: 'A' as const,
    };

    describe('getUsers', () => {
      it('should fetch users with pagination', async () => {
        const mockPagedResult = { items: [mockUser], totalCount: 1 };
        (mockAxios.default.get as any).mockResolvedValue({ data: mockPagedResult });

        const result = await userService.getUsers(1, 20);

        expect(mockAxios.default.get).toHaveBeenCalledWith('/api/Users', {
          params: { pageNumber: 1, pageSize: 20 },
        });
        expect(result.items).toHaveLength(1);
      });
    });

    describe('createUser', () => {
      it('should create user', async () => {
        const newUser = {
          userId: 'USER02',
          firstName: 'Test',
          lastName: 'User',
          userType: 'USER',
          password: 'Pass@123',
        };
        (mockAxios.default.post as any).mockResolvedValue({ data: mockUser });

        const result = await userService.createUser(newUser);

        expect(mockAxios.default.post).toHaveBeenCalledWith('/api/Users', newUser);
        expect(result).toBeDefined();
      });
    });

    describe('updateUser', () => {
      it('should update user', async () => {
        const updateData = { firstName: 'Updated' };
        (mockAxios.default.put as any).mockResolvedValue({ data: { ...mockUser, ...updateData } });

        const result = await userService.updateUser('ADMIN', updateData);

        expect(mockAxios.default.put).toHaveBeenCalledWith('/api/Users/ADMIN', updateData);
        expect(result.firstName).toBe('Updated');
      });
    });

    describe('deleteUser', () => {
      it('should delete user', async () => {
        (mockAxios.default.delete as any).mockResolvedValue({});

        await userService.deleteUser('USER02');

        expect(mockAxios.default.delete).toHaveBeenCalledWith('/api/Users/USER02');
      });
    });
  });

  describe('paymentService', () => {
    const mockPaymentResult = {
      confirmationNumber: 'PAY123',
      timestamp: '2024-01-15T10:00:00Z',
      accountId: 1,
      amount: 500.00,
      newBalance: 1000.00,
    };

    describe('makePayment', () => {
      it('should make payment', async () => {
        const paymentRequest = {
          accountId: 1,
          amount: 500.00,
          paymentDate: '2024-01-15',
        };
        (mockAxios.default.post as any).mockResolvedValue({ data: mockPaymentResult });

        const result = await paymentService.makePayment(paymentRequest);

        expect(mockAxios.default.post).toHaveBeenCalledWith('/api/Payments', paymentRequest);
        expect(result.confirmationNumber).toBe('PAY123');
      });
    });

    describe('getPaymentsByAccount', () => {
      it('should fetch payments by account', async () => {
        const mockPagedResult = { items: [mockPaymentResult], totalCount: 1 };
        (mockAxios.default.get as any).mockResolvedValue({ data: mockPagedResult });

        const result = await paymentService.getPaymentsByAccount(1);

        expect(mockAxios.default.get).toHaveBeenCalledWith('/api/Payments/account/1', {
          params: { pageNumber: 1, pageSize: 20 },
        });
        expect(result.items).toHaveLength(1);
      });
    });
  });
});
