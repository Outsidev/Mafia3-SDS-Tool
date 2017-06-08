using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia3SDSTool
{
    /// <summary>
    /// Finds and extracts all zlib files in sds file.
    /// </summary>
    class Extractor
    {
        public Extractor(string _sdsPath)
        { 
            if(!File.Exists(_sdsPath+".Backup"))           
                File.Copy(_sdsPath,_sdsPath+".Backup",true);
            UnpackZlibs(_sdsPath);
        }

        public void UnpackZlibs(string filePath)
        {
            BinaryReader sbsRead = new BinaryReader(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));
            List<byte[]> zlibFiles = new List<byte[]>();

            sbsRead.BaseStream.Position = 20;
            uint sdsDataStartPos = sbsRead.ReadUInt32();
            sbsRead.BaseStream.Position = sdsDataStartPos;
            sbsRead.ReadUInt32(); //UEzL
            sbsRead.ReadUInt32(); //65536fix
            sbsRead.ReadChar(); //4 fix
            sbsRead.BaseStream.Position += 21;//37bytelık header 21.byte data boyutu

            while (sbsRead.BaseStream.Position < sbsRead.BaseStream.Length)
            {                
                uint zlibLen = sbsRead.ReadUInt32();
                sbsRead.BaseStream.Position += 12; //nulls
                zlibFiles.Add(sbsRead.ReadBytes((int)zlibLen));                
                sbsRead.BaseStream.Position += 21;
            }
            sbsRead.Close();

            //zlibleri çıkar
            List<byte[]> unpackedZlibs = new List<byte[]>();
            for (int i = 0; i < zlibFiles.Count; i++)
            {
                byte[] zlib = zlibFiles[i];
                byte[] unpaket = ZlibStream.UncompressBuffer(zlib);
                unpackedZlibs.Add(unpaket);
            }

            List<byte> allBytes = new List<byte>();
            for (int i = 0; i<unpackedZlibs.Count; i++)
            {
                for (int z = 0; z < unpackedZlibs[i].Length; z++)
                {
                    allBytes.Add(unpackedZlibs[i][z]);                    
                }
            }

            string filName = Path.GetFileNameWithoutExtension(filePath) + ".Unpacked";
            string newfileDir = Path.GetDirectoryName(filePath) + "\\" + filName;
            File.WriteAllBytes(newfileDir, allBytes.ToArray());            
        }
    }
}
