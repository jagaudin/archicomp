using System;
using System.Collections.Generic;

using MathNet.Spatial.Euclidean;

using libarchicomp.utils;
using libarchicomp.vaults;
using libarchicomp.loadcase;
using CatenaryVault = libarchicomp.vaults.CatenaryVault;
using LoadCase = libarchicomp.vaults.VaultLoadCase;
using DistributedLoadX = libarchicomp.vaults.VaultDLoadOverXByLength;
using DistributedLoadZ = libarchicomp.vaults.VaultDLoadOverZ;

namespace archicomp_cli
{
	class MainClass
	{
        public static void Main(string[] args)
	    {
            var udl1 = new DistributedLoadX(x => new Vector3D(0, 0, 1), -1, 1);
            var udl2 = new DistributedLoadZ(z => new Vector3D(1, 0, 3*(2-z)), .9, 2);

            var vault = new CatenaryVault(8.0, 2.0, 1.0, 100.0, 10, Restraint.Fixed);
            var loadcase = new LoadCase(vault, new List<IDiscretizableLoad<Vault>>{udl1, udl2});

            foreach (var pl in loadcase.HorizontalLoads)
            {
                Console.WriteLine(pl);
            }

            Console.ReadLine();

        }
	}
}