// Scraper/HolidayScraper.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CsvHelper;
using HtmlAgilityPack;
using MarketStatusAPI.Models;

namespace MarketStatusAPI.Scraper
{
	public class HolidayScraper
	{
		private readonly string _url;

		public HolidayScraper(string url)
		{
			_url = url;
		}

		public async Task ScrapeAndSaveHolidaysAsync(string filePath)
		{
			using HttpClient client = new HttpClient();
			string pageContent = await client.GetStringAsync(_url);

			HtmlDocument document = new HtmlDocument();
			document.LoadHtml(pageContent);

			var holidayNodes = document.DocumentNode.SelectNodes("//div[@class='table-responsive responsiveMobileTable']//table//tbody//tr");
			var holidays = new List<Holiday>();

			if (holidayNodes != null)
			{
				foreach (var node in holidayNodes)
				{
					var columns = node.SelectNodes("td");

					if (columns != null && columns.Count > 1)
					{
						string dateText = HtmlEntity.DeEntitize(columns[0].InnerText.Trim());
						string description = HtmlEntity.DeEntitize(columns[1].InnerText.Trim());
						string status = HtmlEntity.DeEntitize(columns[2].InnerText.Trim());

						var dates = ParseDates(dateText);
						foreach (var date in dates)
						{
							if (date.Year >= 2024)
							{
								holidays.Add(new Holiday { Date = date, Description = description, Status = status });
							}
						}
					}
				}

				SaveHolidaysToCsv(holidays, filePath);
			}
		}

		private List<DateTime> ParseDates(string dateText)
		{
			var dates = new List<DateTime>();
			var dateRanges = dateText.Split(',');

			foreach (var dateRange in dateRanges)
			{
				try
				{
					if (dateRange.Contains('-'))
					{
						var rangeParts = dateRange.Split('-');
						var startDate = DateTime.Parse(rangeParts[0].Trim(), new CultureInfo("tr-TR"));
						var endDate = DateTime.Parse(rangeParts[1].Trim(), new CultureInfo("tr-TR"));

						for (var date = startDate; date <= endDate; date = date.AddDays(1))
						{
							dates.Add(date);
						}
					}
					else
					{
						var trimmedDate = dateRange.Trim();
						if (!IsDayName(trimmedDate))
						{
							dates.Add(DateTime.Parse(trimmedDate, new CultureInfo("tr-TR")));
						}
					}
				}
				catch (FormatException)
				{
					// Skip invalid date formats
					continue;
				}
			}

			return dates;
		}

		private bool IsDayName(string text)
		{
			var dayNames = new[] { "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi", "Pazar" };
			return Array.Exists(dayNames, day => day.Equals(text, StringComparison.OrdinalIgnoreCase));
		}

		private void SaveHolidaysToCsv(List<Holiday> holidays, string filePath)
		{
			using (var writer = new StreamWriter(filePath))
			using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
			{
				csv.WriteRecords(holidays);
			}
		}
	}
}
