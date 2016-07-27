using System;
using LightOrganApp.Droid.Model;
using Android.Media.Audiofx;

namespace LightOrganApp.Droid
{
    public class LightOrganProcessor
    {
        private const float LowMaxValue = 5000;
        private const float MidMaxValue = 6000;
        private const float HighMaxValue = 2000;

        private const int LowFrequency = 50;
        private const int MidFreguency = 3000;
        private const int HighFrequency = 16000;

        private float bassLevelAcc;
        private float midLevelAcc;
        private float trebleLevelAcc;

        private int numberOfSamplesInOneSec;
        private long systemTimeStartSec;

        public event EventHandler<LightOrganEventArgs> LightOrganDataUpdated;

        public void ProcessFftData(Visualizer visualizer, byte[] fft, int samplingRate)
        {
            var beginningOfTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            if (systemTimeStartSec == 0)
                systemTimeStartSec = (long)(DateTime.UtcNow - beginningOfTime).TotalMilliseconds;

            //bass
            int energySum = 0;
            //energySum = Math.abs(fft[0]);
            int k = 2;
            double captureSize = visualizer.CaptureSize / 2;
            int sampleRate = visualizer.SamplingRate / 2000;

            double nextFrequency = ((k / 2) * sampleRate) / (captureSize);
            while (nextFrequency < LowFrequency)
            {
                energySum += (int)GetAmplitude(fft[k], fft[k + 1]);
                k += 2;
                nextFrequency = ((k / 2) * sampleRate) / (captureSize);
            }
            double sampleAvgAudioEnergy = (double)energySum / (double)((k * 1.0) / 2.0);
            bassLevelAcc += (float)GetRatioAmplitude(sampleAvgAudioEnergy, LowMaxValue);


            //mid
            energySum = 0;
            while (nextFrequency < MidFreguency)
            {
                energySum += (int)GetAmplitude(fft[k], fft[k + 1]);
                k += 2;
                nextFrequency = ((k / 2) * sampleRate) / (captureSize);
            }
            sampleAvgAudioEnergy = (double)energySum / (double)((k * 1.0) / 2.0);
            midLevelAcc += (float)GetRatioAmplitude(sampleAvgAudioEnergy, MidMaxValue);


            //treble
            //energySum = Math.abs(fft[1]);
            energySum = 0;

            while ((nextFrequency < HighFrequency) && (k < fft.Length))
            {
                energySum += (int)GetAmplitude(fft[k], fft[k + 1]);
                k += 2;
                nextFrequency = ((k / 2) * sampleRate) / (captureSize);
            }
            sampleAvgAudioEnergy = (double)energySum / (double)((k * 1.0) / 2.0);
            trebleLevelAcc += (float)GetRatioAmplitude(sampleAvgAudioEnergy, HighMaxValue);


            numberOfSamplesInOneSec++;

            if (((long)(DateTime.UtcNow - beginningOfTime).TotalMilliseconds - systemTimeStartSec) > 100)
            {

                LightOrganData data = new LightOrganData
                {
                    BassLevel = bassLevelAcc / numberOfSamplesInOneSec,
                    MidLevel = midLevelAcc / numberOfSamplesInOneSec,
                    TrebleLevel = trebleLevelAcc / numberOfSamplesInOneSec
                };

                LightOrganDataUpdated?.Invoke(this, new LightOrganEventArgs(data));

                numberOfSamplesInOneSec = 0;
                bassLevelAcc = 0;
                midLevelAcc = 0;
                trebleLevelAcc = 0;
                systemTimeStartSec = (long)(DateTime.UtcNow - beginningOfTime).TotalMilliseconds;
            }
        }

        private static double GetAmplitude(byte r, byte i)
        {
            return Math.Sqrt(r * r + i * i);
        }

        private static double GetRatioAmplitude(double energy, double maxValue)
        {
            double value = energy * 1000;
            if (value > maxValue)
                value = maxValue;

            double v = value / maxValue;

            if (v < 0.05)
                v = 0.05;

            return v;
        }        
    }

    public class LightOrganEventArgs : EventArgs
    {
        public LightOrganData Data { get; private set; }

        public LightOrganEventArgs(LightOrganData data)
        {
            Data = data;
        }
    }            
}