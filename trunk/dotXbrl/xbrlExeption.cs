using System;
using System.Collections.Generic;
using System.Text;

namespace dotXbrl.xbrlApi
{
    public class XbrlException:Exception 
    {
        public XbrlException(string e)
            : base(e)
        {
            
        }
    }
}
