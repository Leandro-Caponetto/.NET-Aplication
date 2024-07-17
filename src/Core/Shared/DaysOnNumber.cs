using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Shared
{
    public static class DaysOnNumber
    {
        public static Dictionary<string, int> ListDaysWithNumber { get; set; } = new Dictionary<string, int>
        {
            {
                "Domingo", 0
            },
            {
                "Lunes", 1
            },
            {
                "Martes", 2
            },
            {
                "Miercoles", 3
            },
            {
                "Jueves", 4
            },
            {
                "Viernes", 5
            },
            {
                "Sabado", 6
            }
        };

        public static int GetDaysWithNumber(string name) =>
            ListDaysWithNumber.FirstOrDefault(x => x.Key.ToUpper() == name.ToUpper() || x.Key.ToUpper().Contains(name.ToUpper())).Value;

    }
}