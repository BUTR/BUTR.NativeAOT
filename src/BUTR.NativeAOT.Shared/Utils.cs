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
    using global::System.Diagnostics.CodeAnalysis;
    using global::System.Runtime.CompilerServices;
    using global::System.Runtime.InteropServices;
    using global::System.Text.Json;
    using global::System.Text.Json.Serialization.Metadata;

    internal static class Utils
    {
        public static unsafe char* SerializeJsonCopy<TValue>(TValue value, JsonTypeInfo<TValue> jsonTypeInfo) => Copy(JsonSerializer.Serialize(value, jsonTypeInfo));

        public static string SerializeJson<TValue>(TValue value, JsonTypeInfo<TValue> jsonTypeInfo) => JsonSerializer.Serialize(value, jsonTypeInfo);


        public static TValue DeserializeJson<TValue>(SafeStringMallocHandle json, JsonTypeInfo<TValue> jsonTypeInfo, [CallerMemberName] string? caller = null)
        {
            if (json.DangerousGetHandle() == IntPtr.Zero)
            {
                throw new JsonDeserializationException($"Received null parameter! Caller: {caller}, Type: {typeof(TValue)};");
            }

            return DeserializeJson((ReadOnlySpan<char>) json, jsonTypeInfo, caller);
        }

        public static unsafe TValue DeserializeJson<TValue>(param_json* json, JsonTypeInfo<TValue> jsonTypeInfo, [CallerMemberName] string? caller = null)
        {
            if (json is null)
            {
                throw new JsonDeserializationException($"Received null parameter! Caller: {caller}, Type: {typeof(TValue)};");
            }

            return DeserializeJson(param_json.ToSpan(json), jsonTypeInfo, caller);
        }

        private static TValue DeserializeJson<TValue>([StringSyntax(StringSyntaxAttribute.Json)] ReadOnlySpan<char> json, JsonTypeInfo<TValue> jsonTypeInfo, [CallerMemberName] string? caller = null)
        {
            try
            {
                if (JsonSerializer.Deserialize(json, jsonTypeInfo) is not { } result)
                {
                    throw new JsonDeserializationException($"Received null! Caller: {caller}, Type: {typeof(TValue)}; Json:{json};");
                }

                return result;
            }
            catch (JsonException e)
            {
                throw new JsonDeserializationException($"Failed to deserialize! Caller: {caller}, Type: {typeof(TValue)}; Json:{json};", e);
            }
        }


        public static unsafe char* Copy(in ReadOnlySpan<char> str)
        {
            var size = (uint) ((str.Length + 1) * 2);

            var dst = (char*) NativeMemory.Alloc(new UIntPtr(size));
            str.CopyTo(new Span<char>(dst, str.Length));
            dst[str.Length] = '\0';
            return dst;
        }

        public static unsafe TValue* Create<TValue>(TValue value) where TValue : unmanaged
        {
            var size = Unsafe.SizeOf<TValue>();
            var dst = (TValue*) NativeMemory.Alloc(new UIntPtr((uint) size));
            MemoryMarshal.Write(new Span<byte>(dst, size), ref value);
            return dst;
        }
    }
}
#nullable restore
#if !BUTR_NATIVEAOT_ENABLE_WARNING
#pragma warning restore
#endif