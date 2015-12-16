﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Limpador
{
    public class Limpador
    {

        private string alvo;
        private DirectoryInfo di = null;

        public Limpador(string dir)
        {
            try
            {
                di = new DirectoryInfo(dir);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(dir.ToString() + " não foi encontrada.");
                throw ex;
            }
            alvo = di.ToString() + @"\TRALHA";
        }


        public List<FileSystemInfo> GetFileList()
        {
            List<FileSystemInfo> retorno = new List<FileSystemInfo>();

            foreach (FileSystemInfo fi in di.EnumerateFileSystemInfos())
            {
                DateTime antigo = ObterUltimoAcessoOuMod(fi);

                TimeSpan ts = DateTime.Now - antigo;
                if (ts.TotalHours > 72)
                {
                    Console.Write("YES: ");
                    Console.WriteLine(fi.Name + " " + antigo + " to " + WeeksInYear(antigo));
                    retorno.Add(fi);
                }
                else
                {
                    Console.Write("NO: ");
                    Console.WriteLine(fi.Name + " " + antigo + " to " + WeeksInYear(antigo));
                }
            }

            return retorno;
        }


        private static Calendar cal = CultureInfo.InvariantCulture.Calendar;
        public static int WeeksInYear(DateTime time)
        {
            // This presumes that weeks start with Monday.
            // Week 1 is the 1st week of the year with a Thursday in it.

            // Seriously cheat.  If it's Monday, Tuesday or Wednesday, then it'll
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = cal.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return cal.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public void OrganizeFiles()
        {
            //Console.Out.WriteLine("ok");
            List<FileSystemInfo> ficheiros = GetFileList();

            DirectoryInfo di2 = new DirectoryInfo(alvo);

            foreach (FileSystemInfo fi in ficheiros)
            {
                if (String.Compare(fi.Name, "TRALHA", true) == 0)
                    continue;

                if (String.Compare(fi.Name, "desktop.ini", true) == 0)
                    continue;

                if (fi.Name.StartsWith(".") == true)
                    continue;


                int week = WeeksInYear(ObterUltimoAcessoOuMod(fi));

                DirectoryInfo destino2 = di2.CreateSubdirectory(DateTime.Now.Year + "-week" + week);
                //Console.Out.WriteLine(fi.Name + "->" + destino2.Name);

                if (fi is FileInfo)
                {
                    var destinoEmTralha = new FileInfo(destino2.ToString() + Path.DirectorySeparatorChar + fi.Name);
                    if (destinoEmTralha.Exists)
                    {
                        FileInfo Original = (FileInfo)fi;
                        var existente = GetChecksumBuffered(new FileStream(destinoEmTralha.ToString(), FileMode.Open));
                        var candidato = GetChecksumBuffered(new FileStream(destino2.ToString() + Path.DirectorySeparatorChar + Original.ToString(), FileMode.Open));

                        Console.WriteLine("IGNORADO DUP: " + fi.Name);
                    }
                    else
                    {
                        ((FileInfo)fi).MoveTo(destinoEmTralha.ToString());
                    }
                }
                else if (fi is DirectoryInfo)
                {
                    try
                    {
                        ((DirectoryInfo)fi).MoveTo(destino2.ToString() + Path.DirectorySeparatorChar + fi.Name);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("IGNORADO EXCEPCAO: " + fi.Name);
                    }
                }
                else
                {

                    throw new Exception();
                }
            }
        }

        private DateTime ObterUltimoAcessoOuMod(FileSystemInfo fi)
        {
            TimeSpan ts1 = fi.LastWriteTime - fi.LastAccessTime;
            if (Math.Abs(ts1.TotalHours) > 1)
            {
                return fi.LastWriteTime;
            }

            return fi.LastAccessTime;

        }

        private string GetChecksumBuffered(Stream stream)
        {
            using (var bufferedStream = new BufferedStream(stream, 1024 * 32))
            {
                var sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(bufferedStream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }
    }
}
