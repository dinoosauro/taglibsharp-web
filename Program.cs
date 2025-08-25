using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MetadataChange;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components.Components.Tooltip;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<ITooltipService, TooltipService>();
builder.Services.AddSingleton<LibraryConfiguration>();
builder.Services.AddSingleton<IKeyCodeService, KeyCodeService>();
builder.Services.AddSingleton<IToastService, ToastService>();
builder.Services.AddFluentUIComponents();

await builder.Build().RunAsync();
