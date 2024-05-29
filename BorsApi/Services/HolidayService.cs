using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using MarketStatusAPI.Models;

namespace MarketStatusAPI.Services
{
	public interface IHolidayService
	{
		List<Holiday> GetHolidays();
	}

	public class HolidayService : IHolidayService
	{
		private readonly List<Holiday> _holidays;

		public HolidayService()
		{
			_holidays = LoadHolidays();
		}

		private List<Holiday> LoadHolidays()
		{
			if (!File.Exists("holidays.csv"))
			{
				return new List<Holiday>();
			}

			using (var reader = new StreamReader("holidays.csv"))
			using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
			{
				return new List<Holiday>(csv.GetRecords<Holiday>());
			}
		}

		public List<Holiday> GetHolidays()
		{
			return _holidays;
		}
	}
}
