using System;
using System.Collections.Generic;
using System.Text;

using dotXbrl.xbrlApi.XLink;

namespace runDotXbrlApi
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = "c:\\XlinkPrueba1.xml";

            Validator validador = new Validator(new Uri(url));

            Console.ReadLine();
        }
    }
}
