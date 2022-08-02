using B1TestTask1;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddMvc();

builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache();// добавляем IDistributedMemoryCache
builder.Services.AddSession();
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});


var app = builder.Build();

app.UseSession();
app.UseHttpsRedirection();

app.UseRouting();
app.UseDeveloperExceptionPage();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(name: "default", pattern: "{controller=Main}/{action=Index}/{id?}");
});

app.Run();
