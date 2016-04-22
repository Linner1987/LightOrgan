using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Pwm;

namespace LightOrganApp
{
    public class LightsController: IDisposable
    {
        private const int REDLED_PIN = 5;
        private const int ORANGELED_PIN = 6;
        private const int BLUELED_PIN = 13;

        PwmPin redPin;
        PwmPin orangePin;
        PwmPin bluePin;

        PwmController pwmController;

        public async Task InitAsync()
        {
            pwmController = (await PwmController.GetControllersAsync(PwmSoftware.PwmProviderSoftware.GetPwmProvider()))[0];
            pwmController.SetDesiredFrequency(100);

            redPin = InitPin(REDLED_PIN);
            orangePin = InitPin(ORANGELED_PIN);
            bluePin = InitPin(BLUELED_PIN);

            SetCyclePercentage(redPin, 1);
            SetCyclePercentage(orangePin, 1);
            SetCyclePercentage(bluePin, 1);

            Task.Delay(50).Wait();

            SetCyclePercentage(redPin, 0);
            SetCyclePercentage(orangePin, 0);
            SetCyclePercentage(bluePin, 0);
        }

        public void SetValues(double bassValue, double midValue, double trebleValue)
        {
            SetCyclePercentage(redPin, bassValue);
            SetCyclePercentage(orangePin, midValue);
            SetCyclePercentage(bluePin, trebleValue);
        }

        public void Stop()
        {
            StopPin(redPin);
            StopPin(orangePin);
            StopPin(bluePin);
        }

        private PwmPin InitPin(int pinNumber)
        {
            var pin = pwmController.OpenPin(pinNumber);            
            pin.Start();           

            return pin;
        }

        private void SetCyclePercentage(PwmPin pin, double value)
        {  
            if (value >= 0 && value <=1)          
                pin.SetActiveDutyCyclePercentage(value);
        }

        private void StopPin(PwmPin pin)
        {
            if (pin.IsStarted)
                pin.Stop();
        }

        private void DisposePin(PwmPin pin)
        {
            if (pin != null)
            {
                StopPin(pin);
                pin.Dispose();
                pin = null;
            }
        }

        public void Dispose()
        {
            DisposePin(redPin);
            DisposePin(orangePin);
            DisposePin(bluePin);
        }
    }
}
