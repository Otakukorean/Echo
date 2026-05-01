namespace Identity.Identity.Dtos;

public record UserMeDto(
    Guid Id,
    string Email,
    string DisplayName,
    bool IsActive,
    bool EmailConfirmed,
    string Role,
    Guid? StoreId ,
    List<SessionDto> Sessions
);
