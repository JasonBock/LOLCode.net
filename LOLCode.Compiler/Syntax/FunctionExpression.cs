using LOLCode.Compiler.Emitter;
using LOLCode.Compiler.Symbols;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Syntax
{
	// TODO: make it sealed
	internal class FunctionExpression 
		: Expression
	{
		// TODO: make these readonly
		public FunctionRef func;
		public List<Expression> arguments = new List<Expression>();

		public FunctionExpression(CodePragma loc) 
			: base(loc) { }

		public FunctionExpression(CodePragma loc, FunctionRef fr) 
			: base(loc) => this.func = fr;

		public override void Emit(LOLMethod lm, Type t, ILGenerator gen)
		{
			if (this.func.IsVariadic)
			{
				//First do standard (non variadic) arguments)
				for (var i = 0; i < this.func.Arity; i++)
				{
					this.arguments[i].Emit(lm, this.func.ArgumentTypes[i], gen);
				}

				//Now any variadic arguments go into an array
				var argType = this.func.ArgumentTypes[this.func.Arity].GetElementType();
				gen.Emit(OpCodes.Ldc_I4, this.arguments.Count - this.func.Arity);
				gen.Emit(OpCodes.Newarr, argType);

				for (var i = this.func.Arity; i < this.arguments.Count; i++)
				{
					gen.Emit(OpCodes.Dup);
					gen.Emit(OpCodes.Ldc_I4, i - this.func.Arity);
					this.arguments[i].Emit(lm, argType, gen);
					gen.Emit(OpCodes.Stelem, argType);
				}

				gen.EmitCall(OpCodes.Call, this.func.Method, null);
			}
			else
			{
				for (var i = 0; i < this.arguments.Count; i++)
				{
					this.arguments[i].Emit(lm, this.func.ArgumentTypes[i], gen);
				}

				gen.EmitCall(OpCodes.Call, this.func.Method, null);
			}

			//Finally, make sure the return type is correct
			Expression.EmitCast(gen, this.func.ReturnType, t);
		}

		public override void Process(LOLMethod lm, CompilerErrorCollection errors, ILGenerator gen)
		{
			if (this.arguments.Count != this.func.Arity && !this.func.IsVariadic)
			{
				errors.Add(new CompilerError(this.location.filename, this.location.startLine, this.location.startColumn, null, 
					$"Function \"{this.func.Name}\" requires {this.func.Arity} arguments, passed {this.arguments.Count}."));
			}
			else if (this.arguments.Count < this.func.Arity && this.func.IsVariadic)
			{
				errors.Add(new CompilerError(this.location.filename, this.location.startLine, this.location.startColumn, null, 
					$"Function \"{this.func.Name}\" requires at least {this.func.Arity} arguments, passed {this.arguments.Count}."));
			}

			foreach (var arg in this.arguments)
			{
				arg.Process(lm, errors, gen);
			}
		}

		public override Type EvaluationType => this.func.ReturnType;
	}
}
