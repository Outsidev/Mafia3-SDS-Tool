using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia3SDSTool
{
    /// <summary>
    /// Extracts all strings to a text file in unpacked sds file.
    /// </summary>
    class StringExporter
    {
        public StringExporter(string unpackedSds)
        {
            GetStrings(unpackedSds);
        }

        public void GetStrings(string unpackedSds)
        {
            int startPos = 498;//fixed size           
            byte[] bb = File.ReadAllBytes(unpackedSds);            
            BinaryReader binred = new BinaryReader(new MemoryStream(bb),Encoding.UTF8);
            binred.BaseStream.Position = startPos;
            List<string> allStrings = new List<string>();
            while (binred.BaseStream.Position < binred.BaseStream.Length)
            {
                binred.BaseStream.Position+=4*2;
                int dirLength = binred.ReadInt32();
                if (binred.BaseStream.Position+dirLength >= binred.BaseStream.Length)
                    break;

                binred.BaseStream.Position += dirLength;
                if (binred.ReadUInt32() != 0)
                {
                    //0 değil ise, bir dosya dizini daha var
                    binred.BaseStream.Position -= 4;
                    continue;
                }
                uint dizinBoyutu = binred.ReadUInt32();
                binred.BaseStream.Position += 66;
                int posLensCount = binred.ReadInt32();
                binred.BaseStream.Position += 8;
                long headerStartPos = binred.BaseStream.Position;
                binred.BaseStream.Position += 8;
                uint textBaslangiclari = binred.ReadUInt32();
                binred.BaseStream.Position +=textBaslangiclari - 4;

                
                string _text = "";
                uint lastLoc=(uint)binred.BaseStream.Position;
                Dictionary<uint,SubText> balaba = new Dictionary<uint, SubText>();
                while (binred.ReadInt32() != 1)
                {
                    binred.BaseStream.Position -=4;
                    byte kar = binred.ReadByte();                    
                    if (kar == 0x0)
                    {
                        SubText st = new SubText();
                        st.StringText = _text;
                        st.BottomPos = lastLoc;
                        balaba.Add(lastLoc,st);
                        _text = "";
                        lastLoc = (uint)binred.BaseStream.Position;

                    }
                    else
                    {
                        if(kar == 0x0D)
                        {
                            binred.ReadByte();
                            _text += "(0D0A)";
                        }
                        else
                        {
                            binred.BaseStream.Position -= 1;
                            _text += binred.ReadChar();
                        }
                                                
                    }
                }

                long endOfText = binred.BaseStream.Position;
                for (int i = 0; i < posLensCount; i++)
                {
                    long headPos = headerStartPos+ 16 * i;
                    long bottomLocPos = headPos + 8;
                    binred.BaseStream.Position = bottomLocPos;
                    uint bottomPos = (uint)binred.BaseStream.Position + binred.ReadUInt32();                  
                    balaba[bottomPos].TopPositions.Add(i);
                }
                
                foreach(var dman in balaba)
                {
                    SubText sub = dman.Value;
                    string mofo = "";
                    for (int z = 0; z < sub.TopPositions.Count; z++)
                    {
                        mofo += sub.TopPositions[z]+">>";
                    }
                    allStrings.Add(mofo);
                    allStrings.Add(sub.StringText);
                }
                allStrings.Add("-EndFile-");
                binred.BaseStream.Position = endOfText;
            }
            
            binred.Close();

            string filName = Path.GetFileNameWithoutExtension(unpackedSds) + "-Texts.xml";
            string newfileDir = Path.GetDirectoryName(unpackedSds) + "\\" + filName;
            File.WriteAllLines(newfileDir,allStrings);
        }
    }
}
