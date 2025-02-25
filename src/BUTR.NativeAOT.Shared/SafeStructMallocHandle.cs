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
    using global::System.Runtime.CompilerServices;
    using global::System.Text.Json.Serialization.Metadata;
    using global::System.Threading.Tasks;

    internal unsafe class SafeStructMallocHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public static SafeStructMallocHandle<TStruct> Create<TStruct>(TStruct* ptr, bool isOwner) where TStruct : unmanaged => new(ptr, isOwner);

        public readonly bool IsOwner;

        protected SafeStructMallocHandle() : base(true) { }
        protected SafeStructMallocHandle(IntPtr handle, bool isOwner) : base(isOwner)
        {
            this.handle = handle;
            IsOwner = isOwner;
        }

        protected override bool ReleaseHandle()
        {
            if (handle != IntPtr.Zero)
                Allocator.Free(handle.ToPointer());
            return true;
        }
    }

    internal sealed unsafe class SafeStructMallocHandle<TStruct> : SafeStructMallocHandle where TStruct : unmanaged
    {
        public static implicit operator TStruct*(SafeStructMallocHandle<TStruct> handle) => (TStruct*) handle.handle.ToPointer();

        public TStruct* Value => this;

        public bool IsNull => Value == null;

        public SafeStructMallocHandle() : base() { }
        public SafeStructMallocHandle(TStruct* param, bool isOwner) : base(new IntPtr(param), isOwner) { }

        public void ValueAsVoid()
        {
            if (typeof(TStruct) != typeof(return_value_void))
                throw new Exception();

            var ptr = (return_value_void*) Value;
            if (ptr->Error is null)
            {
                return;
            }

            using var hError = new SafeStringMallocHandle(ptr->Error, IsOwner);
            throw new NativeCallException(new string(hError));
        }

        public SafeStringMallocHandle ValueAsString()
        {
            if (typeof(TStruct) != typeof(return_value_string))
                throw new Exception();

            var ptr = (return_value_string*) Value;
            if (ptr->Error is null)
            {
                return new SafeStringMallocHandle(ptr->Value, IsOwner);
            }

            using var hError = new SafeStringMallocHandle(ptr->Error, IsOwner);
            throw new NativeCallException(new string(hError));
        }

        public TValue? ValueAsJson<TValue>(JsonTypeInfo<TValue?> jsonTypeInfo, [CallerMemberName] string? caller = null) where TValue : class
        {
            if (typeof(TStruct) != typeof(return_value_json))
                throw new Exception();

            var ptr = (return_value_json*) Value;
            if (ptr->Error is null)
            {
                using var json = new SafeStringMallocHandle(ptr->Value, IsOwner);
                return Utils.DeserializeJson(json, jsonTypeInfo, caller);
            }

            using var hError = new SafeStringMallocHandle(ptr->Error, IsOwner);
            throw new NativeCallException(new string(hError));
        }

        public SafeDataMallocHandle ValueAsData()
        {
            if (typeof(TStruct) != typeof(return_value_data))
                throw new Exception();

            var ptr = (return_value_data*) Value;
            if (ptr->Error is null)
            {
                return new SafeDataMallocHandle(ptr->Value, ptr->Length, IsOwner);
            }

            using var hError = new SafeStringMallocHandle(ptr->Error, IsOwner);
            throw new NativeCallException(new string(hError));
        }

        public bool ValueAsBool()
        {
            if (typeof(TStruct) != typeof(return_value_bool))
                throw new Exception();

            var ptr = (return_value_bool*) Value;
            if (ptr->Error is null)
            {
                return ptr->Value == 1;
            }

            using var hError = new SafeStringMallocHandle(ptr->Error, IsOwner);
            throw new NativeCallException(new string(hError));
        }

        public uint ValueAsUInt32()
        {
            if (typeof(TStruct) != typeof(return_value_uint32))
                throw new Exception();

            var ptr = (return_value_uint32*) Value;
            if (ptr->Error is null)
            {
                return ptr->Value;
            }

            using var hError = new SafeStringMallocHandle(ptr->Error, IsOwner);
            throw new NativeCallException(new string(hError));
        }

        public int ValueAsInt32()
        {
            if (typeof(TStruct) != typeof(return_value_int32))
                throw new Exception();

            var ptr = (return_value_int32*) Value;
            if (ptr->Error is null)
            {
                return ptr->Value;
            }

            using var hError = new SafeStringMallocHandle(ptr->Error, IsOwner);
            throw new NativeCallException(new string(hError));
        }

        public void* ValueAsPointer()
        {
            if (typeof(TStruct) != typeof(return_value_ptr))
                throw new Exception();

            var ptr = (return_value_ptr*) Value;
            if (ptr->Error is null)
            {
                return ptr->Value;
            }

            using var hError = new SafeStringMallocHandle(ptr->Error, IsOwner);
            throw new NativeCallException(new string(hError));
        }


        public void SetAsVoid(TaskCompletionSource<object?> tcs)
        {
            if (typeof(TStruct) != typeof(return_value_void))
            {
                tcs.TrySetException(new Exception());
                return;
            }

            var ptr = (return_value_void*) Value;
            if (ptr->Error is null)
            {
                tcs.TrySetResult(null);
                return;
            }

            using var hError = new SafeStringMallocHandle(ptr->Error, IsOwner);
            tcs.TrySetException(new NativeCallException(new string(hError)));
        }

        public void SetAsString(TaskCompletionSource<string?> tcs)
        {
            if (typeof(TStruct) != typeof(return_value_string))
            {
                tcs.TrySetException(new Exception());
                return;
            }

            var ptr = (return_value_string*) Value;
            if (ptr->Error is null)
            {
                if (ptr->Value is null)
                {
                    tcs.TrySetResult(null);
                }
                else
                {
                    using var hValue = new SafeStringMallocHandle(ptr->Value, IsOwner);
                    tcs.TrySetResult(hValue.ToString());
                }
                return;
            }

            using var hError = new SafeStringMallocHandle(ptr->Error, IsOwner);
            tcs.TrySetException(new NativeCallException(new string(hError)));
        }

        public void SetAsJson<TValue>(TaskCompletionSource<TValue?> tcs, JsonTypeInfo<TValue?> jsonTypeInfo, [CallerMemberName] string? caller = null) where TValue : class
        {
            if (typeof(TStruct) != typeof(return_value_json))
            {
                tcs.TrySetException(new Exception());
                return;
            }

            var ptr = (return_value_json*) Value;
            if (ptr->Error is null)
            {
                using var json = new SafeStringMallocHandle(ptr->Value, IsOwner);
                tcs.TrySetResult(Utils.DeserializeJson(json, jsonTypeInfo, caller));
                return;
            }

            using var hError = new SafeStringMallocHandle(ptr->Error, IsOwner);
            tcs.TrySetException(new NativeCallException(new string(hError)));
        }

        public void SetAsData(TaskCompletionSource<byte[]?> tcs)
        {
            if (typeof(TStruct) != typeof(return_value_data))
            {
                tcs.TrySetException(new Exception());
                return;
            }

            var ptr = (return_value_data*) Value;
            if (ptr->Error is null)
            {
                if (ptr->Value is null)
                {
                    tcs.TrySetResult(null);
                }
                else
                {
                    using var hValue = new SafeDataMallocHandle(ptr->Value, ptr->Length, IsOwner);
                    tcs.TrySetResult(hValue.ToSpan().ToArray());
                }
                return;
            }

            using var hError = new SafeStringMallocHandle(ptr->Error, IsOwner);
            tcs.TrySetException(new NativeCallException(new string(hError)));
        }

        public void SetAsBool(TaskCompletionSource<bool> tcs)
        {
            if (typeof(TStruct) != typeof(return_value_bool))
            {
                tcs.TrySetException(new Exception());
                return;
            }

            var ptr = (return_value_bool*) Value;
            if (ptr->Error is null)
            {
                tcs.TrySetResult(ptr->Value == 1);
                return;
            }

            using var hError = new SafeStringMallocHandle(ptr->Error, IsOwner);
            tcs.TrySetException(new NativeCallException(new string(hError)));
        }

        public void SetAsUInt32(TaskCompletionSource<uint> tcs)
        {
            if (typeof(TStruct) != typeof(return_value_uint32))
            {
                tcs.TrySetException(new Exception());
                return;
            }

            var ptr = (return_value_uint32*) Value;
            if (ptr->Error is null)
            {
                tcs.TrySetResult(ptr->Value);
                return;
            }

            using var hError = new SafeStringMallocHandle(ptr->Error, IsOwner);
            tcs.TrySetException(new NativeCallException(new string(hError)));
        }

        public void SetAsInt32(TaskCompletionSource<int> tcs)
        {
            if (typeof(TStruct) != typeof(return_value_int32))
            {
                tcs.TrySetException(new Exception());
                return;
            }

            var ptr = (return_value_int32*) Value;
            if (ptr->Error is null)
            {
                tcs.TrySetResult(ptr->Value);
                return;
            }

            using var hError = new SafeStringMallocHandle(ptr->Error, IsOwner);
            tcs.TrySetException(new NativeCallException(new string(hError)));
        }

        public void SetAsPointer(TaskCompletionSource<IntPtr> tcs)
        {
            if (typeof(TStruct) != typeof(return_value_ptr))
            {
                tcs.TrySetException(new Exception());
                return;
            }

            var ptr = (return_value_ptr*) Value;
            if (ptr->Error is null)
            {
                tcs.TrySetResult(new(ptr->Value));
                return;
            }

            using var hError = new SafeStringMallocHandle(ptr->Error, IsOwner);
            tcs.TrySetException(new NativeCallException(new string(hError)));
        }
    }
}
#nullable restore
#if !BUTR_NATIVEAOT_ENABLE_WARNING
#pragma warning restore
#endif