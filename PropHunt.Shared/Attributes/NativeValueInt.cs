using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropHunt.Shared.Attributes
{
    public class NativeValueInt : Attribute
    {
        public int NativeValue { get; private set; }

        public NativeValueInt(int nativeValue)
        {
            this.NativeValue = nativeValue;
        }
    }
}
