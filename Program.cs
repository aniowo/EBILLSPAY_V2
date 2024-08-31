using EBILLSPAY_V2;
using EBILLSPAY_V2.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.Mvc;
using EbillsPay = EBILLSPAY_V2.EbillsPay;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("GTCollection")
    ));

builder.Services.AddSession();
builder.Services.AddTransient<Utilities>();
builder.Services.AddTransient<EbillsPay>();
builder.Services.AddTransient<Authentication>();
builder.Services.AddTransient<DataBaseRepo>();

builder.Host.UseSerilog((hostingContext, loggerConfig) =>
{
    loggerConfig.ReadFrom.Configuration(hostingContext.Configuration);
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
