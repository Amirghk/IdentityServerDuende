using System.Collections.Generic;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using IdentityModel;

namespace IdentityServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        // list of OIDC scopes that are supported
        new IdentityResource[]
        {
            // subject id
            new IdentityResources.OpenId(),
            // first name and last name and ... scopes
            new IdentityResources.Profile(),
            // custom identity resource
            new IdentityResource() {
                // name of the scope
                Name = "verification",
                // user claims that would be returned
                UserClaims = new List<string>
                {
                    JwtClaimTypes.Email,
                    JwtClaimTypes.EmailVerified
                }
            }

        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
            {
                // developers will see the name and users will see the display name
                new ApiScope(name: "api1", displayName: "MyAPI")
            };

    public static IEnumerable<Client> Clients =>
        new Client[]
            {
                // machine to machine client
                new Client
                {
                    ClientId = "client",
                    // secret for authentication
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    // no interactive user, use the clientId/secret for authentication
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    // list of scopes that client is allowed to ask for
                    AllowedScopes = {"api1"}
                },
                // interactive ASP.NET Core Web App
                new Client {
                    ClientId = "web",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,

                    // since the flows in OIDC are always interactive, we need to add some redirect URLs to our configuration.
                    // where to redirect to after login
                    RedirectUris = { "https://localhost:5002/signin-oidc"},

                    // where to redirect to after logout
                    PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc"},

                    // list of scopes that client is allowed to ask for
                    AllowedScopes = new List<string> {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "verification"
                    }
                }
             };
}