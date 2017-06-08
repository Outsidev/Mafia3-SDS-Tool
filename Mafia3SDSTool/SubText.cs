using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia3SDSTool
{
    class SubText
    {
        public List<int> TopPositions;
        public byte[] ByteText;
        public string StringText;
        public uint BottomPos;

        public SubText()
        {
            TopPositions = new List<int>();
        }
    }
}
