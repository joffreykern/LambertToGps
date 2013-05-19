using System;
using System.Diagnostics;

namespace LambertToGps
{
    /// <summary>
    /// http://www.forumsig.org/showthread.php?t=7418&page=3 by jm.2
    /// </summary>
    public static class LambertToGps
    {
        public static double[] Convert(double x, double y, LambertVersion version)
        {
            int index = (int) version;
            double[] ntabs = {0.7604059656, 0.7289686274, 0.6959127966, 0.6712679322, 0.7289686274, 0.7256077650};
            double[] ctabs = {11603796.98, 11745793.39, 11947992.52, 12136281.99, 11745793.39, 11754255.426};
            double[] Xstabs = {600000.0, 600000.0, 600000.0, 234.358, 600000.0, 700000.0};
            double[] Ystabs = {5657616.674, 6199695.768, 6791905.085, 7239161.542, 8199695.768, 12655612.050};

            double n = ntabs[index];
            double c = ctabs[index];            // En mètres
            double Xs = Xstabs[index];          // En mètres
            double Ys = Ystabs[index];          // En mètres
            double l0 = 0.0;                    //correspond à la longitude en radian de Paris (2°20'14.025" E) par rapport à Greenwich
            double e = 0.08248325676;           //e du NTF (on le change après pour passer en WGS)
            double eps = Math.Pow(10, -10);     // précision


            /***********************************************************
            *  coordonnées dans la projection de Lambert 2 à convertir *
            ************************************************************/
            double X = x;
            double Y = y;

            Output("X = " + X);
            Output("Y = " + Y);


            /*
             * Conversion Lambert 2 -> NTF géographique : ALG0004
             */

            Output("\n-----------------------------------------------\n");
            Output("Conversion Lambert 2 -> NTF géographique");

            double l;
            double L;
            double phi;
            double phi0;
            double phii;
            double phiprec;
            double R;
            double g;

            R = Math.Sqrt(((X - Xs) * (X - Xs)) + ((Y - Ys) * (Y - Ys)));
            g = Math.Atan((X - Xs) / (Ys - Y));

            l = l0 + (g / n);
            L = -(1 / n) * Math.Log(Math.Abs(R / c));


            phi0 = 2 * Math.Atan(Math.Exp(L)) - (Math.PI / 2.0);
            phiprec = phi0;
            phii = 2 * Math.Atan((Math.Pow(((1 + e * Math.Sin(phiprec)) / (1 - e * Math.Sin(phiprec))), e / 2.0) * Math.Exp(L))) - (Math.PI / 2.0);

            while (!(Math.Abs(phii - phiprec) < eps))
            {
                phiprec = phii;
                phii = 2 * Math.Atan((Math.Pow(((1 + e * Math.Sin(phiprec)) / (1 - e * Math.Sin(phiprec))), e / 2.0) * Math.Exp(L))) - (Math.PI / 2.0);
            }

            phi = phii;

            Output("Lambda = " + l + "rad = " + l * 200 / Math.PI + "gr");
            Output("Phi    = " + phi + "rad = " + phi * 200 / Math.PI + "gr");

            /*
             * Conversion NTF géographique -> NTF cartésien : ALG0009
             */

            Output("\n-----------------------------------------------\n");
            Output("Conversion NTF géographique -> NTF cartésien");

            double N;
            double X_cart;
            double Y_cart;
            double Z_cart;

            double a = 6378249.2;
            double h = 100;         // En mètres

            double XWGS84;
            double YWGS84;
            double ZWGS84;

            N = a / (Math.Pow((1 - (e * e) * (Math.Sin(phi) * Math.Sin(phi))), 0.5));
            X_cart = (N + h) * Math.Cos(phi) * Math.Cos(l);
            Y_cart = (N + h) * Math.Cos(phi) * Math.Sin(l);
            Z_cart = ((N * (1 - (e * e))) + h) * Math.Sin(phi);


            Output("X cartésien NTF = " + X_cart);
            Output("Y cartésien NTF = " + Y_cart);
            Output("Z cartésien NTF = " + Z_cart);



            /*
             * Conversion NTF cartésien -> WGS84 cartésien : ALG0013
             */
            Output("\n-----------------------------------------------\n");
            Output("Conversion NTF cartésien -> WGS84 cartésien");

            // Il s'agit d'une simple translation
            XWGS84 = X_cart - 168;
            YWGS84 = Y_cart - 60;
            ZWGS84 = Z_cart + 320;


            Output("X cartésien WGS84 = " + XWGS84);
            Output("Y cartésien WGS84 = " + YWGS84);
            Output("Z cartésien WGS84 = " + ZWGS84);


            /*
             * Conversion WGS84 cartésien -> WGS84 géographique : ALG0012
             */

            Output("\n-----------------------------------------------\n");
            Output("Conversion WGS84 cartésien -> WGS84 géographique");

            double P;
            double phi840;
            double phi84prec;
            double phi84i;
            double phi84;
            double l840 = 0.04079234433;    // 0.04079234433 pour passer dans un référentiel par rapport au méridien
            // de Greenwich, sinon mettre 0
            double l84;
            e = 0.08181919106;              // On change e pour le mettre dans le système WGS84 au lieu de NTF
            a = 6378137.0;

            P = Math.Sqrt((XWGS84 * XWGS84) + (YWGS84 * YWGS84));

            l84 = l840 + Math.Atan(YWGS84 / XWGS84);

            phi840 = Math.Atan(ZWGS84 / (P * (1 - ((a * e * e))
                                        / Math.Sqrt((XWGS84 * XWGS84) + (YWGS84 * YWGS84) + (ZWGS84 * ZWGS84)))));

            phi84prec = phi840;

            phi84i = Math.Atan((ZWGS84 / P) / (1 - ((a * e * e * Math.Cos(phi84prec))
                    / (P * Math.Sqrt(1 - e * e * (Math.Sin(phi84prec) * Math.Sin(phi84prec)))))));

            while (!(Math.Abs(phi84i - phi84prec) < eps))
            {
                phi84prec = phi84i;
                phi84i = Math.Atan((ZWGS84 / P) / (1 - ((a * e * e * Math.Cos(phi84prec))
                        / (P * Math.Sqrt(1 - ((e * e) * (Math.Sin(phi84prec) * Math.Sin(phi84prec))))))));

            }

            phi84 = phi84i;

            Output("lat WGS84  = " + l84 + "rad = " + l84 * 180.0 / Math.PI + " deg");
            Output("long WGS84 = " + phi84 + "rad = " + phi84 * 180.0 / Math.PI + " deg");

            return new double[] { phi84 * 180.0 / Math.PI, l84 * 180.0 / Math.PI };
        }

