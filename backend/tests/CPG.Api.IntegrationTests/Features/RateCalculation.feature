# Transcribed verbatim from SPEC.md section 3, US-02.
@us02 @rates
Feature: Dynamic Rate Calculation for Specialized Freight
  As a Shipper
  I want to calculate precise shipping rates in real-time
  So that I can budget for cold chain, heavy haul, or FDOT concrete transport accurately

  Scenario: Calculating rate for a Cold Chain refrigerated shipment
    Given a Shipper requests a rate calculation for service type "Cold Chain"
    And origin is "Miami, FL" and destination is "Orlando, FL"
    And cargo weight is 35000 lbs with target temperature of -20 degrees Celsius
    When the client invokes POST "/api/rates/calculate"
    Then the system should return HTTP status 200 OK
    And the computation time must be less than 500 milliseconds
    And the response must break down base rate, cold chain surcharge, and fuel surcharge
