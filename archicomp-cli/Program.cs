using System;
using System.Collections.Generic;

using MathNet.Spatial.Euclidean;

using libarchicomp;
using static libarchicomp.utils.StaticMethods;
using libarchicomp.utils;
using CatenaryVault = libarchicomp.vaults.CatenaryVault;
using GenericVault = libarchicomp.vaults.GenericVault;
using LoadCase = libarchicomp.vaults.VaultLoadCase;
using PointLoad = libarchicomp.vaults.VaultPointLoad;

namespace archicomp_cli
{
	class MainClass
	{

        public static void Main(string[] args)
		{
            Console.WriteLine("Hello World!");
            var v1 = new Vector3D(3.0, -9, 0);
            var v2 = new Vector3D(-2.0, 8.0, 0);
            var v = v1 + v2;
            Console.WriteLine("{0}: {1}", v, v.Length);
            var p1 = new Point3D(3.0, -9, 0);
            var p2 = new Point3D(-2.0, 8.0, 0);
            var p = p1 + p2.ToVector3D();
            Console.WriteLine("{0}: {1}", p, p.DistanceTo(p1));
            Vector3D[] l1 = { v1, v2 };

            foreach(Vector3D vec in accumulate(l1, ((vec1, vec2) => vec1 + vec2), new Vector3D(0, 0, 0)))
            {
                Console.WriteLine(vec);
            }

            var vault = new CatenaryVault(8.0, 2.0, 1.0, 100.0, 10, Restraint.Fixed);
            Console.WriteLine(vault.a);
            
            Console.WriteLine(vault.L);
            Console.WriteLine(vault.XToLength(4));
            Console.WriteLine(vault.Points.ElasticCenter);

            var genvault = new GenericVault(x => vault.F(x),8.0, 2.0, 1.0, 100.0, 10, Restraint.Fixed);
            
            Console.WriteLine(genvault.F(genvault.w / 2));
            Console.WriteLine(genvault.L);
            Console.WriteLine(genvault.XToLength(4));
            Console.WriteLine(genvault.Points.ElasticCenter);

			var loadcase = new LoadCase(vault);

            Console.WriteLine("Point load X:");

            PointLoad pl = new PointLoad(3.99, new Vector3D(0, 0, -5));
            foreach(var load in pl.ToPointLoads(vault))
            {
                Console.WriteLine(load.Loc.X);
            }

            Console.WriteLine("MidSegX list");

            foreach(var x in vault.Coord.MidSegmentX)
            {
                Console.WriteLine(x);
            }

			Console.WriteLine("SegX list");

			foreach (var x in vault.Coord.SegmentX)
			{
				Console.WriteLine(x);
			}


            Console.ReadLine();
		}
	}
}
