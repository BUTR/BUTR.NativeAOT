namespace BUTR.NativeAOT.Analyzer.Test
{
    public class BaseTest
    {
        protected static readonly string CodeBase = @"
namespace BUTR.NativeAOT.Shared
{
    using System;
    using System.Reflection;

    public interface IConstRoot { }
    public interface IConstFlags { }
    public class IsConst : IConstRoot { }
    public class IsNotConst : IConstRoot { }
    public class IsPtrConst : IConstRoot, IConstFlags { }
    public class IsConst<T> : IConstRoot where T : IConstFlags { }
    public class IsNotConst<T> : IConstRoot where T : IConstFlags { }
    
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
";
    }
}