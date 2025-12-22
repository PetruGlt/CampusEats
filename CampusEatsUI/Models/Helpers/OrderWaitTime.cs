namespace CampusEatsUI.Models.Helpers;

public record OrderWaitTime(
    Guid Id,
    string CurrentStatus,
    int EstimatedWaitMinutes,
    DateTime EstimatedCompletionTime,
    string? Message);