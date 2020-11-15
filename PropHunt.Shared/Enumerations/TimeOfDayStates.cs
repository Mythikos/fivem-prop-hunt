using PropHunt.Shared.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropHunt.Shared.Enumerations
{
    public enum TimeOfDayStates
    {
        [NativeValueInt(8)]
        Morning,

        [NativeValueInt(12)]
        Noon,

        [NativeValueInt(18)]
        Evening,

        [NativeValueInt(23)]
        Night
    }
}
