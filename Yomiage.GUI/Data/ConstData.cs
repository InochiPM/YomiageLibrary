using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yomiage.GUI.Data
{
    static class ConstData
    {
        public static string Num1000 { get; }
        public static string[] Prioritys { get; } = new string[5]
        {
            "1.Highest",
            "2.High",
            "3.Mid",
            "4.Low",
            "5.Lowest",
        };

        static ConstData()
        {
            var temp = "";
            foreach(var i in Enumerable.Range(1, 999))
            {
                temp += i.ToString() + Environment.NewLine;
            }
            Num1000 = temp;
        }

        public static string AppName { get; } = "UNI-COE";
    }
}
