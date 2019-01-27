using LOLCode.Compiler.Emitter;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	internal abstract class Expression 
		: Statement
	{
		public abstract Type EvaluationType { get; }

		public abstract void Emit(LOLMethod lm, Type t, ILGenerator gen);

		public override void Emit(LOLMethod lm, ILGenerator gen) => this.Emit(lm, typeof(object), gen);

		// TODO: This should be protected
		public Expression(CodePragma loc) : base(loc) { }

		public static void EmitCast(ILGenerator gen, Type from, Type to)
		{
			if (from == to)
			{
				return;
			}
			else if (to == typeof(object))
			{
				if (from.IsValueType)
				{
					gen.Emit(OpCodes.Box, from);
				}
			}
			else if (from == typeof(object))
			{
				if (to == typeof(int))
				{
					gen.EmitCall(OpCodes.Call, typeof(Utils).GetMethod(nameof(Utils.ToInt), BindingFlags.Public | BindingFlags.Static), null);
				}
				else if (to == typeof(float))
				{
					gen.EmitCall(OpCodes.Call, typeof(Utils).GetMethod(nameof(Utils.ToFloat), BindingFlags.Public | BindingFlags.Static), null);
				}
				else if (to == typeof(string))
				{
					gen.EmitCall(OpCodes.Call, typeof(Utils).GetMethod(nameof(Utils.ToString), BindingFlags.Public | BindingFlags.Static), null);
				}
				else if (to == typeof(bool))
				{
					gen.EmitCall(OpCodes.Call, typeof(Utils).GetMethod(nameof(Utils.ToBool), BindingFlags.Public | BindingFlags.Static), null);
				}
				else
				{
					throw new InvalidOperationException($"Unknown cast: From {from.Name} to {to.Name}");
				}
			}
			else
			{
				throw new InvalidOperationException($"Unknown cast: From {from.Name} to {to.Name}");
			}
		}
	}
}
