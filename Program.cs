using AspNetCoreHero.ToastNotification;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoppingFood.Areas.Admin.Repository;
using ShoppingFood.Hubs;
using ShoppingFood.Models;
using ShoppingFood.Models.Momo;
using ShoppingFood.Repository;
using ShoppingFood.Services.Momo;
using ShoppingFood.Services.Paypal;
using ShoppingFood.Services.Vnpay;

var builder = WebApplication.CreateBuilder(args);

// Connect momo api
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Email Sender
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IOTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});

builder.Services.AddNotyf(config =>
{
    config.DurationInSeconds = 3;
    config.IsDismissable = true;
    config.Position = NotyfPosition.TopRight;
});

builder.Services.AddIdentity<AppUserModel, IdentityRole>().AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});

//Configuration login google account
builder.Services.AddAuthentication(options =>
{
    //options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    //options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    //options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie("ClientScheme", options =>
{
    options.LoginPath = "/account/login"; // Trang đăng nhập client
    options.AccessDeniedPath = "/client/accessdenied";
})
.AddCookie("AdminScheme", options =>
{
    options.LoginPath = "/admin/account/login"; // Trang đăng nhập admin
    options.AccessDeniedPath = "/admin/accessdenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
}).AddCookie().AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
    options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.WithOrigins("https://localhost:7234")
                .AllowAnyHeader()
                .WithMethods("GET", "POST")
                .AllowCredentials();
        });
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

//Add SignalR
builder.Services.AddSignalR();

// Connect vnpay api
builder.Services.AddScoped<IVnPayService, VnPayService>();

// conntect paypal api
builder.Services.AddSingleton(x =>
    new PaypalClient(
        builder.Configuration["Paypal:ClientId"],
        builder.Configuration["Paypal:ClientSecret"],
        builder.Configuration["Paypal:Mode"]
    )
);

var app = builder.Build();


app.UseStatusCodePagesWithRedirects("/Home/Error?statuscode={0}");

app.UseSession();

app.UseCookiePolicy();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseWebSockets();
// UseCors must be called before MapHub.
app.UseCors();
//Set Signal Hub
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// UseCors must be called before MapHub.
app.MapHub<ChatHub>("/chathub");

app.Run();
