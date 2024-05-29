using MarketStatusAPI.Services;
using MarketStatusAPI.Scraper;
using MarketStatusAPI.Jobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Coravel;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSingleton<IHolidayService, HolidayService>();
builder.Services.AddSingleton<TimeService>(); // TimeService'i ekleyin

// Add Coravel services
builder.Services.AddScheduler();
builder.Services.AddTransient<HolidayScraperJob>();

// Add Swagger services
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "Market Status API", Version = "v1" });
});

var app = builder.Build();

// Scrape holiday data and save to CSV initially if not exists
if (!File.Exists("holidays.csv"))
{
	var scraper = new HolidayScraper("https://borsaistanbul.com/tr/sayfa/143/resmi-tatil-gunleri");
	await scraper.ScrapeAndSaveHolidaysAsync("holidays.csv");
}

// Schedule the job to run every 24 hours
app.Services.UseScheduler(scheduler =>
{
	scheduler.Schedule<HolidayScraperJob>().DailyAt(0, 0); // Schedule job to run at midnight every day
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "Market Status API v1");
	});
}

app.UseRouting();
app.UseEndpoints(endpoints =>
{
	endpoints.MapControllers();
});

app.Run();
