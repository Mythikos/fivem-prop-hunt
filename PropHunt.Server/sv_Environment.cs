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
    /// <summary>
    /// Controls the weather from the server side
    /// </summary>
    internal static class sv_Environment
    {
        /// <summary>
        /// Sets the weather state for each client
        /// </summary>
        /// <param name="weatherState"></param>
        public static void SetWeather(WeatherStates weatherState)
            => sv_Init.TriggerClientEvent(Constants.Actions.Environment.SetWeather, (int)weatherState);

        /// <summary>
        /// Randomizes the weather state and assigns it to each client
        /// </summary>
        public static void RandomizeWeather()
            => SetWeather((WeatherStates)Enum.GetValues(typeof(WeatherStates)).Random<int>());

        /// <summary>
        /// Sets the time state for each client
        /// </summary>
        /// <param name="timeOfDayState"></param>
        public static void SetTime(TimeOfDayStates timeOfDayState)
            => sv_Init.TriggerClientEvent(Constants.Actions.Environment.SetTime, (int)timeOfDayState);

        /// <summary>
        /// Randomizes the time state and assigns it to each client
        /// </summary>
        public static void RandomizeTime()
            => SetTime((TimeOfDayStates)Enum.GetValues(typeof(TimeOfDayStates)).Random<int>());

        /// <summary>
        /// Sets the weather and time state for each client
        /// </summary>
        /// <param name="weatherState"></param>
        /// <param name="timeOfDayState"></param>
        public static void SetWeatherAndTime(WeatherStates weatherState, TimeOfDayStates timeOfDayState)
            => sv_Init.TriggerClientEvent(Constants.Actions.Environment.SetWeatherAndTime, (int)weatherState, (int)timeOfDayState);

        /// <summary>
        /// Randomizes the weather and time state and assigns it to each client
        /// </summary>
        public static void RandomizeWeatherAndTime()
            => SetWeatherAndTime((WeatherStates)Enum.GetValues(typeof(WeatherStates)).Random<int>(), (TimeOfDayStates)Enum.GetValues(typeof(TimeOfDayStates)).Random<int>());
    }
}
