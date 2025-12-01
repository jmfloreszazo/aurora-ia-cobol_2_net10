Feature: User Authentication
  As a registered user of the CardDemo system
  I want to authenticate with my credentials
  So that I can securely access my account information

  Background:
    Given the following users exist in the system:
      | UserId    | Password    | FirstName | LastName | UserType |
      | USER0001  | Password123 | John      | Doe      | USER     |
      | ADMIN001  | Admin@123   | Jane      | Smith    | ADMIN    |

  Scenario: Successful login with valid user credentials
    Given I am on the login page
    When I enter user ID "USER0001"
    And I enter password "Password123"
    And I click the login button
    Then I should be redirected to the main menu
    And I should receive a valid JWT token
    And the token should contain user ID "USER0001"
    And the token should contain role "USER"
    And the token should expire in 15 minutes

  Scenario: Successful login with valid admin credentials
    Given I am on the login page
    When I enter user ID "ADMIN001"
    And I enter password "Admin@123"
    And I click the login button
    Then I should be redirected to the admin menu
    And I should receive a valid JWT token
    And the token should contain user ID "ADMIN001"
    And the token should contain role "ADMIN"

  Scenario: Failed login with invalid user ID
    Given I am on the login page
    When I enter user ID "INVALID99"
    And I enter password "Password123"
    And I click the login button
    Then I should see an error message "Invalid credentials"
    And I should remain on the login page
    And the response status should be 401

  Scenario: Failed login with invalid password
    Given I am on the login page
    When I enter user ID "USER0001"
    And I enter password "WrongPassword"
    And I click the login button
    Then I should see an error message "Invalid credentials"
    And I should remain on the login page
    And the response status should be 401

  Scenario: Failed login with empty credentials
    Given I am on the login page
    When I click the login button without entering credentials
    Then I should see validation errors
    And the error for "UserId" should be "User ID is required"
    And the error for "Password" should be "Password is required"

  Scenario: Account lockout after multiple failed attempts
    Given I am on the login page
    When I enter user ID "USER0001"
    And I enter wrong password 3 times
    Then I should see an error message "Account locked. Contact administrator"
    And the account "USER0001" should be locked
    And a security event should be logged

  Scenario: Successful logout
    Given I am logged in as "USER0001"
    When I click the logout button
    Then I should be redirected to the login page
    And my JWT token should be invalidated
    And my refresh token should be revoked

  Scenario: Token refresh with valid refresh token
    Given I am logged in as "USER0001"
    And my access token has expired
    When I send a refresh token request
    Then I should receive a new access token
    And I should receive a new refresh token
    And the old refresh token should be invalidated

  Scenario: Token refresh with invalid refresh token
    Given I have an invalid or expired refresh token
    When I send a refresh token request
    Then I should receive an error "Invalid refresh token"
    And the response status should be 401
    And I should be redirected to the login page

  Scenario: Access protected endpoint without authentication
    Given I am not authenticated
    When I attempt to access "/api/accounts"
    Then the response status should be 401
    And I should receive an error message "Unauthorized"

  Scenario: Access admin endpoint as regular user
    Given I am logged in as "USER0001" with role "USER"
    When I attempt to access "/api/users"
    Then the response status should be 403
    And I should receive an error message "Forbidden"
