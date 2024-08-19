using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MetadataChange;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<LibraryConfiguration>();
builder.Services.AddSingleton<IKeyCodeService, KeyCodeService>();
builder.Services.AddSingleton<IToastService, ToastService>();
builder.Services.AddFluentUIComponents();

await builder.Build().RunAsync();
