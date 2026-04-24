namespace Identity.Identity.Extensions;

public static class CookieExtensions
{
    public static void SetRefreshTokenCookie(this HttpResponse response, string refreshToken, DateTime expires)
    {
        response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/auth",
            Expires = expires
        });
    }
}
