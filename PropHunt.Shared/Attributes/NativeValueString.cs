using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropHunt.Shared.Attributes
{
    public class NativeValueString : Attribute
    {
        public string NativeValue { get; private set; }

        public NativeValueString(string nativeValue)
        {
            this.NativeValue = nativeValue;
        }
    }
}
