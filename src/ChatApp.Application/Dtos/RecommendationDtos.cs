namespace ChatApp.Application.Dtos;

public record RecommendationDto(
    Guid Id,
    Guid TargetUserId,
    double Score,
    string Reason,
    DateTime CreatedAt);
