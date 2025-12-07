namespace CampusEatsUI.Models.DTOs;

public record OrderWaitTimeResponse(
    Guid OrderId,
    string CurrentStatus,
    int EstimatedWaitMinutes,
    DateTime? EstimatedCompletionTime,
    string Message
    );