using System;
using System.EnterpriseServices;
using System.Reflection;
using System.Runtime.InteropServices;
using Neo.Core.Transactions;
using Neo.Tests.Fixtures;
using NUnit.Framework;


namespace Neo.Tests.Fixtures
{
	[TestFixture]
	public class TransactionLayerTests : TestBase
	{
		[SetUp]
		public void SetUp()
		{
			SetupLog4Net();
		}

		[Test]
		[ExpectedException(typeof(COMException))]
		public void NonDecoratedMethodIsNotRunInTransactionContext()
		{
			new TestComponent().NonTransactionalMethod();
		}

		[Test]
		public void TransactionRequiredMethodIsRunInTransactionContext()
		{
			TestComponent component = new TestComponent();
			
			component.DoSomethingTransactional();

			Assert.IsTrue(component.wasInTransaction);
		}

		[Test]
		public void TransactionPropagatesToInnerComponentForTransactionRequired()
		{
			TestComponent transaction = new TestComponent();

			transaction.DoSomethingInNestedTransaction("DoSomethingTransactional");
			
			Assert.AreEqual(transaction.transactionId, transaction.innerTransactionId);
		}

		[Test]
		public void NewTransactionForInnerComponentForNewTransactionRequired()
		{
			TestComponent transaction = new TestComponent();

			transaction.DoSomethingInNestedTransaction("DoSomethingTransactionalWithNewTransaction");
			
			Assert.IsTrue(transaction.transactionId != transaction.innerTransactionId);
		}

		[Test]
		public void NoTransactionContextCreatedForInnerComponentThatDoesNotSupportTransactions()
		{
			TestComponent transaction = new TestComponent();

			transaction.DoSomethingInNestedTransaction("DoSomethingThatDoesNotSupportTransactions");
			
			Assert.IsFalse(transaction.wasInTransaction);
		}

		[Test]
		public void InnerComponentDoesNotSupportTransaction2()
		{
			TestComponent transaction = new TestComponent();

			transaction.DoSomethingInNestedTransaction("DoSomethingThatDisablesTransactions");
			
			// Not sure what else to test here.
			Assert.IsFalse(transaction.wasInTransaction);
		}


	}

	#region Helper class

	[TransactionAware]
	public class TestComponent : ContextBoundObject
	{
		public bool wasInTransaction;
		public Guid transactionId;
		public Guid innerTransactionId;

		public void NonTransactionalMethod()
		{
			// this will raise an exception, nothing gets printed
			Console.WriteLine(ContextUtil.ActivityId);
		}

		[RequiresTransaction(TransactionOption.Required)]
		public void DoSomethingTransactional()
		{
			wasInTransaction = ContextUtil.IsInTransaction;
			transactionId = ContextUtil.TransactionId;
		}

		[RequiresTransaction(TransactionOption.RequiresNew)]
		public void DoSomethingTransactionalWithNewTransaction()
		{
			wasInTransaction = ContextUtil.IsInTransaction;
			transactionId = ContextUtil.TransactionId;
		}

		[RequiresTransaction(TransactionOption.NotSupported)]
		public void DoSomethingThatDoesNotSupportTransactions()
		{
			wasInTransaction = ContextUtil.IsInTransaction;
		}

		[RequiresTransaction(TransactionOption.Disabled)]
		public void DoSomethingThatDisablesTransactions()
		{
			wasInTransaction = ContextUtil.IsInTransaction;
		}

		[RequiresTransaction(TransactionOption.Required)]
		public void DoSomethingInNestedTransaction(string methodName)
		{
			transactionId = ContextUtil.TransactionId;
			TestComponent inner = new TestComponent();
			inner.GetType().InvokeMember(methodName, BindingFlags.InvokeMethod, null, inner, null);
			innerTransactionId = inner.transactionId;
		}

	}

	#endregion


}
