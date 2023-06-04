using Backend.Controllers;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

var builder = WebApplication.CreateBuilder();

var app = builder.Build();

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

app.Run();
