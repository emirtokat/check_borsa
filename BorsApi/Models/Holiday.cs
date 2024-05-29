// Models/Holiday.cs
using System;

namespace MarketStatusAPI.Models
{
	public class Holiday
	{
		public DateTime Date { get; set; }
		public string Description { get; set; }
		public string Status { get; set; }
	}
}
