using System;
using System.EnterpriseServices;


namespace Neo.Core.Transactions
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple=false, Inherited=true)]
	public class RequiresTransactionAttribute : Attribute
	{
		private TransactionOption option;

		public RequiresTransactionAttribute(TransactionOption option)
		{
			this.option = option;
		}

		public TransactionOption Option
		{
			get { return option; }
		}
	}
}
