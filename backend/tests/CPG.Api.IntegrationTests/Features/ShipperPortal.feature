@shipper
Feature: Shipper self-service portal
  As a corporate shipper
  I want to see my active shipments separated from my delivered history
  So that I can track freight and pull proof of delivery

  Scenario: Viewing shipments and downloading a proof of delivery
    Given a shipper is authenticated
    When the shipper requests their loads
    Then the active shipments include an InTransit or Dispatched load
    And the delivered history includes a load with proof of delivery
    When the shipper downloads the proof of delivery for that delivered load
    Then the download is a PDF

  Scenario: A carrier cannot use the shipper portal
    Given a carrier is authenticated for the shipper portal
    When the carrier requests the shipper loads
    Then the shipper request fails with status 403
