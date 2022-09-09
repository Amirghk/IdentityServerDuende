using System.Buffers;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

builder.Services.AddAuthentication(options =>
{
    // After the user has logged in and been redirected back to the client,
    // the client creates its own local cookie.
    // Subsequent requests to the client will include this cookie and
    // be authenticated with the default Cookie scheme.
    options.DefaultScheme = "Cookies";
    // The DefaultChallengeScheme is used when an unauthenticated user must log in.
    // This begins the OpenID Connect protocol, redirecting the user to IdentityServer.
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie("Cookies")
// This uses the authorization code flow with PKCE to connect to the OpenID Connect provider
.AddOpenIdConnect("oidc", options =>
{
    // The Authority indicates where the trusted token service is located.
    options.Authority = "https://localhost:5001";

    options.ClientId = "web";
    options.ClientSecret = "secret";
    options.ResponseType = "code";

    // the scopes to request
    // By default it includes the openid and profile scopes. (added them again for clarity)
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("verification");
    options.ClaimActions.MapJsonKey("email_verified", "email_verified");

    // SaveTokens is used to persist the tokens in the cookie (as they will be needed later).
    options.SaveTokens = true;
    // gets additional info using ID token? from userInfo endpoint
    options.GetClaimsFromUserInfoEndpoint = true;
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages().RequireAuthorization();

app.Run();
