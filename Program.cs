using GrblSpeaker.Utilites;
using Microsoft.AspNetCore.Http.Features;
using System.Device.Gpio;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.Configure<FormOptions>(opt=>opt.MultipartBodyLengthLimit=long.MaxValue);
builder.Services.AddSignalR();
builder.Host.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<NotificationHub>("/notificationHub");
    endpoints.MapRazorPages();
    endpoints.MapControllers(); // Add this line to map API controllers
});


app.Run();