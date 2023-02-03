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

    public interface IConstRoot { }
    public interface IConstFlags { }
    public class IsConst : IConstRoot { }
    public class IsNotConst : IConstRoot { }
    public class IsPtrConst : IConstRoot, IConstFlags { }
    public class IsConst<T> : IConstRoot where T : IConstFlags { }
    public class IsNotConst<T> : IConstRoot where T : IConstFlags { }

    /// <summary>
    /// Indicator for teh Header generator that the parameter is const
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Method)]
    public class IsConstAttribute : Attribute
    {
        public bool PointsToConstant { get; set; }
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Method)]
    public class IsConstAttribute<T> : Attribute where T : IConstFlags { }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Method)]
    public class IsNotConstAttribute<T> : Attribute where T : IConstFlags { }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Method)]
    public class ConstMetaAttribute<T1> : Attribute
        where T1 : IConstRoot
    { }
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Method)]
    public class ConstMetaAttribute<T1, T2> : Attribute
        where T1 : IConstRoot where T2 : IConstRoot
    { }
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Method)]
    public class ConstMetaAttribute<T1, T2, T3> : Attribute
        where T1 : IConstRoot where T2 : IConstRoot where T3 : IConstRoot
    { }
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Method)]
    public class ConstMetaAttribute<T1, T2, T3, T4> : Attribute
        where T1 : IConstRoot where T2 : IConstRoot where T3 : IConstRoot where T4 : IConstRoot
    { }
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Method)]
    public class ConstMetaAttribute<T1, T2, T3, T4, T5> : Attribute
        where T1 : IConstRoot where T2 : IConstRoot where T3 : IConstRoot where T4 : IConstRoot
    { }
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Method)]
    public class ConstMetaAttribute<T1, T2, T3, T4, T5, T6> : Attribute
        where T1 : IConstRoot where T2 : IConstRoot where T3 : IConstRoot where T4 : IConstRoot
        where T5 : IConstRoot where T6 : IConstRoot
    { }
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Method)]
    public class ConstMetaAttribute<T1, T2, T3, T4, T5, T6, T7> : Attribute
        where T1 : IConstRoot where T2 : IConstRoot where T3 : IConstRoot where T4 : IConstRoot
        where T5 : IConstRoot where T6 : IConstRoot where T7 : IConstRoot
    { }
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Method)]
    public class ConstMetaAttribute<T1, T2, T3, T4, T5, T6, T7, T8> : Attribute
        where T1 : IConstRoot where T2 : IConstRoot where T3 : IConstRoot where T4 : IConstRoot
        where T5 : IConstRoot where T6 : IConstRoot where T7 : IConstRoot where T8 : IConstRoot
    { }
}
#nullable restore
#if !BUTR_NATIVEAOT_ENABLE_WARNING
#pragma warning restore
#endif