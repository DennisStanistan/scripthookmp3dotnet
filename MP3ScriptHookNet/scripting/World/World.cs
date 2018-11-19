using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MP3.Native;
using MP3.Math;

namespace MP3
{
    public static class World
    {
        public static Ped CreatePed(uint modelHash, Vector3 position, float heading = 0f)
        {
            if(Streaming.RequestModel(modelHash))
            {
                return new Ped(Function.Call<int>(Hash.CREATE_PED, 7, modelHash, position.X, position.Y, position.Z, heading, false, false));
            }

            return null;
        }
    }
}
