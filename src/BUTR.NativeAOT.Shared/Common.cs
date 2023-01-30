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
    using global::System;
    using global::System.Runtime.CompilerServices;
    using global::System.Runtime.InteropServices;
    using global::System.Text.Json.Serialization.Metadata;

    public unsafe interface IParameter<TSelf> where TSelf : unmanaged, IParameter<TSelf>
    {
        static abstract ReadOnlySpan<char> ToSpan(TSelf* ptr);
    }
    
    public unsafe interface IReturnValueWithError<TSelf> where TSelf : unmanaged, IReturnValueWithError<TSelf>
    {
        static abstract TSelf* AsError(char* error);
    }
    public unsafe interface IReturnValueWithErrorWithValue<TSelf, in TValue> : IReturnValueWithError<TSelf>
        where TSelf : unmanaged, IReturnValueWithErrorWithValue<TSelf, TValue>
        where TValue : unmanaged
    {
        static abstract TSelf* AsValue(TValue value);
    }
    public unsafe interface IReturnValueWithErrorWithValuePtr<TSelf, TValue> : IReturnValueWithError<TSelf>
        where TSelf : unmanaged, IReturnValueWithErrorWithValuePtr<TSelf, TValue>
        where TValue : unmanaged
    {
        static abstract TSelf* AsValue(TValue* value);
    }
    public unsafe interface IReturnValueWithErrorWithValuePtr<TSelf> : IReturnValueWithError<TSelf>
        where TSelf : unmanaged, IReturnValueWithErrorWithValuePtr<TSelf>
    {
        static abstract TSelf* AsValue(void* value);
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct param_ptr : IParameter<param_ptr>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> ToSpan(param_ptr* ptr) => new IntPtr(ptr).ToString();
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct param_bool : IParameter<param_bool>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> ToSpan(param_bool* ptr) => ptr->Value.ToString();

        public readonly byte Value;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct param_int : IParameter<param_int>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> ToSpan(param_int* ptr) => ptr->Value.ToString();
        
        public readonly int Value;
    }    
    
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct param_uint : IParameter<param_uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> ToSpan(param_uint* ptr) => ptr->Value.ToString();
        
        public readonly uint Value;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct param_string : IParameter<param_string>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> ToSpan(param_string* ptr) => MemoryMarshal.CreateReadOnlySpanFromNullTerminated((char*) ptr);
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct param_json : IParameter<param_json>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> ToSpan(param_json* ptr) => MemoryMarshal.CreateReadOnlySpanFromNullTerminated((char*) ptr);
    }


    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct return_value_void : IReturnValueWithError<return_value_void>
    {
        public static return_value_void* AsValue() => Utils.Create(new return_value_void(null));
        public static return_value_void* AsError(char* error) => Utils.Create(new return_value_void(error));

        public readonly char* Error;

        private return_value_void(char* error)
        {
            Error = error;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct return_value_string : IReturnValueWithErrorWithValuePtr<return_value_string, char>
    {
        public static return_value_string* AsValue(char* value) => Utils.Create(new return_value_string(value, null));
        public static return_value_string* AsError(char* error) => Utils.Create(new return_value_string(null, error));

        public readonly char* Error;
        public readonly char* Value;

        private return_value_string(char* value, char* error)
        {
            Value = value;
            Error = error;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct return_value_json : IReturnValueWithErrorWithValuePtr<return_value_json, char>
    {
        public static return_value_json* AsValue<TValue>(TValue value, JsonTypeInfo<TValue> jsonTypeInfo) =>
            AsValue(Utils.SerializeJsonCopy<TValue>(value, jsonTypeInfo));
        public static return_value_json* AsValue(char* value) => Utils.Create(new return_value_json(value, null));
        public static return_value_json* AsError(char* error) => Utils.Create(new return_value_json(null, error));

        public readonly char* Error;
        public readonly char* Value;

        private return_value_json(char* value, char* error)
        {
            Value = value;
            Error = error;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct return_value_bool : IReturnValueWithErrorWithValue<return_value_bool, bool>
    {
        public static return_value_bool* AsValue(bool value) => Utils.Create(new return_value_bool(value, null));
        public static return_value_bool* AsError(char* error) => Utils.Create(new return_value_bool(false, error));

        public readonly char* Error;
        public readonly byte Value;

        private return_value_bool(bool value, char* error)
        {
            Value = (byte) (value ? 1 : 0);
            Error = error;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct return_value_int32 : IReturnValueWithErrorWithValue<return_value_int32, int>
    {
        public static return_value_int32* AsValue(int value) => Utils.Create(new return_value_int32(value, null));
        public static return_value_int32* AsError(char* error) => Utils.Create(new return_value_int32(0, null));

        public readonly char* Error;
        public readonly int Value;

        private return_value_int32(int value, char* error)
        {
            Value = value;
            Error = error;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct return_value_uint32 : IReturnValueWithErrorWithValue<return_value_uint32, uint>
    {
        public static return_value_uint32* AsValue(uint value) => Utils.Create(new return_value_uint32(value, null));
        public static return_value_uint32* AsError(char* error) => Utils.Create(new return_value_uint32(0, error));

        public readonly char* Error;
        public readonly uint Value;

        private return_value_uint32(uint value, char* error)
        {
            Value = value;
            Error = error;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct return_value_ptr : IReturnValueWithErrorWithValuePtr<return_value_ptr>
    {
        public static return_value_ptr* AsValue(void* value) => Utils.Create(new return_value_ptr(value, null));
        public static return_value_ptr* AsError(char* error) => Utils.Create(new return_value_ptr(null, error));

        public readonly char* Error;
        public readonly void* Value;

        private return_value_ptr(void* value, char* error)
        {
            Value = value;
            Error = error;
        }
    }
}
#nullable restore
#if !BUTR_NATIVEAOT_ENABLE_WARNING
#pragma warning restore
#endif