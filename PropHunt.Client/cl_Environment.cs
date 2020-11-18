using CitizenFX.Core;
using PropHunt.Shared;
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
    internal static class cl_Environment
    {
        public static void SetTime(int timeOfDayState)
        {
            TimeOfDayStates timeStateEnum;

            timeStateEnum = (TimeOfDayStates)timeOfDayState;

            NetworkOverrideClockTime(timeStateEnum.GetAttribute<NativeValueInt>().NativeValue, 0, 0);
        }

        public static void SetWeather(int weatherState)
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

        public static void SetWeatherAndTime(int weatherState, int timeOfDayState)
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
    }
}
