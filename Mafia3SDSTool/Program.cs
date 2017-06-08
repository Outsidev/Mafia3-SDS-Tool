using Ionic.Zip;
using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Burak Sey(Grimm)
/// Mafia 3 Sds Export Import Tool
/// x.12.16
/// </summary>

namespace Mafia3SDSTool
{
    class Program
    {
        static string curLocation;

        static void Main(string[] args)
        {
            curLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);            
            //@"C:\Games\Mafia 3\sds_retail\string_tables";//
            if (anyErrors())
            {
                Console.ReadKey();
                return;
            }
            string sdsName = "";
            string sdsPath="",unpackedSds="",xml="";

            if (args.Length > 0)//drag&drop unpack
            {
                if (Path.GetDirectoryName(args[0]) != curLocation) {Console.WriteLine("Exe not in the same directory with sbs file."); return; }

                sdsName = Path.GetFileNameWithoutExtension(args[0]);
                sdsPath = Path.Combine(curLocation, sdsName + ".sds");
                unpackedSds = Path.Combine(curLocation, sdsName + ".Unpacked");
                xml = Path.Combine(curLocation, sdsName + "-Texts.xml");

                Console.WriteLine(sdsPath);
                Console.WriteLine("Çıkarılıyor...");
                Extractor extman = new Extractor(sdsPath);
                StringExporter strExp = new StringExporter(unpackedSds);
            }
            else  //pack the edited file
            {
                sdsName = getUnpackedName();
                sdsPath = Path.Combine(curLocation, sdsName + ".sds");
                unpackedSds = Path.Combine(curLocation, sdsName + ".Unpacked");
                xml = Path.Combine(curLocation, sdsName + "-Texts.xml");

                if (sdsName == "") return;

                if (File.Exists(unpackedSds))
                {
                    Console.WriteLine(unpackedSds);
                    Console.WriteLine("Geri Paketleniyor...");
                    StringImporter strImp = new StringImporter(unpackedSds, xml);
                    Packer packer = new Packer(unpackedSds, sdsPath);
                }
            }



        }

        static string getUnpackedName()
        {
            string[] fileDirs = Directory.GetFiles(curLocation);
            foreach (var file in fileDirs)
            {
                if (Path.GetExtension(file) == ".Unpacked")
                {
                    return Path.GetFileNameWithoutExtension(file);
                }
            }

            Console.WriteLine("No unpacked file.");
            return "";
        }

        static bool anyErrors()
        {
            int xmlCount=0;
            int sdsCount = 0;
            string[] fileDirs = Directory.GetFiles(curLocation);
            foreach (var file in fileDirs)
            {
                if(Path.GetExtension(file) == ".xml")
                    xmlCount++;
                if (Path.GetExtension(file) == ".sds")
                    sdsCount++;
            }

            if (xmlCount >= 2)
            {
                Console.WriteLine("Birden fazla çıkartılmış dil dosyası dosyası var!");
                return true;
            }
            else if (sdsCount == 0)
            {
                Console.WriteLine("Sds dosyası yok!");
                return true;
            }

            return false;
        }
                
    }

}
