using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ServiceDesk.IntegrationTests;

/// <summary>The layout, headers, error pages and health endpoint, as served in Production.</summary>
public sealed class ShellTests(ServiceDeskWebFactory factory) : IClassFixture<ServiceDeskWebFactory>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Dashboard_renders_inside_the_shell()
    {
        using var client = factory.CreateClient();

        var html = await client.GetStringAsync(new Uri("/", UriKind.Relative), Ct);

        Assert.Contains("id=\"sdSidebar\"", html, StringComparison.Ordinal);
        Assert.Contains("<h1 class=\"sd-page-title\">Dashboard</h1>", html, StringComparison.Ordinal);
        Assert.Contains("class=\"sd-nav-link active\"", html, StringComparison.Ordinal);
        Assert.Contains("aria-current=\"page\"", html, StringComparison.Ordinal);
        Assert.Contains("System status", html, StringComparison.Ordinal);
        Assert.Contains("Asia/Manila", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Production_hides_developer_pages_and_the_environment_badge()
    {
        using var client = factory.CreateClient();

        var html = await client.GetStringAsync(new Uri("/", UriKind.Relative), Ct);
        using var devPage = await client.GetAsync(new Uri("/dev/ui", UriKind.Relative), Ct);

        Assert.DoesNotContain("UI kit", html, StringComparison.Ordinal);
        Assert.DoesNotContain("sd-env-badge", html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.NotFound, devPage.StatusCode);
    }

    [Fact]
    public async Task Every_page_sends_security_headers()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/", UriKind.Relative), Ct);

        var csp = Assert.Single(response.Headers.GetValues("Content-Security-Policy"));
        Assert.Contains("script-src 'self';", csp, StringComparison.Ordinal);
        Assert.DoesNotContain("localhost", csp, StringComparison.Ordinal);
        Assert.Equal("nosniff", Assert.Single(response.Headers.GetValues("X-Content-Type-Options")));
        Assert.Equal("DENY", Assert.Single(response.Headers.GetValues("X-Frame-Options")));
        Assert.Equal("strict-origin-when-cross-origin", Assert.Single(response.Headers.GetValues("Referrer-Policy")));
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.False(response.Headers.Contains("Server"));
    }

    [Fact]
    public async Task Unknown_pages_show_the_friendly_404_page_with_headers()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/no-such-page", UriKind.Relative), Ct);
        var html = await response.Content.ReadAsStringAsync(Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Page not found", html, StringComparison.Ordinal);
        Assert.Contains("Go to the dashboard", html, StringComparison.Ordinal);
        Assert.True(response.Headers.Contains("Content-Security-Policy"));
    }

    [Fact]
    public async Task Unknown_api_routes_answer_with_problem_details()
    {
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("/api/no-such-endpoint", UriKind.Relative));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await client.SendAsync(request, Ct);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(404, json.RootElement.GetProperty("status").GetInt32());
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
