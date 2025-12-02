Feature: Batch Job Processing
  As a system administrator
  I want to run batch processing jobs
  So that I can process transactions, calculate interest, and generate statements

  Background:
    Given I am logged in as "ADMIN001" with role "ADMIN"
    And the following accounts exist:
      | AccountId   | CustomerId | CurrentBalance | CreditLimit | ActiveStatus |
      | 10000000001 | 100000001  | 1500.00        | 5000.00     | Y            |
      | 10000000002 | 100000001  | 250.50         | 2000.00     | Y            |
    And the following transactions exist:
      | TransactionId    | AccountId   | Amount  | ProcessedFlag | TransactionDate |
      | TX00000000000001 | 10000000001 | -125.50 | N             | 2025-11-28      |
      | TX00000000000002 | 10000000001 | -45.00  | N             | 2025-11-27      |
      | TX00000000000003 | 10000000002 | -100.00 | Y             | 2025-11-26      |

  # ============================================
  # Transaction Posting (CBTRN01C/CBTRN02C equivalent)
  # ============================================

  Scenario: Post pending transactions successfully
    When I run the "Post Transactions" batch job
    Then the job should complete successfully
    And transaction "TX00000000000001" should have ProcessedFlag "Y"
    And transaction "TX00000000000002" should have ProcessedFlag "Y"
    And account "10000000001" current balance should be updated
    And a job result should be returned with:
      | Field             | Value     |
      | RecordsProcessed  | 2         |
      | RecordsSucceeded  | 2         |
      | RecordsFailed     | 0         |
      | Status            | Completed |

  Scenario: Skip already processed transactions
    When I run the "Post Transactions" batch job
    Then transaction "TX00000000000003" should not be reprocessed
    And the job should only process transactions with ProcessedFlag "N"

  Scenario: Handle transaction posting with inactive card
    Given transaction "TX00000000000004" references an inactive card
    When I run the "Post Transactions" batch job
    Then the transaction should be marked as failed
    And an error should be logged "Card inactive or expired"
    And the job should continue processing other transactions

  Scenario: Handle transaction posting exceeding credit limit
    Given account "10000000001" has:
      | CurrentBalance | CreditLimit |
      | 4900.00        | 5000.00     |
    And a pending transaction exists for $200.00
    When I run the "Post Transactions" batch job
    Then the transaction should be rejected
    And an error should be logged "Credit limit exceeded"
    And the account balance should not change

  Scenario: Update account cycle totals during posting
    Given account "10000000001" has:
      | CurrentCycleDebit | CurrentCycleCredit |
      | 0.00              | 0.00               |
    When I post a debit transaction of $125.50
    Then the CurrentCycleDebit should be updated to $125.50
    When I post a credit transaction of $50.00
    Then the CurrentCycleCredit should be updated to $50.00

  # ============================================
  # Interest Calculation (CBACT02C equivalent)
  # ============================================

  Scenario: Calculate daily interest on account balances
    Given account "10000000001" has current balance of $1500.00
    And the annual interest rate is 19.99%
    When I run the "Calculate Interest" batch job
    Then interest should be calculated using daily rate (APR/365)
    And an interest transaction should be created
    And the account balance should be increased by the interest amount

  Scenario: Skip interest calculation for zero balance accounts
    Given account "10000000002" has current balance of $0.00
    When I run the "Calculate Interest" batch job
    Then no interest transaction should be created for this account
    And the job result should show this account was skipped

  Scenario: Skip interest calculation for inactive accounts
    Given account "10000000003" has status "Inactive"
    When I run the "Calculate Interest" batch job
    Then no interest transaction should be created for this account

  Scenario: Interest calculation creates proper transaction record
    When I run the "Calculate Interest" batch job
    Then interest transactions should have:
      | Field            | Value                    |
      | TransactionType  | Interest                 |
      | Description      | Daily interest charge    |
      | ProcessedFlag    | Y                        |
      | MerchantName     | CardDemo Interest        |

  # ============================================
  # Statement Generation (CBSTM03A/CBSTM03B equivalent)
  # ============================================

  Scenario: Generate monthly statement for an account
    Given account "10000000001" has transactions in November 2025
    When I run the "Generate Statements" batch job for November 2025
    Then a statement should be generated with:
      | Field                | Description                          |
      | Previous Balance     | Balance at start of month            |
      | Total Debits         | Sum of all debit transactions        |
      | Total Credits        | Sum of all credit transactions       |
      | Interest Charges     | Sum of interest transactions         |
      | Current Balance      | Balance at end of month              |
      | Minimum Payment Due  | Greater of $25 or 2% of balance      |

  Scenario: Calculate minimum payment correctly
    Given account "10000000001" has current balance of $1500.00
    When the statement is generated
    Then the minimum payment should be $30.00 (2% of $1500)
    
    Given account "10000000002" has current balance of $250.50
    When the statement is generated
    Then the minimum payment should be $25.00 (minimum floor)

  Scenario: Include transaction details in statement
    Given account "10000000001" has 5 transactions in the statement period
    When I generate the statement
    Then the statement should include all 5 transactions
    And each transaction should show:
      | Field           |
      | Transaction ID  |
      | Date            |
      | Description     |
      | Amount          |
      | Running Balance |

  # ============================================
  # Data Export (CBEXPORT equivalent)
  # ============================================

  Scenario: Export accounts in COBOL fixed-width format
    When I run the "Export Accounts" batch job with format "FixedWidth"
    Then a file should be generated in COBOL-compatible format
    And each record should have fixed-width fields:
      | Field         | Width | Position |
      | AccountId     | 11    | 1-11     |
      | CustomerId    | 9     | 12-20    |
      | Balance       | 12    | 21-32    |
      | CreditLimit   | 12    | 33-44    |
      | Status        | 1     | 45       |

  Scenario: Export transactions in CSV format
    When I run the "Export Transactions" batch job with format "CSV"
    Then a CSV file should be generated
    And the file should have headers
    And data should be comma-separated

  Scenario: Export customers in JSON format
    When I run the "Export Customers" batch job with format "JSON"
    Then a JSON file should be generated
    And the file should be valid JSON
    And each customer should be a JSON object

  Scenario: Export with date range filter
    When I run the "Export Transactions" batch job
    And I specify date range from "2025-11-01" to "2025-11-30"
    Then only transactions within that range should be exported

  # ============================================
  # Nightly Batch Cycle
  # ============================================

  Scenario: Run complete nightly batch cycle
    When I trigger the "Nightly Batch" job
    Then the following jobs should run in sequence:
      | Order | Job                    |
      | 1     | Post Transactions      |
      | 2     | Calculate Interest     |
      | 3     | Generate Statements    |
    And a summary report should be generated
    And the total duration should be recorded

  Scenario: Nightly batch continues on partial failure
    Given the "Post Transactions" job fails for some records
    When the nightly batch is running
    Then the "Calculate Interest" job should still execute
    And the "Generate Statements" job should still execute
    And the final status should be "CompletedWithErrors"

  Scenario: View batch job history
    Given multiple batch jobs have been executed
    When I navigate to the batch jobs page
    And I click "View History"
    Then I should see a list of past job executions
    And each entry should show:
      | Field            |
      | Job Name         |
      | Start Time       |
      | End Time         |
      | Status           |
      | Records Affected |

  # ============================================
  # Authorization & Security
  # ============================================

  Scenario: Regular user cannot run batch jobs
    Given I am logged in as "USER0001" with role "USER"
    When I attempt to run any batch job
    Then I should receive a 403 Forbidden error
    And the job should not execute

  Scenario: Batch jobs are audited
    When I run any batch job
    Then an audit log entry should be created
    And the entry should include:
      | Field          |
      | JobType        |
      | ExecutedBy     |
      | StartTime      |
      | EndTime        |
      | Status         |
      | RecordsAffected|
