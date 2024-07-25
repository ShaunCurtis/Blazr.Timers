namespace Blazr.Timers;

public record StatusData(string Name, DateTimeOffset Timestamp);

public class StatusService
{
    private List<StatusData> statusData = new List<StatusData>()
    {
        new("France",  DateTimeOffset.Now.AddDays(-3)),
        new("Spain",  DateTimeOffset.Now.AddDays(-2)),
        new("Portugal",  DateTimeOffset.Now.AddDays(-1)),
    };

    public async ValueTask<IEnumerable<StatusData>> GetStatus()
    {
        statusData.Add(new("Me",  DateTimeOffset.Now));
        await Task.Yield();
        return statusData.AsEnumerable();
    }
}
