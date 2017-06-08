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
    /// Packages unpacked sds file.
    /// </summary>
    class Packer
    {
        public Packer(string unpackedSds,string _orgsdsPath)
        {
            PackFromUnpacked(unpackedSds, _orgsdsPath);            
        }

        void PackFromUnpacked(string unpackedSds, string orgsdsPath)
        {
            byte[] unpackedZlibs = File.ReadAllBytes(unpackedSds);
            List<byte[]> paketZlibs = new List<byte[]>();
            int filCount = (int)Math.Floor(((double)unpackedZlibs.Length / 65536));
            for (int i = 0; i <= filCount; i++)
            {                
                int orgLen = 65536;
                int len = orgLen;
                if (i == filCount && unpackedZlibs.Length % orgLen!=0)
                    len = unpackedZlibs.Length%orgLen;
                byte[] zlib = new byte[len];

                for (int j = 0; j < len; j++)
                {
                    zlib[j] = unpackedZlibs[(i*orgLen)+j]; 
                }

                byte[] paket = ZlibStream.CompressBuffer(zlib);
                paketZlibs.Add(paket);
            }

            string sdsnewname = "\\"+Path.GetFileNameWithoutExtension(orgsdsPath) + "_new.sds";
            MemoryStream memsds = new MemoryStream(File.ReadAllBytes(orgsdsPath));
            BinaryReader binred = new BinaryReader(memsds,Encoding.UTF8);                        
            BinaryWriter binwr = new BinaryWriter(File.Create(Path.GetDirectoryName(orgsdsPath) + sdsnewname));

            
            binwr.Write(binred.ReadBytes(16));//Top header

            //For header bytes to use fnvHash                                  
            byte[] headerBytes = binred.ReadBytes(52);
            byte[] lenByte = BitConverter.GetBytes(unpackedZlibs.Length);
            //Array.Reverse(lenByte);
            //new unpacked data size
            headerBytes[12] = lenByte[0]; headerBytes[13] = lenByte[1];
            headerBytes[14] = lenByte[2]; headerBytes[15] = lenByte[3];

            binwr.Write(headerBytes);//write 52 byte
            binred.ReadUInt32();//skip old fnvhash
            uint fnvHash = FNV32.Fnvhash32(headerBytes);//create new fnvhash
            binwr.Write(fnvHash);
            binwr.Write(binred.ReadBytes(45));//go direct to Uezloffset+9

            for (int i = 0; i < paketZlibs.Count; i++)
            {
                binwr.Write(paketZlibs[i].Length+32);//paketliSize+32
                binwr.Write((byte)1);//1byte
                int unpaketLen = i != paketZlibs.Count - 1 ? 65536 : unpackedZlibs.Length % 65536;
                binwr.Write(unpaketLen);//acilmisPaketsize
                binwr.Write((int)32);//fixed
                binwr.Write((int)65536);//fixed
                binwr.Write((int)135200769);//fixed
                binwr.Write((int)paketZlibs[i].Length);//paketliSize
                binwr.Write((int)0);binwr.Write((int)0);binwr.Write((int)0);//12byte 0
                binwr.Write(paketZlibs[i]);//data                
            }
            binwr.Write(new byte[] { 0, 0, 0, 0, 0 });//last 5 zeros

            binred.Close();
            binwr.Close();

            string sdsname = "\\"+Path.GetFileNameWithoutExtension(orgsdsPath) + ".sds";
            File.Delete(orgsdsPath);
            File.Move(Path.GetDirectoryName(orgsdsPath) + sdsnewname, Path.GetDirectoryName(orgsdsPath) + sdsname);
        }

    }
}
