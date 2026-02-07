using CaManager.Services;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var useMock = builder.Configuration.GetValue<bool>("UseMockKeyVault");

if (useMock)
{
#if DEBUG
    builder.Services.AddAuthentication("Mock")
        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, MockAuthHandler>("Mock", null);
#endif
}
else
{
    builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration, "AzureAd");
}

var controllersBuilder = builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

if (!useMock)
{
    controllersBuilder.AddMicrosoftIdentityUI();
}

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();

#if DEBUG
if (builder.Configuration.GetValue<bool>("UseMockKeyVault"))
{
    builder.Services.AddSingleton<IKeyVaultService, MockKeyVaultService>();
}
else
{
    builder.Services.AddSingleton<IKeyVaultService, KeyVaultService>();
}
#else
builder.Services.AddSingleton<IKeyVaultService, KeyVaultService>();
#endif

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IVersionCheckService, GithubVersionCheckService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // For Identity/API if exists
app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
