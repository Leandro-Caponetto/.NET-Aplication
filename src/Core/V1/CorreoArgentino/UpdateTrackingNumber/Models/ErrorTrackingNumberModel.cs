using System;
using System.Collections.Generic;
using System.Text;

namespace Core.V1.CorreoArgentino.UpdateTrackingNumber.Models
{
    public class ErrorTrackingNumberModel
    {
        public ErrorTrackingNumberModel() {  }

        public string code { get; set; }
        public string error { get; set; }
    }
}
