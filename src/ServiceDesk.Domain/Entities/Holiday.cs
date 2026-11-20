namespace ServiceDesk.Domain.Entities;

public class Holiday
{
    public int Id { get; set; }
    public DateOnly HolidayDate { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsRecurring { get; set; }
}