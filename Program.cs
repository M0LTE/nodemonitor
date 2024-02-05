using nodemonitor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHostedService<MqttListener>();
builder.Services.AddLogging(builder =>
{
    builder.AddFilter("Microsoft", LogLevel.Warning)
           .AddFilter("System", LogLevel.Warning)
           .AddSimpleConsole(options =>
           {
               options.SingleLine = true;
               options.IncludeScopes = false;
           });
});
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapHub<NodeHub>("/nodeHub");

app.Run();
