using LOLCode.Compiler.Symbols;
using NUnit.Framework;

namespace LOLCode.Compiler.Tests.Symbols
{
	public static class UserFunctionRefTests
	{
		[Test]
		public static void Create()
		{
			var userFunctionRef = new UserFunctionRef("name", 3, true);
			Assert.That(userFunctionRef.Name, Is.EqualTo("name"), nameof(UserFunctionRef.Name));
			Assert.That(userFunctionRef.Arity, Is.EqualTo(3), nameof(UserFunctionRef.Arity));
			Assert.That(userFunctionRef.IsVariadic, Is.True, nameof(UserFunctionRef.IsVariadic));
			Assert.That(userFunctionRef.Builder, Is.Null, nameof(UserFunctionRef.Builder));
			Assert.That(userFunctionRef.ReturnType, Is.EqualTo(typeof(object)), nameof(UserFunctionRef.ReturnType));
			Assert.That(userFunctionRef.ArgumentTypes.Length, Is.EqualTo(3), nameof(UserFunctionRef.ArgumentTypes));
			Assert.That(userFunctionRef.ArgumentTypes[0], Is.EqualTo(typeof(object)), $"{nameof(UserFunctionRef.ArgumentTypes)}[0]");
			Assert.That(userFunctionRef.ArgumentTypes[1], Is.EqualTo(typeof(object)), $"{nameof(UserFunctionRef.ArgumentTypes)}[1]");
			Assert.That(userFunctionRef.ArgumentTypes[2], Is.EqualTo(typeof(object)), $"{nameof(UserFunctionRef.ArgumentTypes)}[2]");
		}
	}
}
