using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.RequestsHTTP.Models.ApiMiCorreo;
using Core.Shared;
using Core.V1.TiendaNube.GetBranchOffices.Models;

namespace Core.Entities
{
    public class BranchOffice
    {
        public BranchOffice(AgencyModel agency)
        {
            this.postalCode = normalizePostalCode(agency.location.address.postalCode);
            if (!string.IsNullOrEmpty(agency.location.address.postalCode))
                this.spostalCode = agency.location.address.postalCode.Trim();
            if (!string.IsNullOrEmpty(agency.name))
                this.name = agency.name.Trim();
            if (!string.IsNullOrEmpty(agency.code))
                this.code = agency.code.Trim();
            if (!string.IsNullOrEmpty(agency.location.address.streetName))
                this.streetName = agency.location.address.streetName.Trim();
            if (!string.IsNullOrEmpty(agency.location.address.streetNumber))
                this.streetNumber = agency.location.address.streetNumber.Trim();
            if (!string.IsNullOrEmpty(agency.location.address.city))
                this.city = agency.location.address.city.Trim();
            if (!string.IsNullOrEmpty(agency.location.longitude))
                this.longitude = agency.location.longitude.Trim();
            if (!string.IsNullOrEmpty(agency.location.latitude))
                this.latitude = agency.location.latitude.Trim();
            if (!string.IsNullOrEmpty(agency.location.address.locality))
                this.locality = agency.location.address.locality.Trim();
            if (!string.IsNullOrEmpty(agency.phone))
                this.phone = agency.phone.Trim();
            if (!string.IsNullOrEmpty(agency.location.address.province))
                this.province = agency.location.address.province.Trim();

            if (agency.hours != null)
            {
                schedules = new List<HourModel>();
                setSchedules(agency.hours);
            }
        }


        private void setSchedules(WeekDays hours)
        {
            if (hours.sunday != null)
                schedules.Add(new HourModel() { day = 0, start = hours.sunday.start,    end = hours.sunday.end });
            if (hours.monday != null)
                schedules.Add(new HourModel() { day = 1, start = hours.monday.start,    end = hours.monday.end });
            if (hours.tuesday != null)
                schedules.Add(new HourModel() { day = 2, start = hours.tuesday.start,   end = hours.tuesday.end });
            if (hours.wednesday != null)
                schedules.Add(new HourModel() { day = 3, start = hours.wednesday.start, end = hours.wednesday.end });
            if (hours.thursday != null)
                schedules.Add(new HourModel() { day = 4, start = hours.thursday.start,  end = hours.thursday.end });
            if (hours.friday != null)
                schedules.Add(new HourModel() { day = 5, start = hours.friday.start,    end = hours.friday.end });
            if (hours.saturday != null)
                schedules.Add(new HourModel() { day = 6, start = hours.saturday.start,  end = hours.saturday.end });
        }


        public static short normalizePostalCode(string unzipp)
        {
            if (string.IsNullOrEmpty(unzipp))
                return 0;
            short nzipp = 0;
            for (int i = 0; i < unzipp.Length; ++i)
            {
                if (Char.IsDigit(unzipp[i]))
                {
                    nzipp *= 10;
                    nzipp += (short)(unzipp[i] - '0');
                }
            }
            return nzipp;
        }


        public short postalCode { get; set; }
        public string spostalCode { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public string streetName { get; set; }
        public string streetNumber { get; set; }
        public string city { get; set; }
        public string longitude { get; set; }
        public string latitude { get; set; }
        public string locality { get; set; }
        public string phone { get; set; }
        public string province { get; set; }
        public List<HourModel> schedules { get; }
    }
}
