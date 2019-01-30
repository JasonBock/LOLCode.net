using LOLCode.Compiler.Syntax;
using NUnit.Framework;
using System;

namespace LOLCode.Compiler.Tests.Syntax
{
	public static class ExpressionTests
	{
		[Test]
		public static void EmitCaseWhenTypesAreTheSame()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			Expression.EmitCast(ilGenerator, typeof(int), typeof(int));
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(0), nameof(ilGenerator.ILOffset));
		}

		[Test]
		public static void EmitCaseWhenToIsObjectAndFromIsValueType()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			Expression.EmitCast(ilGenerator, typeof(int), typeof(object));
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(5), nameof(ilGenerator.ILOffset));
		}

		[Test]
		public static void EmitCaseWhenToIsObjectAndFromIsReferenceType()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			Expression.EmitCast(ilGenerator, typeof(string), typeof(object));
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(0), nameof(ilGenerator.ILOffset));
		}

		[Test]
		public static void EmitCaseWhenFromIsObjectAndToIsInt()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			Expression.EmitCast(ilGenerator, typeof(object), typeof(int));
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(5), nameof(ilGenerator.ILOffset));
		}

		[Test]
		public static void EmitCaseWhenFromIsObjectAndToIsFloat()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			Expression.EmitCast(ilGenerator, typeof(object), typeof(float));
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(5), nameof(ilGenerator.ILOffset));
		}

		[Test]
		public static void EmitCaseWhenFromIsObjectAndToIsString()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			Expression.EmitCast(ilGenerator, typeof(object), typeof(string));
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(5), nameof(ilGenerator.ILOffset));
		}

		[Test]
		public static void EmitCaseWhenFromIsObjectAndToIsBool()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			Expression.EmitCast(ilGenerator, typeof(object), typeof(bool));
			Assert.That(ilGenerator.ILOffset, Is.EqualTo(5), nameof(ilGenerator.ILOffset));
		}

		[Test]
		public static void EmitCaseWhenFromIsObjectAndToIsNotSupported()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			Assert.That(() => Expression.EmitCast(ilGenerator, typeof(object), typeof(Guid)),
				Throws.TypeOf<InvalidOperationException>().And.Message.EqualTo("Unknown cast: From Object to Guid"));
		}

		[Test]
		public static void EmitCaseWhenTypesAreNotTheSameAndNeitherAreObjectTypes()
		{
			var (pragma, method, ilGenerator) = SyntaxEmitGenerators.Create();
			Assert.That(() => Expression.EmitCast(ilGenerator, typeof(string), typeof(Guid)),
				Throws.TypeOf<InvalidOperationException>().And.Message.EqualTo("Unknown cast: From String to Guid"));
		}
	}
}
