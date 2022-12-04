using System.Diagnostics;

namespace ComputeService
{
    public class SampleServiceWithEvents : SampleService
    {
        
        public override double Compute(int startValue)
        {
            double v = (double)startValue;
            for (int i = 0; i < 5; i++)
            {
                //First pass
                MatEventSource.Log.ComputeStart(v, 1, i);
                v = FirstComputationStep(v);
                MatEventSource.Log.ComputeStop(v, 1, i);

                //second pass
                MatEventSource.Log.ComputeStart(v, 2,i );
                v = SecondComputationStep(v);
                MatEventSource.Log.ComputeStop(v, 2, i);
            }

            MatEventSource.Log.ComputeStart(v, 3, 0);
            double ms = _rnd.Next(1000, 3000);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while(sw.ElapsedMilliseconds < ms)
            {
                v = v * v;
                v = Math.Sqrt(v);
            }
            MatEventSource.Log.ComputeStop(v, 3, 0);

            //return
            return v;
        }
    }

}
