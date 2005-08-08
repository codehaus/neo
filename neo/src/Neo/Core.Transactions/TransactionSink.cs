using System;
using System.EnterpriseServices;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;


namespace Neo.Core.Transactions
{
	/// <summary>
	/// Provides the implementation for a transaction sink that uses the Remoting infrastructure.
	/// </summary>
	/// <remarks>
	/// The function of the transaction sink is to participate in the sink chain, intercepting and hosting transactional
	/// methods in a transactional enviroment using the Enterprise Services infrastructure and delegate it's execution.
	/// </remarks>
	public class TransactionSink : IMessageSink
	{
		private MarshalByRefObject target;
		private IMessageSink nextSink;

		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionSink"/> class.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="nextSink">The next sink in the channel sink chain.</param>
		public TransactionSink(MarshalByRefObject target, IMessageSink nextSink)
		{
			this.target = target;
			this.nextSink = nextSink;
		}


		/// <summary>
		/// Gets the next message sink in the sink chain.
		/// </summary>
		public IMessageSink NextSink
		{
			get { return nextSink; }
		}


		/// <summary>
		/// Synchronously processes the provided message.
		/// </summary>
		/// <param name="message">The message to process.</param>
		/// <returns>The response to the processed message.</returns>
		public IMessage SyncProcessMessage(IMessage message)
		{
			IMethodCallMessage methodMessage = message as IMethodCallMessage;
			if(methodMessage != null)
			{
				MethodBase method = RemotingServices.GetMethodBaseFromMethodMessage(methodMessage);
				RequiresTransactionAttribute requiresTransaction = (RequiresTransactionAttribute)Attribute.GetCustomAttribute(methodMessage.MethodBase, typeof(RequiresTransactionAttribute));

				if((method.IsSpecialName == false) && (requiresTransaction != null))
				{
					AutoCompleteAttribute autoComplete = (AutoCompleteAttribute)Attribute.GetCustomAttribute(methodMessage.MethodBase, typeof(AutoCompleteAttribute));
					using(TransactionEnvironment environment = CreateEnvironment(requiresTransaction))
					{
						return environment.InvokeMethod(target, methodMessage, autoComplete != null);
					}

				}
			}
			return nextSink.SyncProcessMessage(message);
		}


		private TransactionEnvironment CreateEnvironment(RequiresTransactionAttribute requiresTransaction)
		{
			switch(requiresTransaction.Option)
			{
				case TransactionOption.Required:	 return new TransactionRequiredEnvironment();
				case TransactionOption.RequiresNew:	 return new NewTransactionRequiredEnvironment();
				case TransactionOption.Supported:	 return new SupportsTransactionEnvironment();
				case TransactionOption.NotSupported: return new NotSupportedTransactionEnvironment();
				case TransactionOption.Disabled:	 return new DisabledTransactionEnvironment();
				default:							 return null;
			}
		}


		/// <summary>
		/// Asynchronously processes the provided message
		/// </summary>
		/// <param name="message">The message to process.</param>
		/// <param name="replySink">The sink that will receive the reply to the provided message.</param>
		/// <returns>An <see cref="IMessageCtrl"/> that provides a way to control the asynchronous message after it has been dispatched.</returns>
		/// <remarks>NOT SUPPORTED</remarks>
		public IMessageCtrl AsyncProcessMessage(IMessage message, IMessageSink replySink)
		{
			throw new NotSupportedException("AsyncProcessMessage not supported.");
		}


	}
}
