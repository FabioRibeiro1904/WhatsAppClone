using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using WhatsAppClone.Data;
using WhatsAppClone.Hubs;
using WhatsAppClone.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMobileApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<WhatsAppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ??
                       "Data Source=WhatsApp.db"));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IChatService, ChatService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.Cookie.Name = "WhatsAppClone.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseCors("AllowMobileApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<WhatsAppClone.Components.App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();
app.MapHub<ChatHub>("/chathub");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<WhatsAppDbContext>();
    await context.Database.EnsureCreatedAsync();
    
    if (!await context.Users.AnyAsync())
    {
        var users = new[]
        {
            new WhatsAppClone.Models.User
            {
                Username = "admin",
                Name = "Admin",
                Email = "admin@whatsapp.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Status = "Sou o administrador do sistema!",
                IsOnline = false,
                LastSeen = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            },
            new WhatsAppClone.Models.User
            {
                Username = "alice",
                Name = "Alice Silva",
                Email = "alice@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("alice123"),
                Status = "Olá! Estou usando WhatsApp Clone.",
                IsOnline = false,
                LastSeen = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            },
            new WhatsAppClone.Models.User
            {
                Username = "bob",
                Name = "Bob Santos",
                Email = "bob@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("bob123"),
                Status = "Disponível para conversar!",
                IsOnline = false,
                LastSeen = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }
}

app.Run();