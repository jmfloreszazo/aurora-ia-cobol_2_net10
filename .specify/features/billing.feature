Feature: Bill Payment Processing
  As a registered user
  I want to make payments on my credit card account
  So that I can reduce my balance and maintain good standing

  Background:
    Given I am logged in as "USER0001"
    And the following accounts exist:
      | AccountId   | CustomerId | CurrentBalance | CreditLimit | ActiveStatus |
      | 10000000001 | 100000001  | 1500.00        | 5000.00     | Y            |
      | 10000000002 | 100000001  | 0.00           | 2000.00     | Y            |

  # ============================================
  # Basic Payment Operations (COBIL00C equivalent)
  # ============================================

  Scenario: View billing information for account
    When I navigate to the bill payment page
    And I select account "10000000001"
    Then I should see the following billing information:
      | Field                | Value       |
      | Account ID           | 10000000001 |
      | Current Balance      | $1,500.00   |
      | Minimum Payment Due  | $30.00      |
      | Payment Due Date     | Next cycle  |
      | Available Credit     | $3,500.00   |

  Scenario: Make a partial payment
    Given account "10000000001" has balance of $1500.00
    When I navigate to the bill payment page
    And I select account "10000000001"
    And I enter payment amount "500.00"
    And I click "Submit Payment"
    Then I should see a success message "Payment processed successfully"
    And a confirmation number should be generated
    And the account balance should be $1,000.00
    And a payment transaction should be created

  Scenario: Make minimum payment
    Given account "10000000001" has balance of $1500.00
    And the minimum payment is $30.00
    When I click "Pay Minimum"
    Then the payment amount should be automatically set to $30.00
    When I confirm the payment
    Then the account balance should be $1,470.00

  Scenario: Pay full balance (COBIL00C exact behavior)
    Given account "10000000001" has balance of $1500.00
    When I click "Pay Full Balance"
    Then the payment amount should be automatically set to $1,500.00
    When I confirm the payment
    Then the account balance should be $0.00
    And I should see a message "Account paid in full"

  Scenario: Pay full balance via API endpoint
    Given account "10000000001" has balance of $1500.00
    When I send POST request to "/api/Payments/pay-full-balance/10000000001"
    Then the response status should be 200
    And the response should contain:
      | Field           | Value       |
      | AccountId       | 10000000001 |
      | AmountPaid      | 1500.00     |
      | NewBalance      | 0.00        |
      | ConfirmationNo  | Generated   |

  # ============================================
  # Payment Validation
  # ============================================

  Scenario: Cannot pay more than current balance
    Given account "10000000001" has balance of $1500.00
    When I enter payment amount "2000.00"
    And I click "Submit Payment"
    Then I should see a warning "Payment amount exceeds current balance"
    And I should be offered to pay the full balance instead

  Scenario: Payment amount must be positive
    When I enter payment amount "0.00"
    And I click "Submit Payment"
    Then I should see a validation error "Payment amount must be greater than zero"

  Scenario: Payment amount must be valid number
    When I enter payment amount "abc"
    Then I should see a validation error "Please enter a valid amount"

  Scenario: Cannot make payment on zero balance account
    Given account "10000000002" has balance of $0.00
    When I select account "10000000002" for payment
    Then I should see a message "No balance due on this account"
    And the payment form should be disabled

  Scenario: Cannot make payment on inactive account
    Given account "10000000003" has status "Inactive"
    When I attempt to make a payment on this account
    Then I should see an error "Cannot make payments on inactive accounts"

  # ============================================
  # Payment Confirmation
  # ============================================

  Scenario: Payment generates confirmation receipt
    When I successfully process a payment of $500.00
    Then I should receive a confirmation with:
      | Field              | Value               |
      | Confirmation Number| Auto-generated      |
      | Account ID         | 10000000001         |
      | Amount Paid        | $500.00             |
      | New Balance        | $1,000.00           |
      | Payment Date       | Current timestamp   |

  Scenario: View payment in transaction history
    When I make a payment of $500.00
    And I navigate to the transactions page
    Then I should see a payment transaction with:
      | Field       | Value     |
      | Type        | Payment   |
      | Amount      | +$500.00  |
      | Description | Bill Payment |

  # ============================================
  # Payment History
  # ============================================

  Scenario: View payment history for account
    Given account "10000000001" has the following payment history:
      | Date       | Amount  | ConfirmationNo |
      | 2025-11-28 | 500.00  | PAY123456789   |
      | 2025-10-28 | 300.00  | PAY123456788   |
      | 2025-09-28 | 250.00  | PAY123456787   |
    When I view payment history for account "10000000001"
    Then I should see all 3 payments
    And they should be sorted by date descending

  Scenario: Filter payment history by date range
    When I view payment history
    And I filter by date range "2025-10-01" to "2025-10-31"
    Then I should only see payments from October 2025

  # ============================================
  # Multiple Payment Methods (Future Enhancement)
  # ============================================

  Scenario: Payment method selection
    When I navigate to the bill payment page
    Then I should see payment method options:
      | Method           | Description           |
      | Bank Transfer    | Direct from checking  |
      | Credit Transfer  | From another card     |
      | Manual Entry     | Enter amount manually |

  # ============================================
  # Edge Cases
  # ============================================

  Scenario: Handle concurrent payments
    Given account "10000000001" has balance of $1500.00
    When two payments of $1000.00 are submitted simultaneously
    Then only one payment should succeed
    And the second should fail with "Insufficient balance"
    And data integrity should be maintained

  Scenario: Payment with decimal precision
    When I enter payment amount "123.45"
    And I process the payment
    Then the payment should be processed for exactly $123.45
    And the balance should be calculated with proper decimal precision

  Scenario: Large payment amount formatting
    Given account has balance of $50,000.00
    When I view the bill payment page
    Then the balance should be displayed as "$50,000.00"
    And I should be able to pay the full amount
