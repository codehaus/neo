using System.EnterpriseServices;
using System.Runtime.InteropServices;


namespace Neo.Core.Transactions
{
	[ComVisible(true)]
	[JustInTimeActivation(true)]
	[Transaction(TransactionOption.Required)]
	public class TransactionRequiredEnvironment : TransactionEnvironment
	{
		public TransactionRequiredEnvironment()
		{
			logger.Debug("Created 'transaction required' enviroment.");
		}
	}

	[ComVisible(true)]
	[JustInTimeActivation(true)]
	[Transaction(TransactionOption.RequiresNew)]
	public class NewTransactionRequiredEnvironment : TransactionEnvironment
	{
		public NewTransactionRequiredEnvironment()
		{
			logger.Debug("Created 'new transaction required' enviroment.");
		}
	}


	[ComVisible(true)]
	[JustInTimeActivation(true)]
	[Transaction(TransactionOption.Supported)]
	public class SupportsTransactionEnvironment : TransactionEnvironment
	{
		public SupportsTransactionEnvironment()
		{
			logger.Debug("Created 'supported transaction' enviroment.");
		}
	}

	
	[ComVisible(true)]
	[JustInTimeActivation(true)]
	[Transaction(TransactionOption.NotSupported)]
	public class NotSupportedTransactionEnvironment : TransactionEnvironment
	{
		public NotSupportedTransactionEnvironment()
		{
			logger.Debug("Created 'not supported transaction' enviroment.");
		}
	}


	[ComVisible(true)]
	[JustInTimeActivation(true)]
	[Transaction(TransactionOption.Disabled)]
	public class DisabledTransactionEnvironment : TransactionEnvironment
	{
		public DisabledTransactionEnvironment()
		{
			logger.Debug("Created 'disabled transaction' enviroment.");
		}
	}


}

