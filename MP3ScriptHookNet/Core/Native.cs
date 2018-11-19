using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
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

        public static unsafe class Memory
        {
            internal static IntPtr NullString => StringToCoTaskMemUTF8(string.Empty);
            private static List<IntPtr> _pinnedStrings = new List<IntPtr>();

            public static void CleanupPinnedStrings()
            {
                _pinnedStrings.Clear();
            }

            public static IntPtr PinString(string str)
            {
                IntPtr handle = StringToCoTaskMemUTF8(str);

                if (handle == IntPtr.Zero)
                {
                    return NullString;
                }
                else
                {
                    _pinnedStrings.Add(handle);

                    return handle;
                }
            }

            internal static string ReadNullTerminatedString(IntPtr ptr)
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

            internal static IntPtr StringToCoTaskMemUTF8(string s)
            {
                if (s == null)
                {
                    return IntPtr.Zero;
                }
                else
                {
                    unsafe
                    {
                        byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(s);
                        IntPtr dest = Marshal.AllocCoTaskMem(utf8Bytes.Length + 1);
                        if (dest == IntPtr.Zero)
                        {
                            throw new OutOfMemoryException();
                        }

                        Marshal.Copy(utf8Bytes, 0, dest, utf8Bytes.Length);
                        ((byte*)dest.ToPointer())[utf8Bytes.Length] = 0;

                        return dest;
                    }
                }
            }

            internal static string PtrToStringUTF8(IntPtr ptr, int byteLen)
            {
                if (byteLen < 0)
                {
                    throw new ArgumentException(null, nameof(byteLen));
                }
                else if (IntPtr.Zero == ptr)
                {
                    return null;
                }
                else if (byteLen == 0)
                {
                    return string.Empty;
                }
                else
                {
                    byte* pByte = (byte*)ptr.ToPointer();
                    return System.Text.Encoding.UTF8.GetString(pByte, byteLen);
                }
            }

            internal static string PtrToStringUTF8(IntPtr ptr)
            {
                if (IntPtr.Zero == ptr)
                {
                    return null;
                }
                unsafe
                {
                    byte* address = (byte*)ptr.ToPointer();
                    int len = 0;

                    while (address[len] != 0)
                        ++len;

                    return PtrToStringUTF8(ptr, len);
                }
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
                    return (uint)Memory.PinString((string)(value)).ToInt32();

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

                    return Memory.PtrToStringUTF8(new IntPtr(address));
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
