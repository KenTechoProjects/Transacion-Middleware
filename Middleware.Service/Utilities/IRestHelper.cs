using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.Utilities
{
    public interface IRestHelper
    {
        IRestResponse Get<T>(string endpoint, IDictionary<string, T> values = null);
        IRestResponse POST<T>(string endpoint, T value);
        IRestResponse PUT<T>(string endpoint, T value);
    }
}
