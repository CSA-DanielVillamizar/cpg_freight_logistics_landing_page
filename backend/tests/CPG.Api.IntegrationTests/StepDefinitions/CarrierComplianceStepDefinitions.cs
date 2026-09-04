using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CPG.Api.IntegrationTests.Support;
using CPG.Application.Common.Interfaces;
using CPG.Application.Features.Authentication;
using CPG.Domain.Enums;
using CPG.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Reqnroll;

namespace CPG.Api.IntegrationTests.StepDefinitions;

[Binding]
public sealed class CarrierComplianceStepDefinitions(ScenarioState state)
{
    private const string CarrierEmail = "carrier@cpgorlando.com";

    private Guid _carrierId;
    private Guid _carrierUserId;
    private Guid _documentId;
    private string _blobUri = string.Empty;

    [Given(@"an authenticated Carrier with ID ""(.*)"" and status ""(.*)""")]
    public async Task GivenAnAuthenticatedCarrier(string carrierRef, string status)
    {
        _ = carrierRef;

        var login = await state.Client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = CarrierEmail,
            Password = ApplicationDbContextInitialiser.SeedPassword,
        });
        login.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        state.Authenticate(document.RootElement.GetProperty("accessToken").GetString()!);
        _carrierUserId = document.RootElement.GetProperty("user").GetProperty("id").GetGuid();

        // Put the seeded carrier into the required starting state, deterministically.
        var expectedStatus = Enum.Parse<ComplianceStatus>(status.Replace(" ", string.Empty, StringComparison.Ordinal));

        await TestScope.WithDbContextAsync(async db =>
        {
            var carrier = await db.Carriers
                .Include(c => c.ComplianceDocuments)
                .FirstAsync(c => c.UserId == _carrierUserId);

            _carrierId = carrier.Id;

            db.ComplianceDocuments.RemoveRange(carrier.ComplianceDocuments);
            await db.SaveChangesAsync();

            await db.Database.ExecuteSqlAsync(
                $"UPDATE carriers SET \"ComplianceStatus\" = {expectedStatus.ToString()} WHERE \"Id\" = {carrier.Id}");
        });
    }

    [When(@"the carrier uploads a valid PDF file ""(.*)"" of size (.*) MB via POST ""(.*)""")]
    public async Task WhenTheCarrierUploadsAPdf(string fileName, string sizeMb, string path)
    {
        var bytes = BuildPdf((int)(double.Parse(sizeMb, System.Globalization.CultureInfo.InvariantCulture) * 1024 * 1024));

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "file", fileName);
        content.Add(new StringContent(nameof(ComplianceDocumentType.CertificateOfInsurance)), "documentType");

        state.LastResponse = await state.Client.PostAsync(path, content);
        state.LastBody = await state.LastResponse.Content.ReadAsStringAsync();

        state.LastResponse.StatusCode.Should().Be(HttpStatusCode.Accepted, state.LastBody);

        using var result = JsonDocument.Parse(state.LastBody!);
        _documentId = result.RootElement.GetProperty("documentId").GetGuid();
        _blobUri = result.RootElement.GetProperty("blobUri").GetString()!;
    }

    [Then(@"the system should store the file securely in cloud blob storage")]
    public async Task ThenTheFileIsStoredInBlobStorage()
    {
        var blobName = await TestScope.WithDbContextAsync(db => db.ComplianceDocuments
            .Where(d => d.Id == _documentId)
            .Select(d => d.BlobUri)
            .FirstAsync());

        blobName.Should().Be(_blobUri);

        // Blob key = everything after "/<container>/" in the stored URI.
        var key = new Uri(_blobUri).AbsolutePath
            .Split($"/{UploadContainer}/", 2, StringSplitOptions.None)[^1];

        var length = await TestScope.WithServiceAsync<IBlobStorage, long>(async blob =>
        {
            await using var stream = await blob.DownloadAsync(UploadContainer, key);
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory);
            return memory.Length;
        });

        length.Should().BeGreaterThan(2_000_000, "the stored blob must contain the uploaded PDF");
    }

    [Then(@"the carrier compliance record should update to status ""(.*)""")]
    public async Task ThenTheCarrierStatusIsUpdated(string status)
    {
        var expected = Enum.Parse<ComplianceStatus>(status.Replace(" ", string.Empty, StringComparison.Ordinal));

        var actual = await TestScope.WithDbContextAsync(db => db.Carriers
            .AsNoTracking()
            .Where(c => c.Id == _carrierId)
            .Select(c => c.ComplianceStatus)
            .FirstAsync());

        actual.Should().Be(expected);
    }

    [Then(@"an audit log entry must be recorded in PostgreSQL with timestamp and user ID")]
    public async Task ThenAnAuditLogEntryIsRecorded()
    {
        var entry = await TestScope.WithDbContextAsync(db => db.AuditLogEntries
            .AsNoTracking()
            .Where(a => a.Action == "ComplianceDocumentUploaded" && a.EntityId == _carrierId.ToString())
            .OrderByDescending(a => a.TimestampUtc)
            .FirstOrDefaultAsync());

        entry.Should().NotBeNull();
        entry!.UserId.Should().Be(_carrierUserId.ToString());
        entry.TimestampUtc.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-5));
        entry.EntityName.Should().Be("Carrier");

        // The RabbitMQ event round-trips: publish -> broker -> consumer writes this audit row.
        var notified = await TestScope.EventuallyAsync(
            () => TestScope.WithDbContextAsync(db => db.AuditLogEntries
                .AsNoTracking()
                .AnyAsync(a => a.Action == "CommercialTeamNotified" && a.EntityId == _carrierId.ToString())),
            TimeSpan.FromSeconds(20));

        notified.Should().BeTrue("the ComplianceDocumentUploaded integration event must be consumed from RabbitMQ");
    }

    private const string UploadContainer = "compliance-documents";

    private static byte[] BuildPdf(int totalBytes)
    {
        var buffer = new byte[totalBytes];
        var header = Encoding.ASCII.GetBytes("%PDF-1.4\n");
        Array.Copy(header, buffer, header.Length);
        for (var i = header.Length; i < buffer.Length; i++)
        {
            buffer[i] = (byte)'0';
        }

        return buffer;
    }
}
