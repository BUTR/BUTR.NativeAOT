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

    [StructLayout(LayoutKind.Sequential, Size = 0)]
    public struct VoidPtr { }

    public unsafe interface IParameter<TSelf> where TSelf : unmanaged, IParameter<TSelf>
    {

    }
    public unsafe interface IParameterSpanFormattable<TSelf>
        where TSelf : unmanaged, IParameterSpanFormattable<TSelf>
    {
        static abstract ReadOnlySpan<char> ToSpan(TSelf* ptr);
    }
    public unsafe interface IParameterNonPtr<TSelf> where TSelf : unmanaged, IParameterNonPtr<TSelf>
    {
    }
    public unsafe interface IParameterRawPtr<TSelf, TValue>
        where TSelf : unmanaged, IParameterRawPtr<TSelf, TValue>
        where TValue : unmanaged
    {
        static abstract TValue* ToRawPtr(TSelf* ptr);
    }
    public unsafe interface IParameterIntPtr<TSelf>
        where TSelf : unmanaged, IParameterIntPtr<TSelf>
    {
        static abstract IntPtr ToPtr(TSelf* ptr);
    }

    public unsafe interface IReturnValueWithError<TSelf>
        where TSelf : unmanaged, IReturnValueWithError<TSelf>
    {
        static abstract TSelf* AsError(char* error, bool isOwner);
    }
    public unsafe interface IReturnValueWithException<TSelf>
        where TSelf : unmanaged, IReturnValueWithException<TSelf>
    {
        static abstract TSelf* AsException(Exception e, bool isOwner);
    }

    public unsafe interface IReturnValueWithNoValue<TSelf>
        where TSelf : unmanaged, IReturnValueWithNoValue<TSelf>
    {
        static abstract TSelf* AsValue(bool isOwner);
    }
    public unsafe interface IReturnValueWithValue<TSelf, in TValue>
        where TSelf : unmanaged, IReturnValueWithValue<TSelf, TValue>
    {
        static abstract TSelf* AsValue(TValue value, bool isOwner);
    }
    public unsafe interface IReturnValueWithValueJson<TSelf>
        where TSelf : unmanaged, IReturnValueWithValueJson<TSelf>
    {
        static abstract TSelf* AsValue<TValue>(TValue value, JsonTypeInfo<TValue> jsonTypeInfo, bool isOwner);
    }
    public unsafe interface IReturnValueWithValueLength<TSelf, in TValue>
        where TSelf : unmanaged, IReturnValueWithValueLength<TSelf, TValue>
        where TValue : unmanaged
    {
        static abstract TSelf* AsValue(TValue* value, int length, bool isOwner);
    }
    public unsafe interface IReturnValueWithValuePtr<TSelf, TValue>
        where TSelf : unmanaged, IReturnValueWithValuePtr<TSelf, TValue>
        where TValue : unmanaged
    {
        static abstract TSelf* AsValue(TValue* value, bool isOwner);
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct param_ptr : IParameter<param_ptr>, IParameterSpanFormattable<param_ptr>, IParameterRawPtr<param_ptr, VoidPtr>, IParameterIntPtr<param_ptr>
    {
        public static implicit operator param_ptr*(param_ptr value) => &value;
        public static implicit operator void*(param_ptr ptr) => &ptr;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> ToSpan(param_ptr* ptr) => new IntPtr(ptr).ToString();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static IntPtr IParameterIntPtr<param_ptr>.ToPtr(param_ptr* ptr) => new(ptr);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VoidPtr* ToRawPtr(param_ptr* ptr) => (VoidPtr*) ptr;

        public readonly void* Value;

    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct param_bool : IParameter<param_bool>, IParameterSpanFormattable<param_bool>, IParameterNonPtr<param_bool>
    {
        public static implicit operator param_bool*(param_bool value) => &value;
        public static implicit operator param_bool(bool value) => new(value);
        public static implicit operator bool(param_bool ptr) => ptr.Value == 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> ToSpan(param_bool* ptr) => ptr->Value.ToString();

        public readonly byte Value;

        public param_bool() { }
        private param_bool(bool value) => Value = (byte) (value ? 1 : 0);
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct param_int : IParameter<param_int>, IParameterSpanFormattable<param_int>, IParameterNonPtr<param_int>
    {
        public static implicit operator param_int*(param_int value) => &value;
        public static implicit operator param_int(int value) => new(value);
        public static implicit operator int(param_int ptr) => ptr.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> ToSpan(param_int* ptr) => ptr->Value.ToString();

        public readonly int Value;

        public param_int() { }
        private param_int(int value) => Value = value;
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct param_uint : IParameter<param_uint>, IParameterSpanFormattable<param_uint>, IParameterNonPtr<param_uint>
    {
        public static implicit operator param_uint*(param_uint value) => &value;
        public static implicit operator param_uint(uint value) => new(value);
        public static implicit operator uint(param_uint ptr) => ptr.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> ToSpan(param_uint* ptr) => ptr->Value.ToString();

        public readonly uint Value;

        public param_uint() { }
        private param_uint(uint value) => Value = value;
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct param_string : IParameter<param_string>, IParameterSpanFormattable<param_string>, IParameterRawPtr<param_string, char>, IParameterIntPtr<param_string>
    {
        public static implicit operator param_string*(param_string value) => &value;
        public static implicit operator char*(param_string ptr) => (char*) &ptr;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> ToSpan(param_string* ptr) => MemoryMarshal.CreateReadOnlySpanFromNullTerminated((char*) ptr);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static IntPtr IParameterIntPtr<param_string>.ToPtr(param_string* ptr) => new(ptr);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char* ToRawPtr(param_string* ptr) => (char*) ptr;

        public readonly char* Value;
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct param_json : IParameter<param_json>, IParameterSpanFormattable<param_json>, IParameterRawPtr<param_json, char>, IParameterIntPtr<param_json>
    {
        public static implicit operator param_json*(param_json value) => &value;
        public static implicit operator char*(param_json ptr) => (char*) &ptr;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> ToSpan(param_json* ptr) => MemoryMarshal.CreateReadOnlySpanFromNullTerminated((char*) ptr);
        static IntPtr IParameterIntPtr<param_json>.ToPtr(param_json* ptr) => new(ptr);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char* ToRawPtr(param_json* ptr) => (char*) ptr;

        public readonly char* Value;
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct param_data : IParameter<param_data>, IParameterSpanFormattable<param_data>, IParameterRawPtr<param_data, char>, IParameterIntPtr<param_data>
    {
        public static implicit operator param_data*(param_data value) => &value;
        public static implicit operator char*(param_data ptr) => (char*) &ptr;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> ToSpan(param_data* ptr) => MemoryMarshal.CreateReadOnlySpanFromNullTerminated((char*) ptr);
        static IntPtr IParameterIntPtr<param_data>.ToPtr(param_data* ptr) => new(ptr);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char* ToRawPtr(param_data* ptr) => (char*) ptr;

        public readonly byte* Value;
    }


    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct return_value_void : IReturnValueWithNoValue<return_value_void>, IReturnValueWithError<return_value_void>, IReturnValueWithException<return_value_void>
    {
        public static return_value_void* AsValue(bool isOwner) => Utils.Create(new return_value_void(null), isOwner);
        public static return_value_void* AsError(char* error, bool isOwner) => Utils.Create(new return_value_void(error), isOwner);
        public static return_value_void* AsException(Exception e, bool isOwner) => Utils.AsException<return_value_void>(e, isOwner);

        public readonly char* Error;

        private return_value_void(char* error)
        {
            Error = error;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct return_value_string : IReturnValueWithValuePtr<return_value_string, char>, IReturnValueWithValue<return_value_string, string>, IReturnValueWithError<return_value_string>, IReturnValueWithException<return_value_string>
    {
        public static return_value_string* AsValue(string value, bool isOwner) => Utils.Create(new return_value_string(Utils.Copy(value, isOwner), null), isOwner);
        public static return_value_string* AsValue(char* value, bool isOwner) => Utils.Create(new return_value_string(value, null), isOwner);
        public static return_value_string* AsError(char* error, bool isOwner) => Utils.Create(new return_value_string(null, error), isOwner);
        public static return_value_string* AsException(Exception e, bool isOwner) => Utils.AsException<return_value_string>(e, isOwner);

        public readonly char* Error;
        public readonly char* Value;

        private return_value_string(char* value, char* error)
        {
            Value = value;
            Error = error;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct return_value_json : IReturnValueWithValuePtr<return_value_json, char>, IReturnValueWithValueJson<return_value_json>, IReturnValueWithError<return_value_json>, IReturnValueWithException<return_value_json>
    {
        public static return_value_json* AsValue<TValue>(TValue value, JsonTypeInfo<TValue> jsonTypeInfo, bool isOwner) => AsValue(Utils.SerializeJsonCopy(value, jsonTypeInfo, isOwner), isOwner);
        public static return_value_json* AsValue(char* value, bool isOwner) => Utils.Create(new return_value_json(value, null), isOwner);
        public static return_value_json* AsError(char* error, bool isOwner) => Utils.Create(new return_value_json(null, error), isOwner);
        public static return_value_json* AsException(Exception e, bool isOwner) => Utils.AsException<return_value_json>(e, isOwner);

        public readonly char* Error;
        public readonly char* Value;

        private return_value_json(char* value, char* error)
        {
            Value = value;
            Error = error;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct return_value_data : IReturnValueWithValueLength<return_value_data, byte>, IReturnValueWithError<return_value_data>, IReturnValueWithException<return_value_data>
    {
        public static return_value_data* AsValue(byte* value, int length, bool isOwner) => Utils.Create(new return_value_data(value, length, null), isOwner);
        public static return_value_data* AsError(char* error, bool isOwner) => Utils.Create(new return_value_data(null, 0, error), isOwner);
        public static return_value_data* AsException(Exception e, bool isOwner) => Utils.AsException<return_value_data>(e, isOwner);

        public readonly char* Error;
        public readonly byte* Value;
        public readonly int Length;

        private return_value_data(byte* value, int length, char* error)
        {
            Value = value;
            Length = length;
            Error = error;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct return_value_bool : IReturnValueWithValue<return_value_bool, bool>, IReturnValueWithError<return_value_bool>, IReturnValueWithException<return_value_bool>
    {
        public static return_value_bool* AsValue(bool value, bool isOwner) => Utils.Create(new return_value_bool(value, null), isOwner);
        public static return_value_bool* AsError(char* error, bool isOwner) => Utils.Create(new return_value_bool(false, error), isOwner);
        public static return_value_bool* AsException(Exception e, bool isOwner) => Utils.AsException<return_value_bool>(e, isOwner);

        public readonly char* Error;
        public readonly byte Value;

        private return_value_bool(bool value, char* error)
        {
            Value = (byte) (value ? 1 : 0);
            Error = error;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct return_value_int32 : IReturnValueWithValue<return_value_int32, int>, IReturnValueWithError<return_value_int32>, IReturnValueWithException<return_value_int32>
    {
        public static return_value_int32* AsValue(int value, bool isOwner) => Utils.Create(new return_value_int32(value, null), isOwner);
        public static return_value_int32* AsError(char* error, bool isOwner) => Utils.Create(new return_value_int32(0, null), isOwner);
        public static return_value_int32* AsException(Exception e, bool isOwner) => Utils.AsException<return_value_int32>(e, isOwner);

        public readonly char* Error;
        public readonly int Value;

        private return_value_int32(int value, char* error)
        {
            Value = value;
            Error = error;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct return_value_uint32 : IReturnValueWithValue<return_value_uint32, uint>, IReturnValueWithError<return_value_uint32>, IReturnValueWithException<return_value_uint32>
    {
        public static return_value_uint32* AsValue(uint value, bool isOwner) => Utils.Create(new return_value_uint32(value, null), isOwner);
        public static return_value_uint32* AsError(char* error, bool isOwner) => Utils.Create(new return_value_uint32(0, error), isOwner);
        public static return_value_uint32* AsException(Exception e, bool isOwner) => Utils.AsException<return_value_uint32>(e, isOwner);

        public readonly char* Error;
        public readonly uint Value;

        private return_value_uint32(uint value, char* error)
        {
            Value = value;
            Error = error;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct return_value_ptr : IReturnValueWithValuePtr<return_value_ptr, VoidPtr>, IReturnValueWithError<return_value_ptr>, IReturnValueWithException<return_value_ptr>
    {
        public static return_value_ptr* AsValue(VoidPtr* value, bool isOwner) => Utils.Create(new return_value_ptr(value, null), isOwner);
        public static return_value_ptr* AsError(char* error, bool isOwner) => Utils.Create(new return_value_ptr(null, error), isOwner);
        public static return_value_ptr* AsException(Exception e, bool isOwner) => Utils.AsException<return_value_ptr>(e, isOwner);

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