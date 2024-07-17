using System;
using System.Collections.Generic;
using Core.Entities;
using Serilog;
using System.Threading.Tasks;
using Core.Data.RequestServices;
using Core.RequestsHTTP.Models.ApiMiCorreo;
using System.Diagnostics;

using AgencyList = System.Collections.Generic.List<Core.Entities.BranchOffice>;
using AgenciesByPostalCode = System.Collections.Generic.Dictionary<short, System.Collections.Generic.List<Core.Entities.BranchOffice>>;
using Provinces = System.Collections.Generic.Dictionary<char, System.Collections.Generic.Dictionary<short, System.Collections.Generic.List<Core.Entities.BranchOffice>>>;

namespace Core.V1
{

    public interface IAgencies
    {
        Task<IEnumerable<BranchOffice>> findAgenciesAsync(string province, string spostalcode, string seller_customerId);
    }


    public class Agencies : IAgencies
    {
        private readonly IApiMiCorreo correoArgentinoRequestService;
        private readonly ILogger logger;



        class update_time
        {
            public update_time(ushort hour, ushort min)
            {
                this.hour = hour;
                this.min = min;
            }

            public ushort hour { get; set; }
            public ushort min { get; set; }
        }
        private static List<update_time> _update_time;


        private static Object _lock = new Object();
        private static DateTime _lastClear = DateTime.Now;
        private static Provinces _provinces = new Provinces();


        public static int get_provinces_in_memory()
        {
            return _provinces.Count;
        }

        public static int get_branchoffice_in_memory()
        {
            int broff = 0;
            foreach (var pair_agencies in _provinces)
            {
                foreach (var pair_agency in pair_agencies.Value)
                    broff += pair_agency.Value.Count;
            }
            return broff;
        }

        public static DateTime get_last_clear_provinces()
        {
            return _lastClear;
        }



        public Agencies(
            IApiMiCorreo correoArgentinoRequestService,
            ILogger logger)
        {
            this.correoArgentinoRequestService = correoArgentinoRequestService ?? throw new ArgumentNullException(nameof(correoArgentinoRequestService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (_update_time == null)
            {
                _update_time = new List<update_time>();
                _update_time.Add(new update_time(3, 0));
                _update_time.Add(new update_time(9, 45));
                _update_time.Add(new update_time(16, 30));
            }
        }



        private bool clear_if_need()
        {
            DateTime now = DateTime.Now;
            foreach (update_time udtm in _update_time)
            {
                DateTime dtu = new DateTime(now.Year, now.Month, now.Day, udtm.hour, udtm.min, 0);
                if (_lastClear < dtu && dtu < now)
                {
                    logger.Information("Agencies - Clear all provinces/agencies on memory for update!");

                    foreach (var province in _provinces)
                    {
                        foreach (var cp in province.Value.Values)
                            cp.Clear();
                        province.Value.Clear();
                    }
                    _provinces.Clear();
                    _lastClear = now;
                    return true;
                }
            }
            return false;
        }



        public async Task<IEnumerable<BranchOffice>> findAgenciesAsync(string province, string spostalcode, string seller_customerId)
        {
            Stopwatch te = new Stopwatch();
            te.Start();
            logger.Information("Start service: Agencies...");

            lock (_lock)
                clear_if_need();

            char provinceCode = Shared.Province.GetProvince(province);
            List<BranchOffice> rBranchOffice = null;
            AgenciesByPostalCode broff = null;
            broff = _provinces.GetValueOrDefault(provinceCode);

            short postalCode = BranchOffice.normalizePostalCode(spostalcode);

            if (broff == null)
            {
                var model = new GetAgencyModel()
                {
                    customerId = seller_customerId,
                    services = "pickup_availability",
                    provinceCode = provinceCode
                };

                try
                {
                    IEnumerable<AgencyModel> agencies = await this.correoArgentinoRequestService.Agencies("Agencies", model);

                    if (agencies == null)
                    {
                        logger.Error($"Agencies - Can't find branches office for clientId: {model.customerId}; services: {model.services}; provinciaCode: {model.provinceCode}");
                        return null;
                    }

                    broff = new AgenciesByPostalCode();

                    foreach (var agency in agencies)
                    {
                        if (!string.IsNullOrEmpty(agency.status) && agency.status.ToUpper() == "ACTIVE")
                        {
                            BranchOffice newBrOff = new BranchOffice(agency);
                            if (!broff.ContainsKey(newBrOff.postalCode))
                                broff[newBrOff.postalCode] = new AgencyList();
                            broff[newBrOff.postalCode].Add(newBrOff);
                        }
                    }
                    lock (_lock)
                        _provinces[provinceCode] = broff;
                }
                catch (Exception ex)
                {
                    logger.Error("Agencies - Exception connecting to ApiMiCorreo/agencies: " + ex.Message);
                    return null;
                }
            }
            lock (_lock)
                rBranchOffice = broff.GetValueOrDefault(postalCode);

            te.Stop();
            if (rBranchOffice != null)
                logger.Information($"End service Agencies [{provinceCode}]: {rBranchOffice.Count} branch offices, elapsed time {te.ElapsedMilliseconds}ms");
            else
                logger.Information($"End service Agencies [{provinceCode}]: <NULL> branch offices, elapsed time {te.ElapsedMilliseconds}ms");

            return rBranchOffice;
        }

    }
}
