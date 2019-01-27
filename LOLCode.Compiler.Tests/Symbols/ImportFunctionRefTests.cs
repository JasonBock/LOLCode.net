using LOLCode.Compiler.Symbols;
using NUnit.Framework;
using System;

namespace LOLCode.Compiler.Tests.Symbols
{
	public static class ImportFunctionRefTests
	{
		public static int NoParameters() => 0;
		public static int ParametersWithNoParams(string a, int b, Guid c) => 0;
		public static int ParametersWithParams(string a, int b, params Guid[] c) => 0;

		[Test]
		public static void CreateWithNoParameters()
		{
			const string methodName = nameof(ImportFunctionRefTests.NoParameters);
			var method = typeof(ImportFunctionRefTests).GetMethod(methodName);

			var importFunctionRef = new ImportFunctionRef(method, methodName);

			Assert.That(importFunctionRef.Name, Is.EqualTo(methodName), nameof(SymbolRef.Name));
			Assert.That(importFunctionRef.Arity, Is.EqualTo(0), nameof(FunctionRef.Arity));
			Assert.That(importFunctionRef.IsVariadic, Is.EqualTo(false), nameof(FunctionRef.IsVariadic));
			Assert.That(importFunctionRef.Method, Is.SameAs(method), nameof(ImportFunctionRef.Method));
			Assert.That(importFunctionRef.ReturnType, Is.EqualTo(typeof(int)), nameof(FunctionRef.ReturnType));
			Assert.That(importFunctionRef.ArgumentTypes.Length, Is.EqualTo(0), nameof(FunctionRef.ArgumentTypes));
		}

		[Test]
		public static void CreateWithParametersWithNoParams()
		{
			const string methodName = nameof(ImportFunctionRefTests.ParametersWithNoParams);
			var method = typeof(ImportFunctionRefTests).GetMethod(methodName);

			var importFunctionRef = new ImportFunctionRef(method, methodName);

			Assert.That(importFunctionRef.Name, Is.EqualTo(methodName), nameof(SymbolRef.Name));
			Assert.That(importFunctionRef.Arity, Is.EqualTo(3), nameof(FunctionRef.Arity));
			Assert.That(importFunctionRef.IsVariadic, Is.EqualTo(false), nameof(FunctionRef.IsVariadic));
			Assert.That(importFunctionRef.Method, Is.SameAs(method), nameof(ImportFunctionRef.Method));
			Assert.That(importFunctionRef.ReturnType, Is.EqualTo(typeof(int)), nameof(FunctionRef.ReturnType));
			Assert.That(importFunctionRef.ArgumentTypes.Length, Is.EqualTo(3), nameof(FunctionRef.ArgumentTypes));
			Assert.That(importFunctionRef.ArgumentTypes[0], Is.EqualTo(typeof(string)), $"{nameof(FunctionRef.ArgumentTypes)}[0]");
			Assert.That(importFunctionRef.ArgumentTypes[1], Is.EqualTo(typeof(int)), $"{nameof(FunctionRef.ArgumentTypes)}[1]");
			Assert.That(importFunctionRef.ArgumentTypes[2], Is.EqualTo(typeof(Guid)), $"{nameof(FunctionRef.ArgumentTypes)}[2]");
		}

		[Test]
		public static void CreateWithParametersWithParams()
		{
			const string methodName = nameof(ImportFunctionRefTests.ParametersWithParams);
			var method = typeof(ImportFunctionRefTests).GetMethod(methodName);

			var importFunctionRef = new ImportFunctionRef(method, methodName);

			Assert.That(importFunctionRef.Name, Is.EqualTo(methodName), nameof(SymbolRef.Name));
			Assert.That(importFunctionRef.Arity, Is.EqualTo(2), nameof(FunctionRef.Arity));
			Assert.That(importFunctionRef.IsVariadic, Is.EqualTo(true), nameof(FunctionRef.IsVariadic));
			Assert.That(importFunctionRef.Method, Is.SameAs(method), nameof(ImportFunctionRef.Method));
			Assert.That(importFunctionRef.ReturnType, Is.EqualTo(typeof(int)), nameof(FunctionRef.ReturnType));
			Assert.That(importFunctionRef.ArgumentTypes.Length, Is.EqualTo(3), nameof(FunctionRef.ArgumentTypes));
			Assert.That(importFunctionRef.ArgumentTypes[0], Is.EqualTo(typeof(string)), $"{nameof(FunctionRef.ArgumentTypes)}[0]");
			Assert.That(importFunctionRef.ArgumentTypes[1], Is.EqualTo(typeof(int)), $"{nameof(FunctionRef.ArgumentTypes)}[1]");
			Assert.That(importFunctionRef.ArgumentTypes[2], Is.EqualTo(typeof(Guid[])), $"{nameof(FunctionRef.ArgumentTypes)}[2]");
		}
	}
}
