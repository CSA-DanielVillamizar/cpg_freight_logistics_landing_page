# Transcribed verbatim from SPEC.md section 3, US-01.
@us01 @rbac
Feature: RBAC and Secure Authentication
  As a System Administrator
  I want to restrict access to platform modules based on user roles (Admin, Carrier, Shipper)
  So that sensitive logistics data and load operations remain secure and compliant

  Scenario: Successful login with valid credentials
    Given a user exists with email "admin@cpgorlando.com" and role "Admin"
    When the user sends a POST request to "/api/auth/login" with valid credentials
    Then the response status code should be 200
    And the response body should contain a valid JWT access token and a refresh token

  Scenario: Unauthorized access to admin endpoints
    Given an authenticated user with role "Carrier"
    When the user attempts to send a GET request to "/api/admin/audit-logs"
    Then the response status code should be 403 Forbidden
    And the response body must contain an error message "Access denied"
