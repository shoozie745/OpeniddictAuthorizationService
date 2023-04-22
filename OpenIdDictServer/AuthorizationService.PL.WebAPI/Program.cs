using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using AuthorizationService.DAL.Database;
using AuthorizationService.DAL.Models;
using AuthorizationService.PL.WebAPI.Endpoints.AccountEndpoints;
using AuthorizationService.PL.WebAPI.Extensions.Builder.Identity;
using AuthorizationService.PL.WebAPI.Extensions.Builder.OpenIdDict;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//Db
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
string migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name!;
builder.Services.AddDbContext<AuthorizationServiceDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.UseOpenIddict();

});

builder.Services.AddIdentity<User, Role>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = false;
    })
    .AddSignInManager()
    .AddEntityFrameworkStores<AuthorizationServiceDbContext>()
    .AddUserManager<UserManager<User>>()
    .AddClaimsPrincipalFactory<UserClaimsFactory>()
    .AddDefaultTokenProviders();
//
builder.Services.AddSingleton<IAuthorizationHandler, OpenIdAuthorizaHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, OpenIdPolicyProvider>();
//
builder.Services.AddOpenIddict()

    // Register the OpenIddict core components.
    .AddCore(options =>
    {
        // Configure OpenIddict to use the EF Core stores/models.
        options.UseEntityFrameworkCore()
            .UseDbContext<AuthorizationServiceDbContext>();
    })

    // Register the OpenIddict server components.
    .AddServer(options =>
    {
        options
            .AllowClientCredentialsFlow()
            .AllowAuthorizationCodeFlow()
            .RequireProofKeyForCodeExchange()
            .AllowRefreshTokenFlow();

        options
            .SetTokenEndpointUris("/connect/token")
            .SetAuthorizationEndpointUris("/connect/authorize")
            .SetUserinfoEndpointUris("/connect/userinfo");

        // Encryption and signing of tokens
        options
            .AddEphemeralEncryptionKey()
            .AddEphemeralSigningKey()
            .DisableAccessTokenEncryption();

        // Register scopes (permissions)
        options.RegisterScopes("api");

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        options
            .UseAspNetCore()
            .EnableTokenEndpointPassthrough()
            .EnableAuthorizationEndpointPassthrough()
            .EnableUserinfoEndpointPassthrough();            
    });


builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/connect/login";
    });

//
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Swagger description
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NEEDS TO BE CHANGED",
        Version = "NEEDS TO BE CHANGED",
        Description = "NEEDS TO BE CHANGED",
    });

    options.ResolveConflictingActions(x => x.First());

    // Controllers titles
    var identityConfiguration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .Build();

    var url = identityConfiguration.GetSection("IdentityServerUrl").GetValue<string>("Authority");

    // Get scopes for AddSecurityDefinition 

    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Flows = new OpenApiOAuthFlows
            {
                /*AuthorizationCode = new OpenApiOAuthFlow
                {
                    TokenUrl = new Uri($"{url}/connect/token", UriKind.Absolute),
                    AuthorizationUrl = new Uri($"{url}/connect/authorize", UriKind.Absolute),
                    Scopes = scopes,
                },
                ClientCredentials = new OpenApiOAuthFlow
                {
                    Scopes = scopes,
                    TokenUrl = new Uri($"{url}/connect/token", UriKind.Absolute),
                },*/
                Password = new OpenApiOAuthFlow
                {
                    TokenUrl = new Uri($"{url}/connect/token", UriKind.Absolute),
                }
            },
            Type = SecuritySchemeType.OAuth2
        }
    );
});


//
builder.Services.AddAuthorization();
builder.Services.AddCors();
var app = builder.Build();

app.UseCors(opts => opts.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.UseAuthentication();

app.UseSwagger();
var accountEndpoindsManager = new AccountEndpoints();
accountEndpoindsManager.RegisterApi(app);
app.UseSwaggerUI();

using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetService<AuthorizationServiceDbContext>()!;
var adminUserSettings = builder.Configuration.GetSection("AdminUser").Get<AdminUser>();
//var adminUser = JsonConvert.DeserializeObject<AdminUser>(adminUserSettings);
await new DatabaseInitializer(app.Services, context).Seed(adminUserSettings);


app.Run();


