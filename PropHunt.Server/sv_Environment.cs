using PropHunt.Shared;
using PropHunt.Shared.Enumerations;
using PropHunt.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropHunt.Server
{
    internal class sv_Environment
    {
        private sv_Init _parentInstance;

        public sv_Environment(sv_Init parentInstance)
        {
            this._parentInstance = parentInstance;
        }

        public void SetWeather(WeatherStates weatherState)
            => sv_Init.TriggerClientEvent(Constants.Events.Client.OnEnvironmentWeatherChanged, (int)weatherState);

        public void RandomizeWeather()
            => SetWeather((WeatherStates)Enum.GetValues(typeof(WeatherStates)).Random<int>());

        public void SetTime(TimeOfDayStates timeOfDayState)
            => sv_Init.TriggerClientEvent(Constants.Events.Client.OnEnvironmentTimeChanged, (int)timeOfDayState);

        public void RandomizeTime()
            => SetTime((TimeOfDayStates)Enum.GetValues(typeof(TimeOfDayStates)).Random<int>());

        public void SetWeatherAndTime(WeatherStates weatherState, TimeOfDayStates timeOfDayState)
            => sv_Init.TriggerClientEvent(Constants.Events.Client.OnEnvironmentWeatherAndTimeChanged, (int)weatherState, (int)timeOfDayState);

        public void RandomizeWeatherAndTime()
            => SetWeatherAndTime((WeatherStates)Enum.GetValues(typeof(WeatherStates)).Random<int>(), (TimeOfDayStates)Enum.GetValues(typeof(TimeOfDayStates)).Random<int>());
    }
}
