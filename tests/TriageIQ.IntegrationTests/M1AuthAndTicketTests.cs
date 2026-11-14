using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TriageIQ.Application.Auth;
using TriageIQ.Application.Tickets;
using TriageIQ.Domain.Enums;

namespace TriageIQ.IntegrationTests;

public sealed class M1AuthAndTicketTests : IClassFixture<TriageIQApiWebApplicationFactory>
{
    private readonly HttpClient _client;
    public M1AuthAndTicketTests(TriageIQApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        factory.EnsureDatabase();
    }

    [Fact]
    public async Task Register_DefaultRole_IsViewer()
    {
        var email = $"viewer-{Guid.NewGuid():N}@test.local";

        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(email, "Password123!@#"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.Role.Should().Be(UserRole.Viewer);
        body.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Viewer_CannotCreateTicket_Returns403()
    {
        var token = await RegisterAndGetTokenAsync($"viewer-{Guid.NewGuid():N}@test.local");
        using var request = CreateTicketRequest(token, "idem-403");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Reviewer_CanCreateTicket_AndIdempotencyKeyReturnsSameTicket()
    {
        var token = await LoginAdminAndCreateReviewerAsync();
        var idempotencyKey = $"idem-{Guid.NewGuid():N}";

        using var first = CreateTicketRequest(token, idempotencyKey);
        var firstResponse = await _client.SendAsync(first);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstTicket = await firstResponse.Content.ReadFromJsonAsync<TicketDto>();

        using var second = CreateTicketRequest(token, idempotencyKey);
        var secondResponse = await _client.SendAsync(second);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondTicket = await secondResponse.Content.ReadFromJsonAsync<TicketDto>();
        secondTicket!.Id.Should().Be(firstTicket!.Id);
    }

    [Fact]
    public async Task ValidationError_ReturnsProblemDetails()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest("not-an-email", "short"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadAsStringAsync();
        problem.Should().Contain("Validation error");
    }

    private async Task<string> RegisterAndGetTokenAsync(string email)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(email, "Password123!@#"));

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return body!.AccessToken;
    }

    private async Task<string> LoginAdminAndCreateReviewerAsync()
    {
        var adminToken = await LoginAdminAsync();

        var reviewerEmail = $"reviewer-{Guid.NewGuid():N}@test.local";
        var register = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(reviewerEmail, "Password123!@#"));
        register.EnsureSuccessStatusCode();
        var registered = await register.Content.ReadFromJsonAsync<AuthResponse>();

        using var changeRole = new HttpRequestMessage(
            HttpMethod.Put,
            $"/api/users/{GetUserIdFromToken(registered!.AccessToken)}/role");
        changeRole.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        changeRole.Content = JsonContent.Create(new ChangeUserRoleRequest(UserRole.Reviewer));

        var roleResponse = await _client.SendAsync(changeRole);
        roleResponse.EnsureSuccessStatusCode();

        var login = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(reviewerEmail, "Password123!@#"));
        login.EnsureSuccessStatusCode();
        var loginBody = await login.Content.ReadFromJsonAsync<AuthResponse>();
        return loginBody!.AccessToken;
    }

    private async Task<string> LoginAdminAsync()
    {
        var login = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                TriageIQApiWebApplicationFactory.AdminEmail,
                TriageIQApiWebApplicationFactory.AdminPassword));

        login.EnsureSuccessStatusCode();
        var body = await login.Content.ReadFromJsonAsync<AuthResponse>();
        return body!.AccessToken;
    }

    private static Guid GetUserIdFromToken(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var json = System.Text.Encoding.UTF8.GetString(PadBase64(payload));
        using var doc = JsonDocument.Parse(json);
        return Guid.Parse(doc.RootElement.GetProperty("sub").GetString()!);
    }

    private static byte[] PadBase64(string base64)
    {
        var padded = base64.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }

        return Convert.FromBase64String(padded);
    }

    private static HttpRequestMessage CreateTicketRequest(string bearerToken, string idempotencyKey)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/tickets");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        request.Headers.Add("Idempotency-Key", idempotencyKey);
        request.Content = JsonContent.Create(new CreateTicketRequest(
            "Synthetic billing question",
            "I was charged twice on my test account.",
            "web-form"));
        return request;
    }
}