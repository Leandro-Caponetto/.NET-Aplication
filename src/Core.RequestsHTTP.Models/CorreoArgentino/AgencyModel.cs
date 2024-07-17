using Core.RequestsHTTP.Models.ApiMiCorreo.RegisterSendModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.RequestsHTTP.Models.ApiMiCorreo
{

	public class AgencyService
    {
		public bool packageReception { get; set; }
		public bool pickupAvailability { get; set; }
	}


	public class AddressAgency : AddressBase
	{
		public string province { get; set; }
	}


	public class Location
	{
		public AddressAgency address { get; set; }
		public string latitude { get; set; }
		public string longitude { get; set; }
	}


	public class DailyHours
    {
		public string start { get; set; }
		public string end { get; set; }
	}


	public class WeekDays
    {
		public DailyHours sunday { get; set; }
		public DailyHours monday { get; set; }
		public DailyHours tuesday { get; set; }
		public DailyHours wednesday { get; set; }
		public DailyHours thursday { get; set; }
		public DailyHours friday { get; set; }
		public DailyHours saturday { get; set; }
		public DailyHours holidays { get; set; }
	}


	public class AgencyModel
	{
		public string code { get; set; }
		public string name { get; set; }
		public string manager { get; set; }
		public string email { get; set; }
		public string phone { get; set; }
		public AgencyService services { get; set; }
		public Location location { get; set; }
		public WeekDays hours { get; set; }
		public string status { get; set; }
    }
}
