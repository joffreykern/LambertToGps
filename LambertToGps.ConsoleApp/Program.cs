using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LambertToGps.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Coordonnées Lambert I du Zenith de Strasbourg
            double y = 113467.422;
            double x = 994272.661;

            double[] gps = LambertToGps.Convert(x, y, LambertVersion.LambertI);
            Console.WriteLine(string.Format("{0},{1}", gps[0], gps[1]));
            Console.Read();
        }
    }
}
