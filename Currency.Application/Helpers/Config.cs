using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Security.Claims;

namespace Currency.Application.Helpers
{
    public static class Config
    {
        public static IEnumerable<Client> Clients =>
        [
            // Frankfurter client — only for frankfurter users
            new Client
            {
                ClientId = "frankfurter",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                ClientSecrets = { new Secret("frankfurter_secret".ToSha256()) },
                AllowedScopes =
                {
                    "currency_api.frankfurter", 
                    "openid", "profile", "roles"
                },
                AccessTokenLifetime = 3600,
                AllowOfflineAccess = false
            },

            // OpenExchange client — only for openexchange users
            new Client
            {
                ClientId = "openexchange",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                ClientSecrets = { new Secret("openexchange_secret".ToSha256()) },
                AllowedScopes =
                {
                    "currency_api.openexchange",
                    "openid", "profile", "roles"
                },
                AccessTokenLifetime = 1800,
                AllowOfflineAccess = false
            }
        ];

        // Distinct scopes per provider
        public static IEnumerable<ApiScope> ApiScopes =>
        [
            new ApiScope("currency_api.frankfurter", "Currency API (Frankfurter)")
            {
                UserClaims = { "currency_provider", JwtClaimTypes.Role, "rate_limit" }
            },
            new ApiScope("currency_api.openexchange", "Currency API (OpenExchange)")
            {
                UserClaims = { "currency_provider", JwtClaimTypes.Role, "rate_limit" }
            }
        ];

        // One API resource that exposes both scopes
        public static IEnumerable<ApiResource> ApiResources =>
        [
            new ApiResource("currency_api", "Currency API")
            {
                Scopes = { "currency_api.frankfurter", "currency_api.openexchange" },
                UserClaims = { "currency_provider", JwtClaimTypes.Role, "rate_limit" }
            }
        ];

        public static IEnumerable<IdentityResource> IdentityResources =>
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource("roles", "User roles", new[] { JwtClaimTypes.Role })
        ];

        public static List<TestUser> Users =>
        [
            // OpenExchange user(s)
            new TestUser
            {
                SubjectId = "1001",
                Username = "oe_user",
                Password = "password1",
                Claims =
                [
                    new Claim("currency_provider", "openexchange"),
                    new Claim(JwtClaimTypes.Role, "User"),
                    new Claim("rate_limit", "2")
                ]
            },

            // Frankfurter user(s)
            new TestUser
            {
                SubjectId = "2001",
                Username = "ff_admin",
                Password = "password2",
                Claims =
                [
                    new Claim("currency_provider", "frankfurter"),
                    new Claim(JwtClaimTypes.Role, "Admin"),
                    new Claim("rate_limit", "5")
                ]
            }
        ];
    }
}