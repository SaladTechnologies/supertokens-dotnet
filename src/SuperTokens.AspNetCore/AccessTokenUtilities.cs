using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace SuperTokens.AspNetCore
{
    internal static class AccessTokenUtilities
    {
        public static bool TryParse(string jwtPayload, [NotNullWhen(true)] out AccessToken? accessToken)
        {
            using var document = JsonDocument.Parse(jwtPayload);
            if (!document.RootElement.TryGetProperty("sessionHandle", out var sessionHandleProperty) || sessionHandleProperty.ValueKind != JsonValueKind.String ||
                !document.RootElement.TryGetProperty("userId", out var userIdProperty) || userIdProperty.ValueKind != JsonValueKind.String ||
                !document.RootElement.TryGetProperty("refreshTokenHash1", out var refreshTokenHash1Property) || refreshTokenHash1Property.ValueKind != JsonValueKind.String ||
                (document.RootElement.TryGetProperty("parentRefreshTokenHash1", out var parentRefreshTokenHash1Property) && parentRefreshTokenHash1Property.ValueKind != JsonValueKind.String) ||
                !document.RootElement.TryGetProperty("userData", out var userDataProperty) || userDataProperty.ValueKind != JsonValueKind.Object ||
                (document.RootElement.TryGetProperty("antiCsrfToken", out var antiCsrfTokenProperty) && antiCsrfTokenProperty.ValueKind != JsonValueKind.String) ||
                !document.RootElement.TryGetProperty("expiryTime", out var expiryTimeProperty) || expiryTimeProperty.ValueKind != JsonValueKind.Number ||
                !document.RootElement.TryGetProperty("timeCreated", out var timeCreatedProperty) || timeCreatedProperty.ValueKind != JsonValueKind.Number)
            {
                accessToken = null;
                return false;
            }

            try
            {
                accessToken = new AccessToken(
                    antiCsrfTokenProperty.ValueKind == JsonValueKind.Undefined ? null : antiCsrfTokenProperty.GetString()!.Trim(),
                    DateTimeOffset.FromUnixTimeMilliseconds(expiryTimeProperty.GetInt64()),
                    parentRefreshTokenHash1Property.ValueKind == JsonValueKind.Undefined ? null : parentRefreshTokenHash1Property.GetString()!.Trim(),
                    refreshTokenHash1Property.GetString()!.Trim(),
                    sessionHandleProperty.GetString()!.Trim(),
                    DateTimeOffset.FromUnixTimeMilliseconds(timeCreatedProperty.GetInt64()),
                    userDataProperty.GetRawText().Trim(),
                    userIdProperty.GetString()!.Trim());
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                accessToken = null;
                return false;
            }
            catch (FormatException)
            {
                accessToken = null;
                return false;
            }
        }
    }
}
