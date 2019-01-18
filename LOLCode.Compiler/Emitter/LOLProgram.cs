using LOLCode.Compiler.Symbols;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace LOLCode.Compiler.Emitter
{
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
				(this.globals[method.info.Name] as UserFunctionRef).Builder =
					cls.DefineMethod(method.info.Name, MethodAttributes.Public | MethodAttributes.Static, method.info.ReturnType, method.info.ArgumentTypes);
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
}
