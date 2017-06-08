using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia3SDSTool
{
    class FNV32
    {
        public const uint hashKey = 2166136261;
        public static uint Fnvhash32(byte[] buffer)
        {
            uint hash = hashKey;
            for (int i = 0; i < buffer.Length; i++)
            {
                hash *= 0x1000193;
                hash ^= buffer[i];
            }

            return hash;
        }
    }
}
