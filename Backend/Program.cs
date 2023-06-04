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
    using (var ps = PowerShell.Create())
    {
        InitialSessionState iss = InitialSessionState.CreateDefault();

        string scriptText = File.ReadAllText("./PowershellFunctions/GetUserInfo.ps1");
        var results = ps.AddScript(scriptText).AddParameter("UserLogin", context.Request.Query["UserLogin"]).Invoke();
        string final = "";
        foreach (var result in results)
        {

            final += result.ToString();
        }
        await context.Response.WriteAsync(final);
    }
});

app.Run();
