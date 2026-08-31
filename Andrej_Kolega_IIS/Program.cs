using System.Text;
using Andrej_Kolega_IIS.Backend.CustomApi.GraphQL;
using Andrej_Kolega_IIS.Backend.CustomApi.Jwt;
using Andrej_Kolega_IIS.Backend.Grpc;
using Andrej_Kolega_IIS.Backend.RestApi.Validation;
using Andrej_Kolega_IIS.Backend.Soap;
using Andrej_Kolega_IIS.Shared.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SoapCore;

// Required for the frontend's gRPC client to call the backend over plain HTTP (no TLS) in dev.
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

// Kestrel cannot multiplex HTTP/1.1 and HTTP/2 on the same cleartext (non-TLS) port, since that
// requires TLS ALPN negotiation. So gRPC gets its own HTTP/2-only port alongside the regular one.
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5183, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });
    options.ListenLocalhost(5184, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient("BackendApi", client =>
{
    var baseUrl = builder.Configuration["BackendApi:BaseUrl"] ?? "http://localhost:5183";
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddSingleton<OrderXmlValidator>();
builder.Services.AddSingleton<OrderJsonValidator>();

builder.Services.AddHttpClient<FirebaseOrdersClient>(client =>
{
    var baseUrl = builder.Configuration["Firebase:BaseUrl"]
        ?? "https://iis-firebase-default-rtdb.europe-west1.firebasedatabase.app/";
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddScoped<OrdersXmlGenerator>();
builder.Services.AddScoped<IOrdersSoapService, OrdersSoapService>();
builder.Services.AddSoapCore();

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<DhmzWeatherClient>(client =>
{
    var baseUrl = builder.Configuration["Dhmz:BaseUrl"] ?? "https://vrijeme.hr/";
    client.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddGrpc();

builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Clear();
    options.ViewLocationFormats.Add("/Frontend/Views/{1}/{0}.cshtml");
    options.ViewLocationFormats.Add("/Frontend/Views/Shared/{0}.cshtml");
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddHttpContextAccessor();
builder.Services
    .AddGraphQLServer()
    .AddQueryType<OrdersQuery>()
    .AddMutationType<OrdersMutation>();

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("GraphQLAuth", policy =>
    {
        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    });
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
    DbSeeder.SeedUsers(context);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

((IApplicationBuilder)app).UseSoapEndpoint<IOrdersSoapService>("/soap/orders", new SoapEncoderOptions(), SoapSerializer.DataContractSerializer);

app.MapGrpcService<WeatherGrpcService>();

app.MapGraphQL().RequireAuthorization("GraphQLAuth");

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
