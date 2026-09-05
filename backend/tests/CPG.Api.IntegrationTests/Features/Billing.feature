@billing
Feature: Freight billing and Stripe payments
  As CPG Enterprises
  I want an invoice raised automatically when a load is delivered
  So that the shipper can pay it online through Stripe Checkout

  Scenario: A delivered load is billed and paid
    Given a carrier is authenticated for billing
    And the carrier has the in-transit load "CPG-48219"
    When the carrier marks the load delivered
    Then an invoice for "CPG-48219" is raised for the shipper
    When the shipper starts a checkout for that invoice
    Then a checkout URL is returned
    When Stripe confirms the checkout completed
    Then the invoice for "CPG-48219" is Paid
