using System;

namespace Presentation.Site.Helpers.Models
{
    public class HttpSuccess
    {
        public HttpSuccess(object data)
        {
            Data = data;
        }

        public object Data { get; }
    }
}
