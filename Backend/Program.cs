using Backend.Controllers;
using Backend.Services;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

var builder = WebApplication.CreateBuilder();


builder.WebHost.UseUrls("http://localhost:7095/");
//HttpClass.Init("https://localhost:7095/");
builder.Services.AddTransient<IPowershellService, PowershellService>();
var  MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .SetIsOriginAllowed((host) => true)
                    .AllowAnyHeader());
        });
var app = builder.Build();
app.UseCors("CorsPolicy");
app.MapGet("/", async context =>
{
    await context.Response.WriteAsync("Default message");
});

app.MapGet("/User", async context =>
{
    UserController.GetInfo(context);
});
app.MapGet("/Ban", async context =>
{
    UserController.BanUser(context);
});
app.MapGet("/Unban", async context =>
{
    UserController.UnbanUser(context);
});
app.MapGet("/AddToGroup", async context =>
{
    UserController.AddToGroup(context);
});
app.MapGet("/ChangePassword", async context =>
{
    UserController.ChangePassword(context);
});
app.MapGet("/CreateMailBox", async context =>
{
    UserController.CreateMailBox(context);
});
app.MapGet("/HideMailBox", async context =>
{
    UserController.HideMailBox(context);
});
app.MapGet("/RemoveFromGroup", async context =>
{
    UserController.RemoveFromGroup(context);
});
app.MapGet("/ShowMailBox", async context =>
{
    UserController.ShowMailBox(context);
});
/*app.MapGet("/CreateUser", async context =>
{
    UserController.UserCreation(context);
});*/
app.Run();
