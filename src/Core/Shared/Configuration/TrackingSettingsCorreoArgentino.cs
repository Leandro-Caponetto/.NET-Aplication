using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Shared.Configuration
{
    public class TrackingSettingsCorreoArgentino
    {

        public TrackingSettingsCorreoArgentino(string url)
        {
            this.UrlShipping = url;
        }

        public string UrlShipping { get; }
    }
}
