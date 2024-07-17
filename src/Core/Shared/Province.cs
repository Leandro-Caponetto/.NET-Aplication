using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Shared
{
    public static class Province
    {
        public static Dictionary<string, char> ListProvinces { get; set; } = new Dictionary<string, char>
        {
            {
                "Salta".ToLower(), 'A'
            },
            {
                "Provincia de Buenos Aires".ToLower(), 'B'
            },
            {
                "Ciudad Autonoma de Buenos Aires".ToLower(), 'C'
            },
            {
                "Capital Federal".ToLower(), 'C'
            },
            {
                "San Luis".ToLower(), 'D'
            },
            {
                "Entre Rios".ToLower(), 'E'
            },
            {
                "La Rioja".ToLower(), 'F'
            },
            {
                "Santiago del Estero".ToLower(), 'G'
            },
            {
                "Chaco".ToLower(), 'H'
            },
            {
                "San Juan".ToLower(), 'J'
            },
            {
                "Catamarca".ToLower(), 'K'
            },
            {
                "La Pampa".ToLower(), 'L'
            },
            {
                "Mendoza".ToLower(), 'M'
            },
            {
                "Misiones".ToLower(), 'N'
            },
            {
                "Formosa".ToLower(), 'P'
            },
            {
                "Neuquen".ToLower(), 'Q'
            },
            {
                "Rio Negro".ToLower(), 'R'
            },
            {
                "Santa Fe".ToLower(), 'S'
            },
            {
                "Tucuman".ToLower(), 'T'
            },
            {
                "Chubut".ToLower(), 'U'
            },
            {
                "Tierra del Fuego".ToLower(), 'V'
            },
            {
                "Corrientes".ToLower(), 'W'
            },
            {
                "Cordoba".ToLower(), 'X'
            },
            {
                "Jujuy".ToLower(), 'Y'
            },
            {
                "Santa Cruz".ToLower(), 'Z'
            },

        };

        public static char GetProvince(string name)
        {
            name = name.ToLower();
            name = name.Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u");
            return ListProvinces.FirstOrDefault(x => x.Key == name || x.Key.Contains(name)).Value;
        }
    }
}