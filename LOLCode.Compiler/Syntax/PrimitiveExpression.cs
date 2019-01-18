using System;
using System.CodeDom.Compiler;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	internal class PrimitiveExpression 
		: Expression
	{
		public object value;

		public override Type EvaluationType => this.value.GetType();

		public override void Emit(LOLMethod lm, Type t, ILGenerator gen)
		{
			if (this.value is int && t == typeof(string))
			{
				this.value = ((int)this.value).ToString();
			}

			if (this.value is float && t == typeof(string))
			{
				this.value = ((float)this.value).ToString();
			}

			if (this.value is string && t == typeof(int))
			{
				this.value = int.Parse((string)this.value);
			}

			if (this.value is string && t == typeof(float))
			{
				this.value = float.Parse((string)this.value);
			}

			if (this.value.GetType() != t && t != typeof(object))
			{
				throw new ArgumentException($"{this.value.GetType().Name} encountered, {t.Name} expected.");
			}

			if (this.value is int)
			{
				gen.Emit(OpCodes.Ldc_I4, (int)this.value);
				if (t == typeof(object))
				{
					gen.Emit(OpCodes.Box, typeof(int));
				}
			}
			else if (this.value is float)
			{
				gen.Emit(OpCodes.Ldc_R4, (float)this.value);
				if (t == typeof(object))
				{
					gen.Emit(OpCodes.Box, typeof(float));
				}
			}
			else if (this.value is string)
			{
				gen.Emit(OpCodes.Ldstr, (string)this.value);
				if (t == typeof(object))
				{
					gen.Emit(OpCodes.Castclass, typeof(object));
				}
			}
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen)
		{
			if (this.value.GetType() != typeof(int) && this.value.GetType() != typeof(string))
			{
				//We throw an exception here because this would indicate an issue with the compiler, not with the code being compiled.
				throw new InvalidOperationException("PrimitiveExpression values must be int or string.");
			}

			return;
		}

		public PrimitiveExpression(CodePragma loc) : base(loc) { }
		public PrimitiveExpression(CodePragma loc, object val) : base(loc) => this.value = val;
	}
}
