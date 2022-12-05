using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;

namespace ComputeService
{
    public class SampleServiceWithActivities : SampleService
    {
        private HttpClient client = new HttpClient() { BaseAddress = new Uri("http://localhost:5000") };
        
        public async Task<double> ComputeAsync(int startValue)
        {
            string startValueMd5 = CreateMD5(startValue.ToString());
            HttpResponseMessage cachedResult = await client.GetAsync($"/cache/{startValueMd5}");

            if(cachedResult.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                using (MatActivitySource.Instance.StartActivity("Service.Compute"))
                {
                    double v = (double)startValue;
                    using (Activity? activity = MatActivitySource.Instance.StartActivity("ComputationLoop")) // IDisposable for timetracking
                    {
                        activity?.AddTag("someMoreInfo", DateTime.Now);
                        activity?.AddTag("User", "Matthes");

                        for (int i = 0; i < 5; i++)
                        {
                            activity?.AddEvent(new ActivityEvent($"Passed Iteration {i}"));
                            //First pass
                            using (MatActivitySource.Instance.StartActivity("FirstComputationStep"))//they know they're wrapped in parent activity
                            {
                                v = FirstComputationStep(v);
                            }

                            //second pass
                            using (MatActivitySource.Instance.StartActivity("SecondComputationStep"))
                            {
                                v = SecondComputationStep(v);
                            }

                        }

                    }

                    using (MatActivitySource.Instance.StartActivity("FinalComputationStep"))
                    {
                        double ms = _rnd.Next(1000, 3000);
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        while (sw.ElapsedMilliseconds < ms)
                        {
                            v = v * v;
                            v = Math.Sqrt(v);
                        }
                    }

                    //Cache the result in caching microservice
                    await client.PostAsync($"/cache/{startValueMd5}", new StringContent(v.ToString()))
                        .ConfigureAwait(false);
                    //return
                    return v;
                }
            }
            else
            {
                var result = await cachedResult.Content.ReadAsStringAsync().ConfigureAwait(false);
                double v = double.Parse(result, NumberStyles.Number, new CultureInfo("en-US"));
                return v;
            }
        }

        private string CreateMD5(string v)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(v);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return Convert.ToHexString(hashBytes);
            }
        }
    }

}
