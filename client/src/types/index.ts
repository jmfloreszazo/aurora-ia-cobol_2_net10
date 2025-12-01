export interface LoginRequest {
  userId: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  userId: string;
  fullName: string;
  userType: string;
}

export interface User {
  userId: string;
  fullName: string;
  userType: string;
  token: string;
}

export interface Account {
  accountId: number;
  customerId: number;
  customerName: string;
  activeStatus: string;
  currentBalance: number;
  creditLimit: number;
  cashCreditLimit: number;
  availableCredit: number;
  creditUtilization: number;
  openDate: string;
  expirationDate: string;
  currentCycleCredit: number;
  currentCycleDebit: number;
  numberOfCards: number;
}

export interface Card {
  cardNumber: string;
  maskedCardNumber: string;
  accountId: number;
  cardType: string;
  embossedName: string;
  expirationDate: string;
  activeStatus: string;
  isActive: boolean;
}

export interface Transaction {
  transactionId: string;
  accountId: number;
  cardNumber: string;
  maskedCardNumber: string;
  transactionType: string;
  transactionTypeDescription: string;
  categoryCode: number;
  categoryDescription: string;
  transactionSource: string;
  description: string;
  amount: number;
  merchantId?: string;
  merchantName?: string;
  merchantCity?: string;
  transactionDate: string;
  processedFlag: string;
  isProcessed: boolean;
}

export interface Customer {
  customerId: number;
  firstName: string;
  middleName?: string;
  lastName: string;
  fullName: string;
  dateOfBirth: string;
  ssn: string;
  governmentId: string;
  phoneNumber1: string;
  phoneNumber2?: string;
  addressLine1: string;
  addressLine2?: string;
  stateCode: string;
  zipCode: string;
  countryCode: string;
  ficoScore: number;
  numberOfAccounts: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface ApiError {
  message: string;
  status?: number;
}
