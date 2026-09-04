# Transcribed verbatim from SPEC.md section 3, US-03.
@us03 @compliance
Feature: Carrier Document Compliance and Verification
  As a Carrier
  I want to upload my mandatory legal documents (COI, Insurance, FDOT permits)
  So that my account status updates from Pending to Verified to accept high-value loads

  Scenario: Successfully uploading a Certificate of Insurance (COI)
    Given an authenticated Carrier with ID "CAR-001" and status "Pending Compliance"
    When the carrier uploads a valid PDF file "coi_insurance.pdf" of size 2.4 MB via POST "/api/compliance/upload"
    Then the system should store the file securely in cloud blob storage
    And the carrier compliance record should update to status "Under Review"
    And an audit log entry must be recorded in PostgreSQL with timestamp and user ID
