using PropHunt.Shared.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropHunt.Shared.Enumerations
{
    public enum WeatherStates
    {
        [NativeValueString("EXTRASUNNY")]
        ExtraSunny,

        [NativeValueString("CLEAR")]
        Clear,

        [NativeValueString("FOGGY")]
        Foggy,

        [NativeValueString("OVERCAST")]
        Overcast,

        [NativeValueString("CLOUDS")]
        Clouds,

        [NativeValueString("CLEARING")]
        Clearing,

        [NativeValueString("RAIN")]
        Rain,

        [NativeValueString("THUNDER")]
        Thunder,

        [NativeValueString("SMOG")]
        Smog,

        [NativeValueString("BLIZZARD")]
        Blizzard,

        [NativeValueString("SNOWLIGHT")]
        SnowLight,

        [NativeValueString("XMAS")]
        Xmas,
    }
}
