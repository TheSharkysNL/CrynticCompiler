using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CrynticCompiler.Extensions;

public static unsafe class UnsafeExtensions
{
	public static nuint GetReferenceDifference<T>(ref T a, ref T b)
    {
        nuint pointerDiff = (nuint)((byte*)Unsafe.AsPointer(ref a) - (byte*)Unsafe.AsPointer(ref b));
        Debug.Assert(pointerDiff % (nuint)Unsafe.SizeOf<T>() == 0);
        return pointerDiff / (nuint)Unsafe.SizeOf<T>();
    }
}

