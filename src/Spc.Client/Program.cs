using Spc.Client.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Razor Pages
builder.Services.AddRazorPages();

// Session configuration (for Cart & PharmacyID)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(2);
});

// Bind API options
builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection("ApiOptions"));

// Register Typed HTTP Client
builder.Services.AddHttpClient<SpcApiClient>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Enable Session middleware

app.MapRazorPages();

app.Run();