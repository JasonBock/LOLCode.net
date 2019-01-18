using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace LOLCode.Compiler
{
	internal enum LOLCodeVersion
	{
		v1_0,
		v1_1,
		v1_2,
		IRCSPECZ
	}

	internal class LOLProgram
	{
		public LOLCodeVersion version = LOLCodeVersion.v1_2;
		public CompilerParameters compileropts;
		public Scope globals = new Scope();
		public Dictionary<string, LOLMethod> methods = new Dictionary<string, LOLMethod>();
		public List<Assembly> assemblies = new List<Assembly>();

		public LOLProgram(CompilerParameters opts)
		{
			this.compileropts = opts;

			var mainRef = new UserFunctionRef("Main", 0, false)
			{
				ReturnType = typeof(void)
			};
			this.globals.AddSymbol(mainRef);
			this.methods.Add("Main", new LOLMethod(mainRef, this));

			this.assemblies.Add(Assembly.GetAssembly(typeof(Core)));
			this.ImportLibrary($"{typeof(Core).Namespace}.{nameof(Core)}");
		}

		public MethodInfo Emit(CompilerErrorCollection errors, ModuleBuilder mb)
		{
			var cls = mb.DefineType(this.compileropts.MainClass);

			//Define methods
			foreach (var method in this.methods.Values)
			{
				(this.globals[method.info.Name] as UserFunctionRef).Builder = cls.DefineMethod(method.info.Name, MethodAttributes.Public | MethodAttributes.Static, method.info.ReturnType, method.info.ArgumentTypes);
			}

			//Define globals
			foreach (var sr in this.globals)
			{
				if (sr is GlobalRef)
				{
					(sr as GlobalRef).Field = cls.DefineField(sr.Name, (sr as GlobalRef).Type, FieldAttributes.Static | FieldAttributes.Public);
				}
			}

			//Emit methods
			foreach (var method in this.methods.Values)
			{
				method.Emit(errors, (this.globals[method.info.Name] as UserFunctionRef).Builder);
			}

			//Create type
			var t = cls.CreateType();

			//Return main
			return t.GetMethod("Main");
		}

		public static void WrapObject(Type t, ILGenerator gen)
		{
			if (t == typeof(int))
			{
				//Box the int
				gen.Emit(OpCodes.Box, typeof(int));
			}
			else if (t == typeof(Dictionary<object, object>))
			{
				//Clone the array
				gen.Emit(OpCodes.Newobj, typeof(Dictionary<object, object>).GetConstructor(new Type[] { typeof(Dictionary<object, object>) }));
			}
		}

		public bool ImportLibrary(string name)
		{
			Type t = null;
			foreach (var a in this.assemblies)
			{
				t = a.GetType(name);
				if (t != null)
				{
					break;
				}
			}

			if (t == null)
			{
				return false;
			}

			foreach (var mi in t.GetMethods(BindingFlags.Public | BindingFlags.Static))
			{
				var attribs = mi.GetCustomAttributes(typeof(LOLCodeFunctionAttribute), true);
				for (var i = 0; i < attribs.Length; i++)
				{
					var attrib = attribs[i] as LOLCodeFunctionAttribute;
					this.globals.AddSymbol(new ImportFunctionRef(mi, mi.Name));
				}
			}

			return true;
		}
	}

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