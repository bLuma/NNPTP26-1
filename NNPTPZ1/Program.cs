using System;
using System.Collections.Generic;
using System.Drawing;
using NNPTPZ1.Mathematics;

namespace NNPTPZ1
{
    /// <summary>
    /// This program should produce Newton fractals.
    /// See more at: https://en.wikipedia.org/wiki/Newton_fractal
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            int width;
            int height;
            string output;
            Bitmap bmp;
            double xmin, ymin, xstep, ystep;
            Polynome p, pd;
            Color[] clrs;
            int maxid;

            PrepareEnvironment(args, out width, out height, out output, out bmp, out xmin, out ymin, out xstep, out ystep, out p, out pd, out clrs, out maxid);
            ComputeNewtonFractal(width, height, bmp, xmin, ymin, xstep, ystep, p, pd, clrs, maxid);
            SaveOutputImage(output, bmp);
        }

        private static void SaveOutputImage(string output, Bitmap bmp)
        {
            bmp.Save(output ?? "../../../out.png");
        }

        private static int ComputeNewtonFractal(int width, int height, Bitmap bmp, double xmin, double ymin, double xstep, double ystep, Polynome p, Polynome pd, Color[] clrs, int maxid)
        {
            List<ComplexNumber> roots = new List<ComplexNumber>();

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    ComplexNumber ox = GetCoordinate(xmin, ymin, xstep, ystep, i, j);

                    float it = DoNewtonIterationMethod(p, pd, ref ox);
                    
                    int rootIndex = FindRoot(roots, ref maxid, ox);
                    
                    ColorizePixel(bmp, clrs, i, j, it, rootIndex);
                }
            }

            return maxid;
        }

        private static void ColorizePixel(Bitmap bmp, Color[] clrs, int i, int j, float it, int rootIndex)
        {
            // colorize pixel according to root number
            Color color = clrs[rootIndex % clrs.Length];
            color = Color.FromArgb(
                Math.Min(Math.Max(0, color.R - (int)it * 2), 255), 
                Math.Min(Math.Max(0, color.G - (int)it * 2), 255), 
                Math.Min(Math.Max(0, color.B - (int)it * 2), 255)
                );

            bmp.SetPixel(j, i, color);
        }

        private static int FindRoot(List<ComplexNumber> roots, ref int maxid, ComplexNumber ox)
        {
            // find solution root number
            var rootKnown = false;
            var rootIndex = 0;

            for (int i = 0; i < roots.Count; i++)
            {
                if (Math.Pow(ox.Real - roots[i].Real, 2) + Math.Pow(ox.Imaginary - roots[i].Imaginary, 2) <= 0.01)
                {
                    rootKnown = true;
                    rootIndex = i;
                }
            }

            if (!rootKnown)
            {
                roots.Add(ox);

                rootIndex = roots.Count;
                maxid = rootIndex + 1;
            }

            return rootIndex;
        }

        private static int DoNewtonIterationMethod(Polynome p, Polynome pd, ref ComplexNumber ox)
        {
            // find solution of equation using newton's iteration
            int iterations = 0;
            for (int i = 0; i < 30; i++)
            {
                ComplexNumber diff = p.Eval(ox).Divide(pd.Eval(ox));
                ox = ox.Subtract(diff);

                if (Math.Pow(diff.Real, 2) + Math.Pow(diff.Imaginary, 2) >= 0.5)
                {
                    i--;
                }
                iterations++;
            }

            return iterations;
        }

        private static ComplexNumber GetCoordinate(double xmin, double ymin, double xstep, double ystep, int i, int j)
        {
            // find "world" coordinates of pixel
            double y = ymin + i * ystep;
            double x = xmin + j * xstep;

            ComplexNumber ox = new ComplexNumber()
            {
                Real = x,
                Imaginary = (float)(y)
            };

            if (ox.Real == 0)
                ox.Real = 0.0001;
            if (ox.Imaginary == 0)
                ox.Imaginary = 0.0001f;
            return ox;
        }

        private static void PrepareEnvironment(string[] args, out int width, out int height, out string output, out Bitmap bmp, out double xmin, out double ymin, out double xstep, out double ystep, out Polynome p, out Polynome pd, out Color[] clrs, out int maxid)
        {
            int[] intargs;
            double[] doubleargs;
            
            ParseNumberFromCommandLineArguments(args, out intargs, out doubleargs);

            output = args[6];
            bmp = new Bitmap(intargs[0], intargs[1]);
            xmin = doubleargs[0];
            double xmax = doubleargs[1];
            ymin = doubleargs[2];
            double ymax = doubleargs[3];

            xstep = (xmax - xmin) / intargs[0];
            ystep = (ymax - ymin) / intargs[1];

            width = intargs[0];
            height = intargs[1];

            clrs = new Color[]
            {
                Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Orange, Color.Fuchsia, Color.Gold, Color.Cyan, Color.Magenta
            };
            maxid = 0;

            PreparePolynomial(out p, out pd);
        }

        private static void ParseNumberFromCommandLineArguments(string[] args, out int[] intargs, out double[] doubleargs)
        {
            intargs = new int[2];

            for (int i = 0; i < intargs.Length; i++)
            {
                intargs[i] = int.Parse(args[i]);
            }
            
            doubleargs = new double[4];
            for (int i = 0; i < doubleargs.Length; i++)
            {
                doubleargs[i] = double.Parse(args[i + 2]);
            }
        }

        private static void PreparePolynomial(out Polynome p, out Polynome pd)
        {
            p = new Polynome();
            p.Coefficients.Add(new ComplexNumber() { Real = 1 });
            p.Coefficients.Add(ComplexNumber.Zero);
            p.Coefficients.Add(ComplexNumber.Zero);
            p.Coefficients.Add(new ComplexNumber() { Real = 1 });
            Polynome ptmp = p;
            pd = p.Derive();
            Console.WriteLine(p);
            Console.WriteLine(pd);
        }
    }


}
