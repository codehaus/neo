using System;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;


namespace Neo.Core.Transactions
{
	[AttributeUsage(AttributeTargets.Class, Inherited=true)]
	public class TransactionAwareAttribute : ContextAttribute, IContributeObjectSink
	{
		public TransactionAwareAttribute() : base("NeoTransaction")
		{
		}

		public override bool IsContextOK(Context context, IConstructionCallMessage constructionMesssage)
		{
			foreach(object contextProperty in context.ContextProperties)
			{
				if(contextProperty == this)
					return true;
			}
			return false;
		}

		public override void GetPropertiesForNewContext(IConstructionCallMessage constructionMesssage)
		{
			constructionMesssage.ContextProperties.Add(this);
		}

		public IMessageSink GetObjectSink(MarshalByRefObject target, IMessageSink nextSink)
		{
			return new TransactionSink(target, nextSink);
		}

	}
}