        static void Output(string text)
        {
            Debug.WriteLine(text);
        }
    }

    //' |---------------------------------------------------------------------------------------------------------------|
    //' | Const | 1 'Lambert I | 2 'Lambert II | 3 'Lambert III | 4 'Lambert IV | 5 'Lambert II Etendue | 6 'Lambert 93 |
    //' |-------|--------------|---------------|----------------|---------------|-----------------------|---------------|
    //' |    n  | 0.7604059656 |  0.7289686274 |   0.6959127966 | 0.6712679322  |    0.7289686274       |  0.7256077650 |
    //' |-------|--------------|---------------|----------------|---------------|-----------------------|---------------|
    //' |    c  | 11603796.98  |  11745793.39  |   11947992.52  | 12136281.99   |    11745793.39        |  11754255.426 |
    //' |-------|--------------|---------------|----------------|---------------|-----------------------|---------------|
    //' |    Xs |   600000.0   |    600000.0   |   600000.0     |      234.358  |    600000.0           |     700000.0  |
    //' |-------|--------------|---------------|----------------|---------------|-----------------------|---------------|
    //' |    Ys | 5657616.674  |  6199695.768  |   6791905.085  |  7239161.542  |    8199695.768        | 12655612.050  |
    //' |---------------------------------------------------------------------------------------------------------------|
    public enum LambertVersion
    {
        LambertI = 0,
        LambertII = 1,
        LambertIII = 2,
        LamberIV = 3,
        LambertIIExtend = 4,
        Lambert93 = 5
    }
}
