using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class TimeService
{
	private readonly HttpClient _httpClient;

	public TimeService()
	{
		_httpClient = new HttpClient();
	}

	public async Task<DateTime> GetCurrentTimeAsync()
	{
		var response = await _httpClient.GetStringAsync("http://worldtimeapi.org/api/timezone/Europe/Istanbul");
		var json = JObject.Parse(response);
		var dateTimeString = json["datetime"].ToString();
		var dateTime = DateTime.Parse(dateTimeString);
		return dateTime;
	}
}
