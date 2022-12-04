using System.Diagnostics;

namespace ComputeService
{
    public class SampleService
    {

        protected static Random _rnd = new Random();

        public virtual double Compute(int startValue)
        {
            double v = (double)startValue;
            for (int i = 0; i < 5; i++)
            {  
                //First pass
                v = FirstComputationStep(v);

                //second pass
                v = SecondComputationStep(v);
            }

            double ms = _rnd.Next(1000, 3000);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds < ms)
            {
                v = v * v;
                v = Math.Sqrt(v);
            }

            //return 
            return v;
        }

        protected double FirstComputationStep(double value)
        {
            double ms = _rnd.Next(500);
            var stop = DateTime.Now.AddMilliseconds(ms);

            while (DateTime.Now < stop)
            {
                value = value * value;
                value = Math.Sqrt(value);
            }
            return (value + _rnd.Next(10));
        }

        protected double SecondComputationStep(double value)
        {
            double ms = _rnd.Next(500);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds < ms)
            {
                value = value * value;
                value = Math.Sqrt(value);
            }
            return (value - _rnd.Next(10));
        }

    }

}
