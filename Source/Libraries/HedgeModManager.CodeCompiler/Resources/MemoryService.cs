using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace HMMCodes
{
    public static unsafe class MemoryService
    {
        [DllImport("kernel32.dll")]
        public static extern bool VirtualProtect(IntPtr lpAddress,
                IntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        public static long ModuleBase = (long)GetModuleHandle(null);

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey);

        public static dynamic MemoryProvider;

        public static void RegisterProvider(object provider)
        {
            MemoryProvider = provider;
        }

        public static bool IsMemoryWritable(nint address) 
            => address > (nint)ModuleBase;

        public static bool IsAligned(nint address)
            => (address % (nint)AlignOf<nint>()) == 0;

        public static long ASLR(long address)
             => ModuleBase + (address - (IntPtr.Size == 8 ? 0x140000000 : 0x400000));

        public static void Write(IntPtr address, IntPtr dataPtr, IntPtr length)
        {
            if (!IsMemoryWritable(address))
            {
                return;
            }

            if (IsAligned(address) && IsAligned(dataPtr))
            {
                Unsafe.CopyBlock(dataPtr.ToPointer(), address.ToPointer(), (uint)length);
            }
            else
            {
                Unsafe.CopyBlockUnaligned(dataPtr.ToPointer(), address.ToPointer(), (uint)length);
            }
        }

        public static void Write<T>(IntPtr address, in T data)
        {
            if (!IsMemoryWritable(address))
            {
                return;
            }

            Unsafe.Copy(address.ToPointer(), data);
        }

        public static void Write<T>(long address, params T[] data)
        {
            if (!IsMemoryWritable((IntPtr)address))
            {
                return;
            }
            
            Write((IntPtr)address, (IntPtr)Unsafe.AsPointer<T>(ref data[0]), (nint)(Unsafe.SizeOf<T>() * data.Length));
        }

        public static void Write<T>(long address, in T data)
            => Write<T>((IntPtr)address, data);

        public static byte[] Read(IntPtr address, IntPtr length)
        {
            var buffer = new byte[(int)length];
            Write((IntPtr)Unsafe.AsPointer(ref buffer[0]), address, length);
            return buffer;
        }

        public static ref T Read<T>(IntPtr address) where T : unmanaged
            => ref Unsafe.AsRef<T>(address);

        public static ref T Read<T>(long address) where T : unmanaged
            => ref Read<T>((IntPtr)address);

        public static byte[] Assemble(string source)
            => MemoryProvider.AssembleInstructions(source);

        public static long GetPointer(long address, params long[] offsets)
        {
            if (address == 0)
                return 0;

            var result = (long)(*(void**)address);

            if (result == 0)
                return 0;

            if (offsets.Length > 0)
            {
                for (int i = 0; i < offsets.Length - 1; i++)
                {
                    result = (long)((void*)(result + offsets[i]));
                    result = (long)(*(void**)result);
                    if (result == 0)
                        return 0;
                }

                return result + offsets[offsets.Length - 1];
            }

            return result;
        }

        public static void WriteProtected(IntPtr address, IntPtr dataPtr, IntPtr length)
        {
            VirtualProtect((IntPtr)address, length, 0x04, out uint oldProtect);
            Write(address, dataPtr, length);
            VirtualProtect((IntPtr)address, length, oldProtect, out _);
        }

        public static void WriteProtected<T>(long address, in T data) where T : unmanaged
        {
            VirtualProtect((IntPtr)address, (IntPtr)sizeof(T), 0x04, out uint oldProtect);
            Write<T>(address, data);
            VirtualProtect((IntPtr)address, (IntPtr)sizeof(T), oldProtect, out _);
        }

        public static void WriteProtected<T>(long address, params T[] data) where T : unmanaged
        {
            VirtualProtect((IntPtr)address, (IntPtr)(sizeof(T) * data.Length), 0x04, out uint oldProtect);
            Write<T>(address, data);
            VirtualProtect((IntPtr)address, (IntPtr)(sizeof(T) * data.Length), oldProtect, out _);
        }

        public static void WriteAsmHook(string instructions, long address, HookBehavior behavior = HookBehavior.After, HookParameter parameter = HookParameter.Jump)
            => MemoryProvider.WriteASMHook(instructions, (IntPtr)address, (int)behavior, (int)parameter);

        public static void WriteAsmHook(string instructions, long address, HookParameter parameter, HookBehavior behavior)
            => WriteAsmHook(instructions, address, behavior, parameter);

        public static void WriteAsmHook(long address, HookBehavior behavior, HookParameter parameter, params string[] instructions)
            => WriteAsmHook(string.Join("\r\n", instructions), address, behavior, parameter);

        public static void WriteAsmHook(long address, HookBehavior behavior, params string[] instructions)
            => WriteAsmHook(string.Join("\r\n", instructions), address, behavior, HookParameter.Jump);

        public static IntPtr ScanSignature(byte[] pattern, string mask)
            => MemoryProvider.ScanSignature(pattern, mask);

        public static uint NopInstructions(long address, uint count)
            => MemoryProvider.NopInstructions((IntPtr)address, count);

        public static uint NopInstruction(long address)
            => NopInstructions(address, 1);

        public static void WriteNop(long address, long count)
        {
            for (long i = 0; i < count; i++)
                WriteProtected<byte>(address + i, 0x90);
        }

        public static byte[] MakePatternFromString(string pattern)
        {
            byte[] patBytes = new byte[pattern.Length];

            for (int i = 0; i < patBytes.Length; i++)
                patBytes[i] = (byte)pattern[i];

            return patBytes;
        }

        public static long ScanSignature(params string[] sigs)
        {
            if (sigs.Length % 2 != 0)
                return 0;

            for (int i = 0; i < sigs.Length - 1; i++)
            {
                var result = ScanSignature(MakePatternFromString(sigs[i * 2 + 0]), sigs[i * 2 + 1]).ToInt64();
                if (result != 0)
                    return result;
            }
            return 0;
        }

        public static bool IsKeyDown(Keys key)
            => (GetAsyncKeyState(key) & 1) != 0;
        
        public static int SizeOf<T>() => Unsafe.SizeOf<T>();
        public static int AlignOf<T>() => Unsafe.SizeOf<AlignmentHelper<T>>() - Unsafe.SizeOf<T>();
        private struct AlignmentHelper<T>
        {
            private byte b;
            private T value;
        };
    }

    public enum HookBehavior
    {
        After,
        Before,
        Replace
    }

    public enum HookParameter
    {
        Jump = 0,
        Call = 1,
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class LibraryInitializerAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class LibraryUpdateAttribute : Attribute { }

    public class StartupUtil
    {
        public static List<Action> UpdateEvents = new List<Action>();
        public static bool IsLoaderExecutable() => true;

        public static void InitStatic()
        {
            var assembly = typeof(StartupUtil).Assembly;
            foreach (var type in assembly.GetExportedTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
                {
                    if (method.GetCustomAttribute<LibraryInitializerAttribute>() != null)
                    {
                        method.Invoke(null, null);
                    }

                    if (method.GetCustomAttribute<LibraryUpdateAttribute>() != null)
                    {
                        UpdateEvents.Add((Action)Delegate.CreateDelegate(typeof(Action), method));
                    }
                }
            }
        }

        public static void OnFrameStatic()
        {
            foreach (var ev in UpdateEvents)
            {
                ev();
            }
        }

        public void Init()
        {
            InitStatic();
        }

        public void OnFrame()
        {
            OnFrameStatic();
        }
    }
}

namespace System.Runtime.CompilerServices
{
    public static unsafe class Unsafe
    {
        public static TTo BitCast<TFrom, TTo>(TFrom source)
            where TFrom : struct
            where TTo : struct
        {
            return ReadUnaligned<TTo>(ref As<TFrom, byte>(ref source));
        }

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern void* AsPointer<T>(ref T value);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern int SizeOf<T>();

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern T As<T>(object? o) where T : class?;

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern ref TTo As<TFrom, TTo>(ref TFrom source);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern ref T AsRef<T>(IntPtr source);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern ref T AsRef<T>(void* source);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern ref T AsRef<T>(in T source);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern void Copy<T>(void* destination, in T source);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern void Copy<T>(ref T destination, void* source);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern void CopyBlock(void* destination, void* source, uint byteCount);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern void CopyBlock(ref byte destination, ref byte source, uint byteCount);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern void CopyBlockUnaligned(void* destination, void* source, uint byteCount);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern void CopyBlockUnaligned(ref byte destination, ref byte source, uint byteCount);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern ref T Add<T>(ref T source, int n);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern ref T Add<T>(ref T source, IntPtr n);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern void* Add<T>(void* source, int n);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern void* Add<T>(void* source, nint n);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern void SkipInit<T>(out T value);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern bool AreSame<T>(ref T left, ref T right);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern void InitBlock(void* startAddress, byte value, uint byteCount);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern void InitBlock(ref byte startAddress, byte value, uint byteCount);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern void InitBlockUnaligned(void* startAddress, byte value, uint byteCount);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern void InitBlockUnaligned(ref byte startAddress, byte value, uint byteCount);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern T Read<T>(void* source);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern T Read<T>(ref byte source);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern T ReadUnaligned<T>(void* source);

        [CompilerGenerated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern T ReadUnaligned<T>(ref byte source);
    }
}