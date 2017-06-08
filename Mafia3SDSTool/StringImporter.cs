using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia3SDSTool
{
    /// <summary>
    /// Imports strings in xml file to unpacked sds file.
    /// </summary>
    class StringImporter
    {
        List<SubText> subTexts;
        public StringImporter(string unpackedSds,string xmlPath)
        {
            subTexts = new List<SubText>();
            GetStringsfromXML(xmlPath);
            SetStrings(unpackedSds,xmlPath);
        }

        void GetStringsfromXML(string xmlPath)
        {
            SubText sub = new SubText();
            int say = 0;
            foreach (string line in File.ReadLines(xmlPath, Encoding.UTF8))
            {
                say++;
                if (line == "-EndFile-")
                {
                    sub = new SubText();
                    sub.StringText = line;
                    subTexts.Add(sub);
                }
                else if (line != "" && line[line.Length - 1] == '>' && line[line.Length - 2] == '>')
                {
                    sub = new SubText();
                    string[] headPos = line.Split(new string[] { ">>" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < headPos.Length; i++)
                    {
                        int aa = Int32.Parse(headPos[i]);
                        sub.TopPositions.Add(aa);
                    }
                }
                else 
                {
                    //yeni satırlar için /for new lines
                    string datline = line;
                    if (datline.Contains("(0D0A)"))
                    {
                        datline = line.Replace("(0D0A)", "\r\n");
                    }
                    sub.StringText = datline;
                    sub.ByteText = Encoding.UTF8.GetBytes(datline);
                    subTexts.Add(sub);
                }

            }
        }

        void SetStrings(string unpackedSds,string xmlPath)
        {
            BinaryReader binred = new BinaryReader(File.Open(unpackedSds,FileMode.Open,FileAccess.Read,FileShare.Read), Encoding.UTF8);
            BinaryWriter binwrit = new BinaryWriter(File.Create(unpackedSds + ".newStrs.sds"), Encoding.UTF8);

            binwrit.Write(binred.ReadBytes(498));
            int strIndex = 0;
            uint dizinBoyutuPos= 0;
            while (binred.BaseStream.Position < binred.BaseStream.Length)
            {
                long startPos = binred.BaseStream.Position;
                binred.BaseStream.Position += 4 * 2;
                int dizinWordCount = binred.ReadInt32();
                if (binred.BaseStream.Position + dizinWordCount >= binred.BaseStream.Length)
                {
                    binred.BaseStream.Position = startPos;
                    while (binred.BaseStream.Position < binred.BaseStream.Length)
                        binwrit.Write(binred.ReadByte());
                    break;//son
                }

                binred.BaseStream.Position += dizinWordCount;

                if (binred.ReadUInt32() != 0)
                {
                    binred.BaseStream.Position -= 4;

                    long lastLen2 = binred.BaseStream.Position - startPos;
                    binred.BaseStream.Position = startPos;
                    binwrit.Write(binred.ReadBytes((int)lastLen2));

                    continue;
                }                

                long lastLen = binred.BaseStream.Position - startPos;
                binred.BaseStream.Position = startPos;
                binwrit.Write(binred.ReadBytes((int)lastLen));

                //birust dizinin boyutunu yaz
                if (dizinBoyutuPos != 0)
                {
                    uint endPos = (uint)binwrit.BaseStream.Position-4;
                    binwrit.BaseStream.Position = dizinBoyutuPos;
                    binwrit.Write(endPos-dizinBoyutuPos+4);
                    binwrit.BaseStream.Position = endPos+4;
                }

                dizinBoyutuPos = (uint)binwrit.BaseStream.Position;
                binred.ReadUInt32();
                binwrit.Write(1234);

                binwrit.Write(binred.ReadBytes(10));//10 geç

                uint dizinBoyutuPos2 = (uint)binwrit.BaseStream.Position;
                binwrit.Write(binred.ReadUInt32());

                binwrit.Write(binred.ReadBytes(12+12));//24geç

                uint textSonuPos = (uint)binwrit.BaseStream.Position;
                binred.ReadUInt32();
                binwrit.Write(1234);

                binwrit.Write(binred.ReadBytes(24));//36geç
                uint posLensSize = binred.ReadUInt32();// multp by 16
                uint posLensSize2 = binred.ReadUInt32();//multp by 16
                binwrit.Write(posLensSize);
                binwrit.Write(posLensSize2);
                binwrit.Write(binred.ReadBytes(4));

                long headerPos = binwrit.BaseStream.Position;
                long redHeaderPos = binred.BaseStream.Position;
                binwrit.Write(binred.ReadBytes( (int)(posLensSize*16) ));
                binred.BaseStream.Position -= 8;
                binred.BaseStream.Position +=binred.ReadUInt32()-4;
                while (binred.ReadByte() != 1) ;binred.BaseStream.Position += 3;
               //text posları yazdı, şu an textleri yazma posda

                while(subTexts[strIndex].StringText != "-EndFile-")
                {
                    SubText sub = subTexts[strIndex];
                    sub.BottomPos = (uint)binwrit.BaseStream.Position;
                    binwrit.Write(sub.ByteText);
                    binwrit.Write((byte)0x0);
                    long tut = binwrit.BaseStream.Position;
                    for (int z = 0; z < sub.TopPositions.Count; z++)
                    {
                        binwrit.BaseStream.Position = headerPos + (16 * sub.TopPositions[z])+8;
                        uint toppos = (uint)(sub.BottomPos - binwrit.BaseStream.Position);
                        binwrit.Write(toppos);                        
                    }
                    binwrit.BaseStream.Position = tut;
                    strIndex++;                    
                }
                //texsonuposu yazdır
                uint lastPo = (uint)binwrit.BaseStream.Position;
                binwrit.BaseStream.Position = textSonuPos;
                binwrit.Write(lastPo-textSonuPos-4);
                binwrit.BaseStream.Position = lastPo;

                binwrit.Write((uint)1);
                strIndex++;


            }

            if (dizinBoyutuPos != 0)
            {
                uint endPos = (uint)binwrit.BaseStream.Position;
                binwrit.BaseStream.Position = dizinBoyutuPos;
                binwrit.Write(endPos - dizinBoyutuPos + 4);
                binwrit.BaseStream.Position = endPos + 4;
            }
            binred.Close();
            binwrit.Close();

            File.Delete(unpackedSds);
            File.Move(unpackedSds + ".newStrs.sds", unpackedSds);

        }
    }
}
