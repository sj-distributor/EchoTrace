using EchoTrace.Engines.Bases;
using EchoTrace.FilterAndMiddlewares;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<AutoResolveFilter>();
var app = builder.BuildWithEngines();
app.Run();