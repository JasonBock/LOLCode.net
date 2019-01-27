using LOLCode.Compiler.Symbols;
using LOLCode.Compiler.Syntax;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Emitter
{
	internal class LOLMethod
	{
		public FunctionRef info;
		public ArgumentRef[] args;
		public LOLProgram program;
		public Statement statements;
		public List<BreakableStatement> breakables = new List<BreakableStatement>();
		public Scope locals;
		private readonly Dictionary<Type, Stack<LocalBuilder>> tempLocals = new Dictionary<Type, Stack<LocalBuilder>>();

		public LOLMethod(FunctionRef info, LOLProgram prog)
		{
			this.info = info;
			this.args = new ArgumentRef[info.Arity + (info.IsVariadic ? 1 : 0)];
			this.program = prog;
			this.locals = new Scope(prog.globals);

			var it = new LocalRef("IT");
			this.locals.AddSymbol(it);
		}

		public void DefineLocal(ILGenerator gen, LocalRef l)
		{
			l.Local = gen.DeclareLocal(l.Type);
			if (this.program.compileropts.IncludeDebugInformation)
			{
				l.Local.SetLocalSymInfo(l.Name);
			}
		}

		public LocalBuilder GetTempLocal(ILGenerator gen, Type t)
		{
			if (this.tempLocals.TryGetValue(t, out var temps) && temps.Count > 0)
			{
				return temps.Pop();
			}
			else
			{
				return gen.DeclareLocal(t);
			}
		}

		public void ReleaseTempLocal(LocalBuilder lb)
		{
			if (!this.tempLocals.TryGetValue(lb.LocalType, out var temps))
			{
				temps = new Stack<LocalBuilder>();
				this.tempLocals.Add(lb.LocalType, temps);
			}

			temps.Push(lb);
		}

		public void Emit(CompilerErrorCollection errors, MethodBuilder m)
		{
			//Set the parameters
			for (var i = 0; i < this.args.Length; i++)
			{
				m.DefineParameter(i + 1, ParameterAttributes.None, this.args[i].Name);
			}

			var gen = m.GetILGenerator();

			//Define the IT variable
			var it = this.locals["IT"] as LocalRef;
			this.DefineLocal(gen, it);

			this.statements.Process(this, errors, gen);

			this.statements.Emit(this, gen);

			//Cast the IT variable to our return type and return it
			if (m.ReturnType != typeof(void))
			{
				gen.Emit(OpCodes.Ldloc, it.Local);
				Expression.EmitCast(gen, it.Type, m.ReturnType);
			}
			gen.Emit(OpCodes.Ret);
		}

		public void SetArgumentName(short num, string name)
		{
			this.args[num] = new ArgumentRef(name, num);
			this.locals.AddSymbol(this.args[num]);
		}
	}
}