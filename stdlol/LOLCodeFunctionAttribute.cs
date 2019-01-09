using System;

namespace stdlol
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public sealed class LOLCodeFunctionAttribute : Attribute { }
}
