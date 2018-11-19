using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MP3.Native;

namespace MP3
{ 
    public enum Gender
    {
        Male,
        Female
    }
    public sealed class Ped : INativeValue, IDeletable
    {
        public int Handle { get; private set; }
        public uint NativeValue { get; set; }

        public Gender Gender
        {
            get
            {
                return Function.Call<bool>(Hash.IS_PED_MALE, Handle) ? Gender.Male : Gender.Female;
            }
        }

        public Ped(int handle)
        {
            Handle = handle;
        }

        public bool Exists()
        {
            return Function.Call<bool>(Hash.DOES_PED_EXIST, Handle);
        }

        public void Delete()
        {
            if (!Exists())
                throw new Exception(string.Format("Ped doesn't exist, can't delete it! INativeValue handle: {0:X8}", Handle));

            Function.Call(Hash.DELETE_PED, Handle);
        }
    }
}
