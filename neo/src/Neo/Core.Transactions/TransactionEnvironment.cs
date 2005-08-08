using System;
using System.EnterpriseServices;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using log4net;


namespace Neo.Core.Transactions
{
	public abstract class TransactionEnvironment : ServicedComponent
	{
		protected static ILog logger;

		protected TransactionEnvironment()
		{
			if(logger == null)
				logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
		}
	
		/// <summary>
		/// Invokes a method in the target environment.
		/// </summary>
		/// <param name="target">The object instance that owns the message.</param>
		/// <param name="message">The message (method and arguments) to be executed.</param>
		/// <param name="autoComplete">Whether to complete the transaction if invokation was successful.</param>
		/// <returns>Returns the invokation result.</returns>
		public IMethodReturnMessage InvokeMethod(MarshalByRefObject target, IMethodCallMessage message, bool autoComplete)
		{
			IMethodReturnMessage returnMessage = RemotingServices.ExecuteMessage(target, message);

			if(ContextUtil.IsInTransaction)
			{
				if(returnMessage.Exception == null)
				{
					if(autoComplete)
					{
						logger.Debug("Voting Commit. (Operation successful and autocomplete set.)");
						ContextUtil.SetComplete();
					}
				}
				else
				{
					logger.Debug("Voting Abort. Reason: " + returnMessage.Exception.Message);
					ContextUtil.MyTransactionVote = TransactionVote.Abort;
				}
			}

			return returnMessage;
		}

	}
}
