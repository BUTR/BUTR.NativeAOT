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

    internal sealed unsafe class SafeDataMallocHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public static implicit operator param_data*(SafeDataMallocHandle handle) => (param_data*) handle.handle.ToPointer();
        public static implicit operator byte*(SafeDataMallocHandle handle) => (byte*) handle.handle.ToPointer();

        public readonly int Length;
        public readonly bool IsOwner;

        public SafeDataMallocHandle() : base(true) { }
        public SafeDataMallocHandle(byte* ptr, int length, bool isOwner) : base(isOwner)
        {
            handle = new IntPtr(ptr);
            Length = length;
            IsOwner = isOwner;
        }

        protected override bool ReleaseHandle()
        {
            if (handle != IntPtr.Zero)
                Allocator.Free(handle.ToPointer());
            return true;
        }

        public ReadOnlySpan<byte> ToSpan() => new(handle.ToPointer(), Length);
    }
}
#nullable restore
#if !BUTR_NATIVEAOT_ENABLE_WARNING
#pragma warning restore
#endif