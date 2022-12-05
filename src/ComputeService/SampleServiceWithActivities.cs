using System.Diagnostics;

namespace ComputeService
{
    public class SampleServiceWithActivities : SampleService
    {
        public async Task<double> ComputeAsync(int startValue)
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
                string startValueMd5 = CreateMD5(startValue.ToString());
                var client = new HttpClient() { BaseAddress = new Uri("http://localhost:5000") };
                await client.PostAsync($"/cache/{startValueMd5}", new StringContent(v.ToString()));

                //return
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
