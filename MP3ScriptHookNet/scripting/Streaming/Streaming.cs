using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MP3.Native;

namespace MP3
{
    public static class Streaming
    {
        public static bool RequestModel(ulong modelHash, uint requestTimeout = 1000)
        {
            if (!Function.Call<bool>(Hash.IS_MODEL_IN_CDIMAGE, modelHash))
                return false;

            if (Function.Call<bool>(Hash.HAS_MODEL_LOADED))
                return true;

            Function.Call(Hash.REQUEST_MODEL, modelHash);

            uint ticks = 0;

            while(!Function.Call<bool>(Hash.HAS_MODEL_LOADED, modelHash))
            {
                if (++ticks >= requestTimeout)
                    return false;
            }

            return true;
        }
    }
}
