namespace CardDemo.Application.Features.BatchJobs;

/// <summary>
/// Result of a batch job execution
/// </summary>
public class BatchJobResult
{
    public string JobId { get; set; } = default!;
    public string JobName { get; set; } = default!;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public BatchJobStatus Status { get; set; }
    public int RecordsProcessed { get; set; }
    public int RecordsSucceeded { get; set; }
    public int RecordsFailed { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? OutputFilePath { get; set; }
    
    public TimeSpan? Duration => EndTime.HasValue ? EndTime - StartTime : null;
    
    public static BatchJobResult Started(string jobName)
    {
        return new BatchJobResult
        {
            JobId = Guid.NewGuid().ToString("N")[..16].ToUpper(),
            JobName = jobName,
            StartTime = DateTime.UtcNow,
            Status = BatchJobStatus.Running
        };
    }
    
    public void Complete()
    {
        EndTime = DateTime.UtcNow;
        Status = RecordsFailed > 0 ? BatchJobStatus.CompletedWithErrors : BatchJobStatus.Completed;
    }
    
    public void Fail(string error)
    {
        EndTime = DateTime.UtcNow;
        Status = BatchJobStatus.Failed;
        Errors.Add(error);
    }
}

public enum BatchJobStatus
{
    Pending,
    Running,
    Completed,
    CompletedWithErrors,
    Failed,
    Cancelled
}
