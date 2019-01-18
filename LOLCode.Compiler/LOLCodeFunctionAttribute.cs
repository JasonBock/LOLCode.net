using System;

namespace LOLCode.Compiler
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public sealed class LOLCodeFunctionAttribute : Attribute { }
}
