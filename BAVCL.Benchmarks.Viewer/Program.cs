using BAVCL.Benchmarks.Viewer.Components;
using BAVCL.Benchmarks.Viewer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

builder.Services.AddSingleton<BenchmarkDataStore>();
builder.Services.AddSingleton<BenchmarkCsvLoader>();
builder.Services.AddSingleton<BenchmarkPlotService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.Run();
