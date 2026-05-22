namespace TudfConverter.Application.Configuration;

public class TudfAppSettings
{
    public string MemberUserId { get; set; } = string.Empty;
    public string MemberShortName { get; set; } = string.Empty;
    public string ReportingCycle { get; set; } = string.Empty;
    public string OutputFolder { get; set; } = string.Empty;
    public string ReportFolder { get; set; } = string.Empty;
    public string LogFolder { get; set; } = string.Empty;
    public bool AutoCleanOldFiles { get; set; } = true;
    public int RetentionDays { get; set; } = 90;
}
