using ServiceDesk.Application.Exceptions;
using ServiceDesk.Web.ErrorHandling;

namespace ServiceDesk.UnitTests.Web;

public sealed class ExceptionStatusMapperTests
{
    [Fact]
    public void Not_found_maps_to_404_with_a_friendly_message()
    {
        var mapping = ExceptionStatusMapper.Map(new NotFoundException("Ticket", 42));

        Assert.Equal(404, mapping.StatusCode);
        Assert.True(mapping.IsExpected);
        Assert.Equal("We couldn't find that ticket. It may have been removed, or the link may be out of date.", mapping.Message);
    }

    [Fact]
    public void Forbidden_maps_to_403()
    {
        Assert.Equal(403, ExceptionStatusMapper.Map(new ForbiddenAccessException()).StatusCode);
    }

    [Fact]
    public void Business_rules_and_edit_conflicts_map_to_409_and_keep_their_message()
    {
        var rule = ExceptionStatusMapper.Map(new BusinessRuleException("A category named \"Printers\" already exists."));
        var conflict = ExceptionStatusMapper.Map(new ConcurrencyConflictException("Ticket"));

        Assert.Equal(409, rule.StatusCode);
        Assert.Equal("A category named \"Printers\" already exists.", rule.Message);
        Assert.Equal(409, conflict.StatusCode);
        Assert.StartsWith("Someone else changed this ticket", conflict.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Unexpected_errors_never_leak_their_message()
    {
        var mapping = ExceptionStatusMapper.Map(new InvalidOperationException("Server=db;Password=secret"));

        Assert.Equal(500, mapping.StatusCode);
        Assert.False(mapping.IsExpected);
        Assert.Equal(ExceptionStatusMapper.UnexpectedMessage, mapping.Message);
        Assert.DoesNotContain("secret", mapping.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void A_missing_exception_is_treated_as_unexpected()
    {
        Assert.Equal(500, ExceptionStatusMapper.Map(null).StatusCode);
    }

    [Theory]
    [InlineData(404, "Page not found")]
    [InlineData(403, "No access")]
    [InlineData(429, "Too many requests")]
    [InlineData(503, "Something went wrong")]
    public void Status_codes_have_friendly_titles(int statusCode, string title)
    {
        Assert.Equal(title, StatusCodeText.For(statusCode).Title);
    }
}
