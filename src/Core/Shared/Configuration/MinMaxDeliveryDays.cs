using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Shared.Configuration
{
    public class MinMaxDeliveryDays
    {

        public MinMaxDeliveryDays(int min, int max)
        {
            this.MinDays = min;
            this.MaxDays = max;
        }

        public int MinDays { get; }
        public int MaxDays { get; }
    }
}
