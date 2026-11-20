using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ServiceDesk.IntegrationTests;

/// <summary>
/// The developer UI kit, and the request plumbing it exercises: antiforgery tokens, TempData
/// toasts after a redirect, and server-side validation messages.
/// </summary>
public sealed partial class DevelopmentPagesTests(DevelopmentWebFactory factory) : IClassFixture<DevelopmentWebFactory>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Ui_kit_shows_every_ticket_status()
    {
        using var client = factory.CreateClient();

        var html = await client.GetStringAsync(new Uri("/dev/ui", UriKind.Relative), Ct);

        foreach (var label in new[] { "New", "Open", "In Progress", "Pending Requester", "Pending Third Party", "On Hold", "Reopened", "Resolved", "Closed", "Canceled" })
        {
            Assert.Contains($"\">{label}</span>", html, StringComparison.Ordinal);
        }
        Assert.Contains("sd-env-badge", html, StringComparison.Ordinal);
        // Unicode text is written as-is, not as &#x..; entities.
        Assert.Contains("Delete the category “Printers”?", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Posts_without_an_antiforgery_token_are_rejected()
    {
        using var client = CreateClient();

        using var response = await client.PostAsync(new Uri("/dev/ui/confirm", UriKind.Relative), new FormUrlEncodedContent([]), Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task A_valid_post_redirects_and_the_next_page_shows_the_toast()
    {
        using var client = CreateClient();
        var token = await GetTokenAsync(client);

        using var post = await client.PostAsync(
            new Uri("/dev/ui/toast", UriKind.Relative),
            new FormUrlEncodedContent([new("kind", "success"), new("__RequestVerificationToken", token)]),
            Ct);
        var next = await client.GetStringAsync(new Uri("/dev/ui", UriKind.Relative), Ct);
        var afterThat = await client.GetStringAsync(new Uri("/dev/ui", UriKind.Relative), Ct);

        Assert.Equal(HttpStatusCode.Redirect, post.StatusCode);
        Assert.Equal("/dev/ui", post.Headers.Location?.OriginalString);
        Assert.Contains("Ticket MIS-26K014 created.", next, StringComparison.Ordinal);
        Assert.DoesNotContain("Ticket MIS-26K014 created.", afterThat, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_server_validates_the_sample_form()
    {
        using var client = CreateClient();
        var token = await GetTokenAsync(client);

        using var post = await client.PostAsync(
            new Uri("/dev/ui/form", UriKind.Relative),
            new FormUrlEncodedContent([new("Form.Subject", ""), new("Form.Description", "short"), new("__RequestVerificationToken", token)]),
            Ct);
        var html = await post.Content.ReadAsStringAsync(Ct);

        Assert.Equal(HttpStatusCode.OK, post.StatusCode);
        Assert.Contains("Enter a subject.", html, StringComparison.Ordinal);
        Assert.Contains("Choose a category.", html, StringComparison.Ordinal);
        Assert.Contains("Describe the problem in at least 10 characters.", html, StringComparison.Ordinal);
    }

    private HttpClient CreateClient() =>
        factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private static async Task<string> GetTokenAsync(HttpClient client)
    {
        var html = await client.GetStringAsync(new Uri("/dev/ui", UriKind.Relative), Ct);
        return TokenPattern().Match(html).Groups[1].Value;
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex TokenPattern();
}
