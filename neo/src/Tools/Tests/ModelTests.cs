using System;
using System.IO;
using System.Collections;
using NUnit.Framework;
using Neo.Model;
using Neo.Model.Impl;


namespace Neo.Tools.Tests
{
	[TestFixture]
	public class ModelTests
	{
		private AttributeImpl	 attr1;
		private AttributeImpl	 attr2;
		private RelationshipImpl rela;
		private RelationshipImpl relb;
		private RelationshipImpl relc;
		private RelationshipImpl reld;
		private MyEntity		entity;

		[SetUp]
		public void SetUp()
		{
			entity = new MyEntity(new ModelImpl());
			entity.ClassName = "Foo";

			attr1 = new AttributeImpl(entity);
			attr1.PropertyName = "attr1";
			attr1.ColumnName = "col1";
			attr2 = new AttributeImpl(entity);
			attr2.PropertyName = "attr2";
			attr2.ColumnName = "col2";

			rela = new RelationshipImpl(entity);
			rela.VarName = "a";
			rela.LocalKey = attr1.ColumnName;
			relb = new RelationshipImpl(entity);
			relb.VarName = "b";
			relb.LocalKey = attr1.ColumnName;
			relc = new RelationshipImpl(entity);
			relc.VarName = "c";
			relc.LocalKey = attr2.ColumnName;
			reld = new RelationshipImpl(entity);
			reld.VarName = "d";
			reld.LocalKey = attr2.ColumnName;

			entity.AddRelationship(rela);
			entity.AddRelationship(relb);
			entity.AddRelationship(relc);
			entity.AddRelationship(reld);
		}
		

		[Test]
		public void RelationshipForAttribute()
		{
			IRelationship rel;

			rel = entity.RelationshipForAttribute(attr1);
			Assertion.AssertNotNull("Did not find any.", rel);
			Assertion.AssertNotNull("Found wrong one.", rel.VarName == "a" || rel.VarName == "b");
		}


		[Test]
		public void RelationshipsForAttribute()
		{
			ArrayList	vars;

			vars = new ArrayList();
			foreach(IRelationship r in entity.RelationshipsForAttribute(attr1))
				vars.Add(r.VarName);

			Assertion.AssertEquals("Found wrong number.", 2, vars.Count);
			Assertion.Assert("Did not find a.", vars.Contains("a"));
			Assertion.Assert("Did not find b.", vars.Contains("b"));
		}


		[Test]
		public void AddCombinationsWithEmptyFixedPart()
		{
			ArrayList		result;
			IRelationship[] fixedPart;
			IRelationship[]	combinations;
			IList			combination;

			result = new ArrayList();
			fixedPart = new IRelationship[] {};
			combinations = new IRelationship[] { relc, reld }; 
	
			entity.AddCombinations(fixedPart, combinations, result);
			Assertion.AssertEquals("Wrong number of combinations.", 2, result.Count);
			
			combination = (IList)result[0];
			Assertion.AssertEquals("Wrong length.", 1, combination.Count);
			Assertion.AssertEquals("Wrong combination 0.", "c", ((IRelationship)combination[0]).VarName);

			combination = (IList)result[1];
			Assertion.AssertEquals("Wrong length.", 1, combination.Count);
			Assertion.AssertEquals("Wrong combination 0.", "d", ((IRelationship)combination[0]).VarName);
		}

		
		[Test]
		public void AddCombinations()
		{
			ArrayList		result;
			IRelationship[]	fixedPart;
			IRelationship[]	combinations;
			IList			combination;

			result = new ArrayList();
			fixedPart = new IRelationship[] { rela };
			combinations = new IRelationship[] { relc, reld }; 
	
			entity.AddCombinations(fixedPart, combinations, result);
			Assertion.AssertEquals("Wrong number of combinations.", 2, result.Count);
			
			combination = (IList)result[0];
			Assertion.AssertEquals("Wrong length.", 2, combination.Count);
			Assertion.AssertEquals("Wrong combination 0.", "a", ((IRelationship)combination[0]).VarName);
			Assertion.AssertEquals("Wrong combination 0.", "c", ((IRelationship)combination[1]).VarName);

			combination = (IList)result[1];
			Assertion.AssertEquals("Wrong length.", 2, combination.Count);
			Assertion.AssertEquals("Wrong combination 0.", "a", ((IRelationship)combination[0]).VarName);
			Assertion.AssertEquals("Wrong combination 0.", "d", ((IRelationship)combination[1]).VarName);
		}


		[Test]
		public void RelationshipSets()
		{
			IAttribute[]	attrList;
			IList[]			result;
			IList			combination;

			attrList = new IAttribute[2] { attr1, attr2 };

			result = entity.RelationshipSetsForColumns(attrList);
			Assertion.AssertEquals("Wrong number of combinations.", 4, result.Length);
			
			combination = (IList)result[0];
			Assertion.AssertEquals("Wrong combination 0.", "a", ((IRelationship)combination[0]).VarName);
			Assertion.AssertEquals("Wrong combination 0.", "c", ((IRelationship)combination[1]).VarName);

			combination = (IList)result[1];
			Assertion.AssertEquals("Wrong combination 0.", "a", ((IRelationship)combination[0]).VarName);
			Assertion.AssertEquals("Wrong combination 0.", "d", ((IRelationship)combination[1]).VarName);

			combination = (IList)result[2];
			Assertion.AssertEquals("Wrong combination 0.", "b", ((IRelationship)combination[0]).VarName);
			Assertion.AssertEquals("Wrong combination 0.", "c", ((IRelationship)combination[1]).VarName);

			combination = (IList)result[3];
			Assertion.AssertEquals("Wrong combination 0.", "b", ((IRelationship)combination[0]).VarName);
			Assertion.AssertEquals("Wrong combination 0.", "d", ((IRelationship)combination[1]).VarName);
		}

	}


	#region Helper class for access

	public class MyEntity : EntityImpl
	{
		public MyEntity(ModelImpl model) : base(model)
		{
		}

		public new void AddCombinations(IList fixedPart, IList combinations, ArrayList list)
		{
			base.AddCombinations(fixedPart, combinations, list);
		}
	}

	#endregion


}
