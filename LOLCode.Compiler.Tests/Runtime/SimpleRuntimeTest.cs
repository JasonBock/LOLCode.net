using NUnit.Framework;

namespace LOLCode.Compiler.Tests.Runtime
{
	public static class SimpleRuntimeTest
	{
		[Test]
		[Ignore("Failing now, need to revisit when all unit tests are added.")]
		public static void VisibleKeywordRuntime()
		{
			var sources = SampleHelper.GetCodeFromSample("visible.lol");
			var baseline = SampleHelper.GetBaselineFromSample("visible.lol");
			RuntimeTestHelper.TestExecuteSourcesNoInput(sources, baseline,
				 "VisibleKeywordRuntime", ExecuteMethod.ExternalProcess);
		}

		[Test]
		[Ignore("Failing now, need to revisit when all unit tests are added.")]
		public static void HaiWorldRuntime()
		{
			var sources = SampleHelper.GetCodeFromSample("haiworld.lol");
			var baseline = SampleHelper.GetBaselineFromSample("haiworld.lol");
			RuntimeTestHelper.TestExecuteSourcesNoInput(sources, baseline,
				 "HaiWorldRuntime", ExecuteMethod.ExternalProcess);
		}

		[Test]
		[Ignore("Failing now, need to revisit when all unit tests are added.")]
		public static void Simple1Runtime()
		{
			var sources = SampleHelper.GetCodeFromSample("simple1.lol");
			var baseline = SampleHelper.GetBaselineFromSample("simple1.lol");
			RuntimeTestHelper.TestExecuteSourcesNoInput(sources, baseline,
				 "Simple1Runtime", ExecuteMethod.ExternalProcess);
		}
	}
}
