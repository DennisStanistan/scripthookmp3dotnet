using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MP3ScriptHookNet;
using MP3ScriptHookNet.Core;

using static MP3ScriptHookNet.Core.NativeMethods;

namespace MP3
{
    namespace Native
    {
        public interface INativeValue
        {
            uint NativeValue { get; set; }
        }

        public static class Memory
        {
            /// <summary>
            /// A dictionary that stores strings memory addresses.
            /// </summary>
            private static Dictionary<IntPtr, string> memoryStringPointers = new Dictionary<IntPtr, string>();

            /// <summary>
            /// Allocates a string in process' memory and returns it's memory address.
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            internal static IntPtr AllocStringToMemory(string str)
            {
                IntPtr addr = IntPtr.Zero;
                IntPtr handle = Instance.ProcessHandle;
                if (memoryStringPointers.ContainsValue(str))
                    return memoryStringPointers.FirstOrDefault(x => x.Value == str).Key;

                addr = VirtualAllocEx(handle, IntPtr.Zero, (uint)str.Length + 4, AllocationType.Commit, MemoryProtection.ExecuteReadWrite);

                if (addr == IntPtr.Zero)
                {
                    Log.Print("ERROR", "Failed to allocate memory!");
                    return IntPtr.Zero;
                }

                UIntPtr bytesWritten = UIntPtr.Zero;

                if (!NativeMethods.WriteProcessMemory(handle, addr, Encoding.ASCII.GetBytes(str), (uint)str.Length + 4, out bytesWritten))
                {
                    Log.Print("Memory.WriteStringToMemory ERROR", "Could not write process memory!");
                    return IntPtr.Zero;
                }
                else memoryStringPointers.Add(addr, str);

                if (str.Length + 4 != (int)bytesWritten)
                {
                    Log.Print("Memory.WriteStringToMemory WARNING", "String length is not equal to the amount of bytes written.");
                }

                return addr;
            }

            internal static unsafe string ReadNullTerminatedString(IntPtr ptr)
			{
                // TODO: write a better method of acquiring the string from the memory address.
                List<byte> stringBytes = new List<byte>();
                IntPtr currentPtr = new IntPtr(ptr.ToInt32());
                byte b = Marshal.ReadByte(currentPtr);

                while(b != 0x00)
                {
                    stringBytes.Add(b);
                    IntPtr.Add(currentPtr, 1);
                }

                return Encoding.ASCII.GetString(stringBytes.ToArray());
			}
        }

        public static class Function
        {
            [DllImport("ScriptHook.dll", ExactSpelling = true, EntryPoint = "?nativeInit@@YAXK@Z")]
            static extern void NativeInit(uint hash);
            [DllImport("ScriptHook.dll", ExactSpelling = true, EntryPoint = "?nativePush32@@YAXI@Z")]
            static extern void NativePush32(uint val);
            [DllImport("ScriptHook.dll", ExactSpelling = true, EntryPoint = "?nativeCall@@YAPAIXZ")]
            static unsafe extern uint* NativeCall();

            public static T Call<T>(Hash hash, params object[] o)
            {
                NativeInit((uint)hash);
                foreach(var arguement in o)
                {
                    NativePush32(ObjectToNative(arguement));
                }

                unsafe { return (T)(ObjectFromNative(typeof(T), NativeCall())); }
            }

            public static void Call(Hash hash, params object[] o)
            {
                NativeInit((uint)hash);
                foreach (var arguement in o)
                {
                    NativePush32(ObjectToNative(arguement));
                }

                unsafe { NativeCall(); }
            }

            /// <summary>
            /// Converts a managed object to a native value.
            /// </summary>
            /// <param name="value">The object to convert.</param>
            /// <returns>A native value representing the input <paramref name="value"/>.</returns>
            internal static unsafe uint ObjectToNative(object value)
            {
                if (ReferenceEquals(value, null))
                    return 0;

                var type = value.GetType();

                if (type.IsEnum)
                    value = Convert.ChangeType(value, type = Enum.GetUnderlyingType(type));

                if(type == typeof(bool))
                    return (uint)(((bool)value) ? 1 : 0);

                if (type == typeof(int))
                    return (uint)((int)value);

                if (type == typeof(uint))
                {
                    return (uint)value;
                }
                if (type == typeof(float))
                {
                    return BitConverter.ToUInt32(BitConverter.GetBytes((float)value), 0);
                }
                if (type == typeof(double))
                {
                    return BitConverter.ToUInt32(BitConverter.GetBytes((float)((double)value)), 0);
                }

                if (type == typeof(string))
                {
                    // TODO: figure out why this crashes the game
                    return (uint)Memory.AllocStringToMemory((string)value).ToInt32(); // crashes
                    
                }
                if (type == typeof(IntPtr))
                {
                    return (uint)((IntPtr)value).ToInt32();
                }

                if (typeof(INativeValue).IsAssignableFrom(type))
                {
                    return ((INativeValue)value).NativeValue;
                }

                throw new InvalidCastException(string.Concat("Unable to cast object of type '", type.FullName, "' to native value"));
            }

            /// <summary>
			/// Converts a native value to a managed object.
			/// </summary>
			/// <param name="type">The type to convert to.</param>
			/// <param name="value">The native value to convert.</param>
			/// <returns>A managed object representing the input <paramref name="value"/>.</returns>
            internal static unsafe object ObjectFromNative(Type type, uint* value)
            {
                if (type.IsEnum)
                {
                    type = Enum.GetUnderlyingType(type);
                }

                if (type == typeof(bool))
                {
                    return *(int*)(value) != 0;
                }
                if (type == typeof(int))
                {
                    return *(int*)(value);
                }
                if (type == typeof(uint))
                {
                    return *(uint*)(value);
                }
                if (type == typeof(long))
                {
                    return *(long*)(value);
                }
                if (type == typeof(ulong))
                {
                    return *(value);
                }
                if (type == typeof(float))
                {
                    return *(float*)(value);
                }
                if (type == typeof(double))
                {
                    return (double)(*(float*)(value));
                }

                if (type == typeof(string))
                {
                    var address = (char*)(*value);

                    if (address == null)
                    {
                        return String.Empty;
                    }

                    return Memory.ReadNullTerminatedString(new IntPtr(address));
                }

                if (type == typeof(IntPtr))
                {
                    return new IntPtr((long)(value));
                }

                if (type == typeof(Math.Vector2))
                {
                    var data = (float*)(value);

                    return new Math.Vector2(data[0], data[2]);

                }
                if (type == typeof(Math.Vector3))
                {
                    var data = (float*)(value);

                    return new Math.Vector3(data[0], data[2], data[4]);

                }

                if (typeof(INativeValue).IsAssignableFrom(type))
                {
                    // Warning: Requires classes implementing 'INativeValue' to repeat all constructor work in the setter of 'NativeValue'
                    var result = (INativeValue)(System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type));
                    result.NativeValue = *value;

                    return result;
                }

                throw new InvalidCastException(String.Concat("Unable to cast native value to object of type '", type.FullName, "'"));
            }
        }
    }
}
