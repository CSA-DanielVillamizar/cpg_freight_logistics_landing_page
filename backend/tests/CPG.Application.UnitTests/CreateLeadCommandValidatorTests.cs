using CPG.Application.Features.Leads.Create;
using CPG.Domain.Enums;
using FluentValidation.TestHelper;
using Xunit;

namespace CPG.Application.UnitTests;

public sealed class CreateLeadCommandValidatorTests
{
    private readonly CreateLeadCommandValidator _validator = new();

    private static CreateLeadCommand Valid() => new(
        CompanyName: "Apex Construction",
        ContactName: "Alex Apex",
        ContactEmail: "contact@apex.com",
        Phone: "(407) 555-0100",
        VerticalSlug: "fdot-concrete-barricades",
        ServiceType: ServiceType.FdotConcrete,
        CargoDetails: "120 concrete Jersey barricades, Orlando to Tampa, next month");

    [Fact]
    public void Accepts_a_well_formed_inquiry()
        => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Rejects_bad_email(string email)
        => _validator.TestValidate(Valid() with { ContactEmail = email })
            .ShouldHaveValidationErrorFor(x => x.ContactEmail);

    [Fact]
    public void Rejects_markup_injection_in_cargo_details()
        => _validator.TestValidate(Valid() with { CargoDetails = "<script>alert(1)</script> barricades" })
            .ShouldHaveValidationErrorFor(x => x.CargoDetails);

    [Fact]
    public void Rejects_links_in_cargo_details()
        => _validator.TestValidate(Valid() with { CargoDetails = "see http://spam.example for details" })
            .ShouldHaveValidationErrorFor(x => x.CargoDetails);

    [Fact]
    public void Rejects_non_kebab_case_vertical_slug()
        => _validator.TestValidate(Valid() with { VerticalSlug = "FDOT Concrete" })
            .ShouldHaveValidationErrorFor(x => x.VerticalSlug);

    [Fact]
    public void Rejects_malformed_phone()
        => _validator.TestValidate(Valid() with { Phone = "call me" })
            .ShouldHaveValidationErrorFor(x => x.Phone);
}
