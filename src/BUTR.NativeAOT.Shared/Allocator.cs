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
using System.Runtime.InteropServices;
#if !BUTR_NATIVEAOT_ENABLE_WARNING
#pragma warning disable
#endif

namespace BUTR.NativeAOT.Shared
{
    using global::System;
    using global::System.Diagnostics;

    public static unsafe class Allocator
    {
#if TRACK_ALLOCATIONS
        private static readonly ConcurrentCollections.ConcurrentHashSet<IntPtr> _pointers = new();

        public static int GetCurrentAllocations(void* ptr) => _pointers.Count;
        
        [Conditional("TRACK_ALLOCATIONS")]
        private static void TrackAllocation(void* ptr)
        {
            var pointer = new IntPtr(ptr);
            if (!_pointers.Add(pointer)) throw new AllocationException("Allocation: Tracking an already allocated pointer!");
        }
        
        [Conditional("TRACK_ALLOCATIONS")]
        private static void TrackDeallocation(void* ptr)
        {
            var pointer = new IntPtr(ptr);
            if (!_pointers.TryRemove(pointer)) throw new AllocationException("Deallocation: Deallocating an untracked pointer!");
        }
#endif

        private static delegate*<nuint, void*> _alloc = &NativeMemory.Alloc;
        private static delegate*<void*, void> _dealloc = &NativeMemory.Free;

        public static void SetDefault()
        {
            _alloc = &NativeMemory.Alloc;
            _dealloc = &NativeMemory.Free;
        }
        public static void SetCustom(delegate*<nuint, void*> alloc, delegate*<void*, void> dealloc)
        {
            _alloc = alloc;
            _dealloc = dealloc;
        }
        
        public static void* Alloc(nuint byteCount, bool track)
        {
            var ptr = _alloc(byteCount);
            if (track)
                TrackAllocation(ptr);
            return ptr;
        }

        public static void Free(void* ptr, bool track)
        {
            if (track)
                TrackDeallocation(ptr);
            _dealloc(ptr);
        }
    }
}
#nullable restore
#if !BUTR_NATIVEAOT_ENABLE_WARNING
#pragma warning restore
#endif