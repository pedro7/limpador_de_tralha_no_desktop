﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Limpador
{
    class Program
    {
        static void Main(string[] args)
        {
            //new Testes();
            //return;

            if (args.Count() == 0) {
                var CONFS = ConfiguracoesGlobais.GetConfs().DIRS;
                foreach (string str in CONFS)
                {
                    var obj = new WeekSorter(str);
                    obj.OrganizeFiles();
                }
            }
            else
            {
                Console.WriteLine("Parâmetros ainda não suportados . . . ");
            }

            Console.WriteLine("Press any key to continue . . . ");
            Console.ReadKey();
        }
    }
}
