using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using MarketStatusAPI.Services;
using MarketStatusAPI.Models;

namespace MarketStatusAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class MarketStatusController : ControllerBase
	{
		private readonly IHolidayService _holidayService;
		private readonly TimeService _timeService;
		private readonly TimeZoneInfo _turkeyTimeZone;

		public MarketStatusController(IHolidayService holidayService, TimeService timeService)
		{
			_holidayService = holidayService;
			_timeService = timeService;
			_turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
		}

		[HttpGet("marketstatus")]
		public async Task<IActionResult> GetMarketStatus()
		{
			var now = await _timeService.GetCurrentTimeAsync();
			return CheckMarketStatus(now);
		}

		[HttpGet("testmarketstatus")]
		public async Task<IActionResult> TestMarketStatus(string date)
		{
			if (DateTime.TryParse(date, out DateTime testDate))
			{
				var testDateInTurkeyTime = TimeZoneInfo.ConvertTimeFromUtc(testDate.ToUniversalTime(), _turkeyTimeZone);
				return CheckMarketStatus(testDateInTurkeyTime);
			}

			return BadRequest("Invalid date format. Use YYYY-MM-DDTHH:MM:SS format.");
		}

		private IActionResult CheckMarketStatus(DateTime dateTime)
		{
			// Tatil mi kontrol et
			var holiday = IsHoliday(dateTime);
			if (holiday != null)
			{
				if (IsHalfDayHoliday(holiday.Status))
				{
					if (dateTime.Hour >= 10 && dateTime.Hour < 13)
					{
						return Ok(new { marketOpen = true, reason = "Half-day holiday, open in the morning" });
					}
					else
					{
						return Ok(new { marketOpen = false, reason = holiday.Description });
					}
				}
				else
				{
					return Ok(new { marketOpen = false, reason = holiday.Description });
				}
			}

			// Çalışma saatleri içinde mi kontrol et
			if (!IsWithinWorkingHours(dateTime))
			{
				return Ok(new { marketOpen = false, reason = "Out of working hours" });
			}

			return Ok(new { marketOpen = true });
		}

		private bool IsWithinWorkingHours(DateTime dateTime)
		{
			return dateTime.Hour >= 10 && dateTime.Hour < 18;
		}

		private Holiday IsHoliday(DateTime dateTime)
		{
			foreach (var holiday in _holidayService.GetHolidays())
			{
				if (holiday.Date.Date == dateTime.Date)
				{
					return holiday;
				}
			}
			return null;
		}

		private bool IsHalfDayHoliday(string status)
		{
			var halfDayPattern = @"(Yarım Gün Tatil)|(Saat 13:00'e kadar)";
			return Regex.IsMatch(status, halfDayPattern, RegexOptions.IgnoreCase);
		}
	}
}
