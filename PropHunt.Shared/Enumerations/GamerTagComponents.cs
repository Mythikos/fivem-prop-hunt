using PropHunt.Shared.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropHunt.Shared.Enumerations
{
    public enum GamerTagComponents
    {
        [NativeValueInt(0)]
        GamerName,

        [NativeValueInt(1)]
        CrewTag,

        [NativeValueInt(2)]
        HealthAndArmor,

        [NativeValueInt(3)]
        BigText,

        [NativeValueInt(4)]
        AudioIcon,

        [NativeValueInt(5)]
        MpUsingMenu,

        [NativeValueInt(6)]
        MpPassiveMode,

        [NativeValueInt(7)]
        WantedStars,

        [NativeValueInt(8)]
        MpDriver,

        [NativeValueInt(9)]
        MpCoDriver,

        [NativeValueInt(10)]
        MpTagged,

        [NativeValueInt(11)]
        GamerNameNearby,

        [NativeValueInt(12)]
        Arrow,

        [NativeValueInt(13)]
        MpPackages,

        [NativeValueInt(14)]
        InvIfPedFollowing,

        [NativeValueInt(15)]
        RankText,

        [NativeValueInt(16)]
        MpTyping
    }
}
