using System.Diagnostics;

namespace ComputeService
{
    public class SampleServiceWithActivities : SampleService
    {
        public override double Compute(int startValue)
        {
            double v = (double)startValue;
            using (Activity? activity = MatActivitySource.Instance.StartActivity("ComputationLoop")) // IDisposable for timetracking
            {
                
                for (int i = 0; i < 5; i++)
                {
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

            //return
            return v;
        }

        
    }

}
