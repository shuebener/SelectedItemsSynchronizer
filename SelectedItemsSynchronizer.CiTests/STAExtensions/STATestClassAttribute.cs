﻿
namespace SelectedItemsSynchronizer.CiTests.STAExtensions
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	public class STATestClassAttribute : TestClassAttribute
	{
		public override TestMethodAttribute GetTestMethodAttribute(TestMethodAttribute testMethodAttribute)
		{
			if (testMethodAttribute is STATestMethodAttribute)
				return testMethodAttribute;

			return new STATestMethodAttribute(base.GetTestMethodAttribute(testMethodAttribute));
		}
	}
}
