using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ServiceDesk.IntegrationTests;

/// <summary>
/// Core shell, auth redirects, headers, error pages and health endpoint as served in Production.
/// </summary>
public sealed class ShellTests(ServiceDeskWebFactory factory) : IClassFixture<ServiceDeskWebFactory>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Home_redirects_to_sign_in_when_unauthenticated()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        using var response = await client.GetAsync(new Uri("/", UriKind.Relative), Ct);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/account/login", response.Headers.Location?.OriginalString, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Sign_in_page_renders()
    {
        using var client = factory.CreateClient();

        var html = await client.GetStringAsync(new Uri("/account/login", UriKind.Relative), Ct);

        Assert.Contains("<h1>Sign in</h1>", html, StringComparison.Ordinal);
        Assert.Contains("name=\"UserName\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Production_hides_developer_pages()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        using var devPage = await client.GetAsync(new Uri("/dev/ui", UriKind.Relative), Ct);
        Assert.Equal(HttpStatusCode.NotFound, devPage.StatusCode);
    }

    [Fact]
    public async Task Every_page_sends_security_headers()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        using var response = await client.GetAsync(new Uri("/", UriKind.Relative), Ct);

        var csp = Assert.Single(response.Headers.GetValues("Content-Security-Policy"));
        Assert.Contains("script-src 'self';", csp, StringComparison.Ordinal);
        Assert.DoesNotContain("localhost", csp, StringComparison.Ordinal);
        Assert.Equal("nosniff", Assert.Single(response.Headers.GetValues("X-Content-Type-Options")));
        Assert.Equal("DENY", Assert.Single(response.Headers.GetValues("X-Frame-Options")));
        Assert.Equal("strict-origin-when-cross-origin", Assert.Single(response.Headers.GetValues("Referrer-Policy")));
        Assert.False(response.Headers.Contains("Server"));
    }

    [Fact]
    public async Task Unknown_pages_redirect_unauthenticated_users_to_sign_in()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        using var response = await client.GetAsync(new Uri("/no-such-page", UriKind.Relative), Ct);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/account/login", response.Headers.Location?.OriginalString, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Unknown_api_routes_answer_with_unauthorized_for_anonymous_requests()
    {
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("/api/no-such-endpoint", UriKind.Relative));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await client.SendAsync(request, Ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Health_endpoint_reports_healthy_as_json()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/health", UriKind.Relative), Ct);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("Healthy", json.RootElement.GetProperty("status").GetString());
        var check = Assert.Single(json.RootElement.GetProperty("checks").EnumerateArray());
        Assert.Equal("self", check.GetProperty("name").GetString());
        Assert.False(check.TryGetProperty("description", out _));
    }

    [Fact]
    public async Task Theme_files_are_served()
    {
        using var client = factory.CreateClient();

        using var css = await client.GetAsync(new Uri("/css/site.css", UriKind.Relative), Ct);
        using var font = await client.GetAsync(new Uri("/lib/fonts/plus-jakarta-sans/files/plus-jakarta-sans-latin-400-normal.woff2", UriKind.Relative), Ct);

        Assert.Equal(HttpStatusCode.OK, css.StatusCode);
        Assert.Equal(HttpStatusCode.OK, font.StatusCode);
    }
}
