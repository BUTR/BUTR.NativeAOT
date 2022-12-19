#region License
// MIT License
//
// Copyright (c) Bannerlord's Unofficial Tools & Resources
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion

#if !BUTR_NATIVEAOT_ENABLE_WARNING
#nullable enable
#pragma warning disable
#endif

namespace BUTR.NativeAOT.Shared
{
    using global::System;
    using global::System.Runtime.InteropServices;
    using global::Microsoft.Win32.SafeHandles;

    internal unsafe class SafeStringMallocHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public static implicit operator ReadOnlySpan<char>(SafeStringMallocHandle handle) =>
            MemoryMarshal.CreateReadOnlySpanFromNullTerminated((char*) handle.handle.ToPointer());

        public SafeStringMallocHandle(): base(true) { }
        public SafeStringMallocHandle(char* ptr): base(true)
        {
            handle = new IntPtr(ptr);
            var b = false;
            DangerousAddRef(ref b);
        }

        protected override bool ReleaseHandle()
        {
            if (handle != IntPtr.Zero)
                NativeMemory.Free(handle.ToPointer());
            return true;
        }
    }
}
#if !BUTR_NATIVEAOT_ENABLE_WARNING
#pragma warning restore
#nullable restore
#endif