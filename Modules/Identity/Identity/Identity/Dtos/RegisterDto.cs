namespace Identity.Identity.Dtos;

public record RegisterDto(
    string Email,
    string Password,
    string DisplayName
    );