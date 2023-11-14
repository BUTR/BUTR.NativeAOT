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

#nullable enable
#if !BUTR_NATIVEAOT_ENABLE_WARNING
#pragma warning disable
#endif

namespace BUTR.NativeAOT.Shared
{
    using global::Microsoft.Win32.SafeHandles;
    using global::System;
    using global::System.Runtime.InteropServices;

    internal sealed unsafe class SafeStringMallocHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public static implicit operator param_string*(SafeStringMallocHandle handle) => (param_string*) handle.handle.ToPointer();
        public static implicit operator param_json*(SafeStringMallocHandle handle) => (param_json*) handle.handle.ToPointer();
        public static implicit operator char*(SafeStringMallocHandle handle) => (char*) handle.handle.ToPointer();

        public readonly bool IsOwner;

        public SafeStringMallocHandle() : base(true) { }
        public SafeStringMallocHandle(char* ptr, bool isOwner) : base(isOwner)
        {
            handle = new IntPtr(ptr);
            IsOwner = isOwner;
        }

        protected override bool ReleaseHandle()
        {
            if (handle != IntPtr.Zero)
                Allocator.Free(handle.ToPointer());
            return true;
        }

        public ReadOnlySpan<char> ToSpan() => MemoryMarshal.CreateReadOnlySpanFromNullTerminated((char*) handle.ToPointer());

        public override string ToString() => ToSpan().ToString();
    }
}
#nullable restore
#if !BUTR_NATIVEAOT_ENABLE_WARNING
#pragma warning restore
#endif