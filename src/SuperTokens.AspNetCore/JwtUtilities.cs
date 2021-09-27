using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace SuperTokens.AspNetCore
{
    internal static class JwtUtilities
    {
        /// <summary>
        ///     A base64 encoded representation of the following JWT header:
        ///     <code>
        ///{"alg":"RS256","typ":"JWT","version":"1"}
        ///     </code>
        /// </summary>
        private const string VersionOneHeader = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsInZlcnNpb24iOiIxIn0=";

        /// <summary>
        ///     A base64 encoded representation of the following JWT header:
        ///     <code>
        ///{"alg":"RS256","typ":"JWT","version":"2"}
        ///     </code>
        /// </summary>
        private const string VersionTwoHeader = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsInZlcnNpb24iOiIyIn0=";

        public static bool TryParse(string jwt, [NotNullWhen(true)] out string? jwtPayload, out string[]? components)
        {
            // Parse JWT.
            components = jwt.Split('.');
            if (components.Length != 3 || components[1].Length == 0)
            {
                jwtPayload = null;
                components = null;
                return false;
            }

            // Verify JWT header.
            if (!VersionOneHeader.Equals(components[0], StringComparison.Ordinal) && !VersionTwoHeader.Equals(components[0], StringComparison.Ordinal))
            {
                jwtPayload = null;
                components = null;
                return false;
            }

            // Verify JWT payload.
            var buffer = new Span<byte>(new byte[components[1].Length]);
            if (!Convert.TryFromBase64String(components[1], buffer, out var bufferLength))
            {
                jwtPayload = null;
                components = null;
                return false;
            }

            jwtPayload = Encoding.UTF8.GetString(buffer.Slice(0, bufferLength));
            return true;
        }

        public static bool TryParseAndValidate(string jwt, string jwtSigningPublicKey, [NotNullWhen(true)] out string? jwtPayload, out bool isSignatureValid)
        {
            // Parse JWT.
            var components = jwt.Split('.');
            if (components.Length != 3 || components[1].Length == 0)
            {
                jwtPayload = null;
                isSignatureValid = false;
                return false;
            }

            // Verify JWT header.
            if (!VersionOneHeader.Equals(components[0], StringComparison.Ordinal) && !VersionTwoHeader.Equals(components[0], StringComparison.Ordinal))
            {
                jwtPayload = null;
                isSignatureValid = false;
                return false;
            }

            // Verify JWT payload.
            var buffer = new Span<byte>(new byte[components[1].Length]);
            if (!Convert.TryFromBase64String(components[1], buffer, out var bufferLength))
            {
                jwtPayload = null;
                isSignatureValid = false;
                return false;
            }

            jwtPayload = Encoding.UTF8.GetString(buffer.Slice(0, bufferLength));
            isSignatureValid = Validate(components, jwtSigningPublicKey);
            return true;
        }

        public static bool Validate(string[] components, string jwtSigningPublicKey)
        {
            // Verify JWT signature.
            var rsa = RSA.Create();
            var key = Convert.FromBase64String(jwtSigningPublicKey);
            rsa.ImportSubjectPublicKeyInfo(key, out _);

            var data = Encoding.UTF8.GetBytes($"{components[0]}.{components[1]}");
            var signature = Convert.FromBase64String(components[2]);
            return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }
}
