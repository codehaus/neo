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
			Assert.IsNotNull(rel, "Did not find any.");
			Assert.IsNotNull(rel.VarName == "a" || rel.VarName == "b", "Found wrong one.");
		}


		[Test]
		public void RelationshipsForAttribute()
		{
		    ArrayList	vars;

			vars = new ArrayList();
			foreach(EntityRelationship r in entity.RelationshipsForAttribute(attr1))
				vars.Add(r.VarName);

			Assert.AreEqual(2, vars.Count, "Found wrong number.");
			Assert.IsTrue(vars.Contains("a"), "Did not find a.");
			Assert.IsTrue(vars.Contains("b"), "Did not find b.");
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
			Assert.AreEqual(2, result.Count, "Wrong number of combinations.");
			
			combination = (IList)result[0];
			Assert.AreEqual(1, combination.Count, "Wrong length.");
			Assert.AreEqual("c", ((EntityRelationship)combination[0]).VarName, "Wrong combination 0.");

			combination = (IList)result[1];
			Assert.AreEqual(1, combination.Count, "Wrong length.");
			Assert.AreEqual("d", ((EntityRelationship)combination[0]).VarName, "Wrong combination 0.");
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
			Assert.AreEqual(2, result.Count, "Wrong number of combinations.");
			
			combination = (IList)result[0];
			Assert.AreEqual(2, combination.Count, "Wrong length.");
			Assert.AreEqual("a", ((EntityRelationship)combination[0]).VarName, "Wrong combination 0.");
			Assert.AreEqual("c", ((EntityRelationship)combination[1]).VarName, "Wrong combination 0.");

			combination = (IList)result[1];
			Assert.AreEqual(2, combination.Count, "Wrong length.");
			Assert.AreEqual("a", ((EntityRelationship)combination[0]).VarName, "Wrong combination 0.");
			Assert.AreEqual("d", ((EntityRelationship)combination[1]).VarName, "Wrong combination 0.");
		}


		[Test]
		public void RelationshipSets()
		{
		    EntityAttribute[]	attrList;
			IList[]				result;
			IList				combination;

			attrList = new EntityAttribute[2] { attr1, attr2 };

			result = entity.RelationshipSetsForColumns(attrList);
			Assert.AreEqual(4, result.Length, "Wrong number of combinations.");
			
			combination = result[0];
			Assert.AreEqual("a", ((EntityRelationship)combination[0]).VarName, "Wrong combination 0.");
			Assert.AreEqual("c", ((EntityRelationship)combination[1]).VarName, "Wrong combination 0.");

			combination = result[1];
			Assert.AreEqual("a", ((EntityRelationship)combination[0]).VarName, "Wrong combination 0.");
			Assert.AreEqual("d", ((EntityRelationship)combination[1]).VarName, "Wrong combination 0.");

			combination = result[2];
			Assert.AreEqual("b", ((EntityRelationship)combination[0]).VarName, "Wrong combination 0.");
			Assert.AreEqual("c", ((EntityRelationship)combination[1]).VarName, "Wrong combination 0.");

			combination = result[3];
			Assert.AreEqual("b", ((EntityRelationship)combination[0]).VarName, "Wrong combination 0.");
			Assert.AreEqual("d", ((EntityRelationship)combination[1]).VarName, "Wrong combination 0.");
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
