using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static MP3ScriptHookNet.Core.NativeMethods;

namespace MP3ScriptHookNet
{
    public static class Instance
    {
        public static Process GameProcess { get; set; }

        public static IntPtr ProcessHandle
        {
            get
            {
                if (GameProcess == null)
                    return IntPtr.Zero;

                return OpenProcess(2035711U, false, GameProcess.Id);
            }
        }

        public static uint HashString(string str)
        {
            uint hash = 0;

            for (int i = 0; i < str.Length; ++i)
            {
                hash += char.ToLower(str[i]);
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }

            hash += (hash << 3);
            hash ^= (hash >> 11);
            hash += (hash << 15);

            return hash;
        }
    }
}
