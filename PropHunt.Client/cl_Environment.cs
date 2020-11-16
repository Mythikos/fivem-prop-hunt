using CitizenFX.Core;
using PropHunt.Shared.Attributes;
using PropHunt.Shared.Enumerations;
using PropHunt.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace PropHunt.Client
{
    internal class cl_Environment
    {
        private cl_Init _parentInstance;

        public cl_Environment(cl_Init parentInstance)
        {
            this._parentInstance = parentInstance;
        }

        #region Events
        public void OnTimeChanged(int timeOfDayState)
        {
            TimeOfDayStates timeStateEnum;

            timeStateEnum = (TimeOfDayStates)timeOfDayState;

            NetworkOverrideClockTime(timeStateEnum.GetAttribute<NativeValueInt>().NativeValue, 0, 0);
        }
        
        public void OnWeatherChanged(int weatherState)
        {
            WeatherStates weatherStateEnum;

            weatherStateEnum = (WeatherStates)weatherState;

            SetWeatherTypePersist(weatherStateEnum.GetAttribute<NativeValueString>().NativeValue);
            SetWeatherTypeNowPersist(weatherStateEnum.GetAttribute<NativeValueString>().NativeValue);
            SetWeatherTypeNow(weatherStateEnum.GetAttribute<NativeValueString>().NativeValue);
            SetOverrideWeather(weatherStateEnum.GetAttribute<NativeValueString>().NativeValue);
            SetForcePedFootstepsTracks(false);
            SetForceVehicleTrails(false);
        }
        
        public void OnWeatherAndTimeChanged(int weatherState, int timeOfDayState)
        {
            WeatherStates weatherStateEnum;
            TimeOfDayStates timeStateEnum;

            weatherStateEnum = (WeatherStates)weatherState;
            timeStateEnum = (TimeOfDayStates)timeOfDayState;

            SetWeatherTypePersist(weatherStateEnum.GetAttribute<NativeValueString>().NativeValue);
            SetWeatherTypeNowPersist(weatherStateEnum.GetAttribute<NativeValueString>().NativeValue);
            SetWeatherTypeNow(weatherStateEnum.GetAttribute<NativeValueString>().NativeValue);
            SetOverrideWeather(weatherStateEnum.GetAttribute<NativeValueString>().NativeValue);
            SetForcePedFootstepsTracks(false);
            SetForceVehicleTrails(false);
            NetworkOverrideClockTime(timeStateEnum.GetAttribute<NativeValueInt>().NativeValue, 0, 0);
        }
        #endregion
    }
}
