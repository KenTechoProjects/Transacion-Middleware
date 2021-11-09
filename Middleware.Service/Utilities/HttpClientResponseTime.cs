using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Middleware.Service.Utilities
{
    public class HttpClientResponseTime
    {
        public async Task<Tuple<HttpResponseMessage, TimeSpan>> GetHttpWithTimingInfo(dynamic request, bool isPost = false, string url = null)
        {
            var url_ = "https://stmt-proxy.azurewebsites.net/api/v1/Statement/generate-statement";
            if (string.IsNullOrWhiteSpace(url))
            {
                url = url_;
            }
            var stopWatch = Stopwatch.StartNew();
            var result = new HttpResponseMessage();
            using (var  client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("AppId", "DIGLAB");
                client.DefaultRequestHeaders.Add("AppKey", "ca5b1ab3181141bead4dc9ca9d6ed5a5");
                var input = Util.SerializeAsJson<dynamic>(request);
                var data = new object();
                var message = new StringContent(input, Encoding.UTF8, "application/json");
                if (isPost)
                {
                     result = await client.PostAsync(url, message);

                }
                else
                {
                    result = await client.GetAsync(url);
                }
                var body = await result.Content.ReadAsStringAsync();
                if (body != null)
                {
                     data = Util.DeserializeFromJson<dynamic>(body);
                }
         
                return new Tuple<HttpResponseMessage, TimeSpan>((dynamic)result, stopWatch.Elapsed);
            }
        }
        public void Start()
        {
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 1000;
            aTimer.Enabled = true;
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 1000;
            aTimer.Enabled = true;
        }

    }
}
