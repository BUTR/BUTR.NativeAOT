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
        public static unsafe TSelf* AsException<TSelf>(Exception e, bool isOwner)
            where TSelf : unmanaged, IReturnValueWithError<TSelf> => TSelf.AsError(Copy(e.ToString(), isOwner), isOwner);

        public static SafeStringMallocHandle SerializeJsonCopy<TValue>(TValue value, JsonTypeInfo<TValue> jsonTypeInfo, bool isOwner)
            where TValue: class => Copy(SerializeJson(value, jsonTypeInfo), isOwner);

        public static string SerializeJson<TValue>(TValue? value, JsonTypeInfo<TValue> jsonTypeInfo)
            where TValue: class => value is null ? string.Empty : JsonSerializer.Serialize(value, jsonTypeInfo);

        public static TValue? DeserializeJson<TValue>(SafeStringMallocHandle json, JsonTypeInfo<TValue> jsonTypeInfo, [CallerMemberName] string? caller = null)
            where TValue: class => json.IsInvalid ? null : DeserializeJson(json.ToSpan(), jsonTypeInfo, caller);

        [return: NotNullIfNotNull(nameof(json))]
        public static unsafe TValue? DeserializeJson<TValue>(param_json* json, JsonTypeInfo<TValue> jsonTypeInfo, [CallerMemberName] string? caller = null)
            where TValue: class => json is null ? null : DeserializeJson(param_json.ToSpan(json), jsonTypeInfo, caller);

        private static TValue? DeserializeJson<TValue>([StringSyntax(StringSyntaxAttribute.Json)] ReadOnlySpan<char> json, JsonTypeInfo<TValue> jsonTypeInfo, [CallerMemberName] string? caller = null)
        {
            try
            {
                return JsonSerializer.Deserialize(json, jsonTypeInfo);
            }
            catch (JsonException e)
            {
                throw new JsonDeserializationException($"Failed to deserialize! Caller: {caller}, Type: {typeof(TValue)}; Json: {json};", e);
            }
        }


        public static unsafe SafeDataMallocHandle Copy(in ReadOnlySpan<byte> data, bool isOwner)
        {
            var dst = (byte*) Allocator.Alloc(new UIntPtr((uint) data.Length));
            data.CopyTo(new Span<byte>(dst, data.Length));
            return new(dst, data.Length, isOwner);
        }
        
        public static unsafe SafeStringMallocHandle Copy(in ReadOnlySpan<char> str, bool isOwner)
        {
            var size = (uint) ((str.Length + 1) * 2);
            var dst = (char*) Allocator.Alloc(new UIntPtr(size));
            str.CopyTo(new Span<char>(dst, str.Length));
            dst[str.Length] = '\0';
            return new(dst, isOwner);
        }

        public static unsafe SafeStructMallocHandle<TValue> Create<TValue>(TValue value, bool isOwner) where TValue : unmanaged
        {
            var size = Unsafe.SizeOf<TValue>();
            var dst = (TValue*) Allocator.Alloc(new UIntPtr((uint) size));
            MemoryMarshal.Write(new Span<byte>(dst, size), in value);
            return SafeStructMallocHandle.Create(dst, isOwner);
        }
    }
}
#nullable restore
#if !BUTR_NATIVEAOT_ENABLE_WARNING
#pragma warning restore
#endif