using System.Collections;
using Neo.MetaModel;
using NUnit.Framework;


namespace Neo.Tools.Tests
{
	[TestFixture]
	public class ModelTests
	{
		private EntityAttribute	 attr1;
		private EntityAttribute	 attr2;
		private EntityRelationship rela;
		private EntityRelationship relb;
		private EntityRelationship relc;
		private EntityRelationship reld;
		private MyEntity		entity;

		[SetUp]
		public void SetUp()
		{
			entity = new MyEntity(new Model());
			entity.ClassName = "Foo";

			attr1 = new EntityAttribute(entity);
			attr1.PropertyName = "attr1";
			attr1.ColumnName = "col1";
			attr2 = new EntityAttribute(entity);
			attr2.PropertyName = "attr2";
			attr2.ColumnName = "col2";

			rela = new EntityRelationship(entity);
			rela.VarName = "a";
			rela.LocalKey = attr1.ColumnName;
			relb = new EntityRelationship(entity);
			relb.VarName = "b";
			relb.LocalKey = attr1.ColumnName;
			relc = new EntityRelationship(entity);
			relc.VarName = "c";
			relc.LocalKey = attr2.ColumnName;
			reld = new EntityRelationship(entity);
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
			EntityRelationship rel;

			rel = entity.RelationshipForAttribute(attr1);
			Assertion.AssertNotNull("Did not find any.", rel);
			Assertion.AssertNotNull("Found wrong one.", rel.VarName == "a" || rel.VarName == "b");
		}


		[Test]
		public void RelationshipsForAttribute()
		{
		    ArrayList	vars;

			vars = new ArrayList();
			foreach(EntityRelationship r in entity.RelationshipsForAttribute(attr1))
				vars.Add(r.VarName);

			Assertion.AssertEquals("Found wrong number.", 2, vars.Count);
			Assertion.Assert("Did not find a.", vars.Contains("a"));
			Assertion.Assert("Did not find b.", vars.Contains("b"));
		}


		[Test]
		public void AddCombinationsWithEmptyFixedPart()
		{
			ArrayList			 result;
			EntityRelationship[] fixedPart;
			EntityRelationship[] combinations;
			IList				 combination;

			result = new ArrayList();
			fixedPart = new EntityRelationship[] {};
			combinations = new EntityRelationship[] { relc, reld }; 
	
			entity.AddCombinations(fixedPart, combinations, result);
			Assertion.AssertEquals("Wrong number of combinations.", 2, result.Count);
			
			combination = (IList)result[0];
			Assertion.AssertEquals("Wrong length.", 1, combination.Count);
			Assertion.AssertEquals("Wrong combination 0.", "c", ((EntityRelationship)combination[0]).VarName);

			combination = (IList)result[1];
			Assertion.AssertEquals("Wrong length.", 1, combination.Count);
			Assertion.AssertEquals("Wrong combination 0.", "d", ((EntityRelationship)combination[0]).VarName);
		}

		
		[Test]
		public void AddCombinations()
		{
			ArrayList			 result;
			EntityRelationship[] fixedPart;
			EntityRelationship[] combinations;
			IList				 combination;

			result = new ArrayList();
			fixedPart = new EntityRelationship[] { rela };
			combinations = new EntityRelationship[] { relc, reld }; 
	
			entity.AddCombinations(fixedPart, combinations, result);
			Assertion.AssertEquals("Wrong number of combinations.", 2, result.Count);
			
			combination = (IList)result[0];
			Assertion.AssertEquals("Wrong length.", 2, combination.Count);
			Assertion.AssertEquals("Wrong combination 0.", "a", ((EntityRelationship)combination[0]).VarName);
			Assertion.AssertEquals("Wrong combination 0.", "c", ((EntityRelationship)combination[1]).VarName);

			combination = (IList)result[1];
			Assertion.AssertEquals("Wrong length.", 2, combination.Count);
			Assertion.AssertEquals("Wrong combination 0.", "a", ((EntityRelationship)combination[0]).VarName);
			Assertion.AssertEquals("Wrong combination 0.", "d", ((EntityRelationship)combination[1]).VarName);
		}


		[Test]
		public void RelationshipSets()
		{
		    EntityAttribute[]	attrList;
			IList[]				result;
			IList				combination;

			attrList = new EntityAttribute[2] { attr1, attr2 };

			result = entity.RelationshipSetsForColumns(attrList);
			Assertion.AssertEquals("Wrong number of combinations.", 4, result.Length);
			
			combination = result[0];
			Assertion.AssertEquals("Wrong combination 0.", "a", ((EntityRelationship)combination[0]).VarName);
			Assertion.AssertEquals("Wrong combination 0.", "c", ((EntityRelationship)combination[1]).VarName);

			combination = result[1];
			Assertion.AssertEquals("Wrong combination 0.", "a", ((EntityRelationship)combination[0]).VarName);
			Assertion.AssertEquals("Wrong combination 0.", "d", ((EntityRelationship)combination[1]).VarName);

			combination = result[2];
			Assertion.AssertEquals("Wrong combination 0.", "b", ((EntityRelationship)combination[0]).VarName);
			Assertion.AssertEquals("Wrong combination 0.", "c", ((EntityRelationship)combination[1]).VarName);

			combination = result[3];
			Assertion.AssertEquals("Wrong combination 0.", "b", ((EntityRelationship)combination[0]).VarName);
			Assertion.AssertEquals("Wrong combination 0.", "d", ((EntityRelationship)combination[1]).VarName);
		}

	}


	#region Helper class for access

	public class MyEntity : Entity
	{
		public MyEntity(Model model) : base(model)
		{
		}

		public new void AddCombinations(IList fixedPart, IList combinations, ArrayList list)
		{
			base.AddCombinations(fixedPart, combinations, list);
		}
	}

	#endregion


}
