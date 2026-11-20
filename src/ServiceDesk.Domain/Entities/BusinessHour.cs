namespace ServiceDesk.Domain.Entities;

public class BusinessHour
{
    public int Id { get; set; }
    public byte DayOfWeek { get; set; } // 0=Sunday..6=Saturday (consistent with .NET DayOfWeek)
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsActive { get; set; } = true;
}