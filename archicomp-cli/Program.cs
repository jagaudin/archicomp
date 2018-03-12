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
using DistributedLoadX = libarchicomp.vaults.VaultDistributedLoadOverX;
using DistributedLoadZ = libarchicomp.vaults.VaultDistributedLoadOverZ;

namespace archicomp_cli
{
	class MainClass
	{
        public static void Main(string[] args)
	    {
            var v1 = new Vector3D(3.0, -9, 0);
            var v2 = new Vector3D(-2.0, 8.0, 0);
            Vector3D[] l1 = { v1, v2 };

            foreach (Vector3D vec in accumulate(l1, ((vec1, vec2) => vec1 + vec2), new Vector3D(0, 0, 0)))
            {
                Console.WriteLine(vec);
            }

            var vault = new CatenaryVault(8.0, 2.0, 1.0, 100.0, 10, Restraint.Fixed);
            Console.WriteLine(vault.a);

            Console.WriteLine(vault.L);
            Console.WriteLine(vault.XToLength(4));
            Console.WriteLine(vault.Points.ElasticCenter);

            var genvault = new GenericVault(x => vault.F(x), 8.0, 2.0, 1.0, 100.0, 10, Restraint.Fixed);

            Console.WriteLine(genvault.F(genvault.w / 2));
            Console.WriteLine(genvault.L);
            Console.WriteLine(genvault.XToLength(4));
            Console.WriteLine(genvault.Points.ElasticCenter);

            var loadcase = new LoadCase(vault);

            Console.WriteLine("Point load X:");

            PointLoad pl = new PointLoad(3.99, new Vector3D(0, 0, -5));
            foreach (var load in pl.Load.ToProjectedPointLoads(vault))
            {
                Console.WriteLine(load.Loc.X);
            }

            Console.WriteLine("MidSegX list");

            foreach (var x in vault.Coord.MidSegmentX)
            {
                Console.WriteLine(x);
            }

            Console.WriteLine("SegX list");

            foreach (var x in vault.Coord.SegmentX)
            {
                Console.WriteLine(x);
            }

            Console.WriteLine("Values betwwen:");
            var res = ValuesBetween(-3.4, -2.9, vault.Coord.SegmentX);

            foreach (var x in res)
            {
                Console.WriteLine(x);
            }

            //var udl1 = new DistributedLoadX(x => new Vector3D(1, 0, -3), -3, 3);

            //foreach(var f in udl1.ToProjectedPointLoads(vault))
            //{
            //    Console.WriteLine(f);
            //}
            var udl2 = new DistributedLoadZ(z => new Vector3D(1, 0, 0), 0, 1);

            foreach (var f in udl2.ToProjectedPointLoads(vault))
            {
                Console.WriteLine(f);
            }


            Console.ReadLine();
        }
	}
}

