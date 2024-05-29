using System;
using System.Threading.Tasks;
using Coravel.Invocable;
using MarketStatusAPI.Scraper;

namespace MarketStatusAPI.Jobs
{
	public class HolidayScraperJob : IInvocable
	{
		private readonly HolidayScraper _scraper;
		private readonly TimeService _timeService;

		public HolidayScraperJob(TimeService timeService)
		{
			_scraper = new HolidayScraper("https://borsaistanbul.com/tr/sayfa/143/resmi-tatil-gunleri");
			_timeService = timeService;
		}

		public async Task Invoke()
		{
			var currentTime = await _timeService.GetCurrentTimeAsync();
			Console.WriteLine($"{currentTime}: HolidayScraperJob started.");
			await _scraper.ScrapeAndSaveHolidaysAsync("holidays.csv");
			Console.WriteLine($"{currentTime}: HolidayScraperJob finished.");
		}
	}
}
