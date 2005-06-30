/* This is a customised template.
 * It overrides some optional methods from EntityMap to allow for testing 
 * with Mocks and fast object creation. Search for #custom# below. 
 */


namespace Pubs4.Model
{
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using Neo.Core;
using Neo.Core.Util;
using Neo.Framework;

//-------- Base Class ----------------------------------------------------

public class TitleBase : EntityObject
{
	public readonly TitleAuthorRelation TitleAuthors;
       
	protected TitleBase(System.Data.DataRow aRow, Neo.Core.ObjectContext aContext) : base(aRow, aContext)
	{
		TitleAuthors = new TitleAuthorRelation(this, "TitleAuthors");
	}
	
	public virtual System.String TitleId
	{
		get { return Row["title_id"] as System.String; }
		set { Row["title_id"] = (value != null) ? (object)value : (object)DBNull.Value; }
	}    

	public virtual System.String TheTitle
	{
		get { return Row["title"] as System.String; }
		set { Row["title"] = (value != null) ? (object)value : (object)DBNull.Value; }
	}    

	public virtual System.String Type
	{
		get { return Row["type"] as System.String; }
		set { Row["type"] = (value != null) ? (object)value : (object)DBNull.Value; }
	}    

	public virtual System.Decimal Price
	{
		get { object v = Row["price"]; return (System.Decimal)((v != DBNull.Value) ? v : HandleNullValueForProperty("Price")); }
		set { Row["price"] = value; }
	}    

	public virtual System.Decimal Advance
	{
		get { object v = Row["advance"]; return (System.Decimal)((v != DBNull.Value) ? v : HandleNullValueForProperty("Advance")); }
		set { Row["advance"] = value; }
	}    

	public virtual System.Int32 Royalty
	{
		get { object v = Row["royalty"]; return (System.Int32)((v != DBNull.Value) ? v : HandleNullValueForProperty("Royalty")); }
		set { Row["royalty"] = value; }
	}    

	public virtual System.Int32 YtdSales
	{
		get { object v = Row["ytd_sales"]; return (System.Int32)((v != DBNull.Value) ? v : HandleNullValueForProperty("YtdSales")); }
		set { Row["ytd_sales"] = value; }
	}    

	public virtual System.String Notes
	{
		get { return Row["notes"] as System.String; }
		set { Row["notes"] = (value != null) ? (object)value : (object)DBNull.Value; }
	}    

	public virtual System.DateTime PublicationDate
	{
		get { object v = Row["pubdate"]; return (System.DateTime)((v != DBNull.Value) ? v : HandleNullValueForProperty("PublicationDate")); }
		set { Row["pubdate"] = value; }
	}    


	public virtual Publisher Publisher
	{
		get { object fk = Row["pub_id"]; return (fk == DBNull.Value) ? null : (Publisher)GetRelatedObject("publishers", fk); }
		set { SetRelatedObject(value, "pub_id", "pub_id" ); }
	}


	public override object GetProperty(string propName)
	{
		if(propName == "TitleId") 
			return TitleId;
		if(propName == "TheTitle") 
			return TheTitle;
		if(propName == "Type") 
			return Type;
		if(propName == "Price") 
			return Price;
		if(propName == "Advance") 
			return Advance;
		if(propName == "Royalty") 
			return Royalty;
		if(propName == "YtdSales") 
			return YtdSales;
		if(propName == "Notes") 
			return Notes;
		if(propName == "PublicationDate") 
			return PublicationDate;
		if(propName == "Publisher") 
			return Publisher;

		return base.GetProperty(propName);
	}

}


//-------- Query Template ------------------------------------------------

public class TitleTemplate : IFetchSpecification
{
	private IEntityMap entityMap;
	private ListDictionary queryValues;
	private int fetchLimit;
	private PropertyComparer[] sortOrderings;
	
	public TitleTemplate(IEntityMap anEntityMap)
	{
		entityMap = anEntityMap;
		queryValues = new ListDictionary();
		fetchLimit = -1;
	}

	public IEntityMap EntityMap 
	{ 
		get { return entityMap; } 
	}

	public Qualifier Qualifier
	{
		get { return Qualifier.FromPropertyDictionary(queryValues); }
	}
	
	public Int32 FetchLimit
	{
		get { return fetchLimit; }
		set { fetchLimit = value; }
	}
	
	public PropertyComparer[] SortOrderings
	{
		get { return sortOrderings; }
		set { sortOrderings = value; }
	}
	
	public System.String TitleId
	{
		get { return (System.String)queryValues["TitleId"]; }
		set { queryValues["TitleId"] = value; }
	}

	public System.String TheTitle
	{
		get { return (System.String)queryValues["TheTitle"]; }
		set { queryValues["TheTitle"] = value; }
	}

	public System.String Type
	{
		get { return (System.String)queryValues["Type"]; }
		set { queryValues["Type"] = value; }
	}

	public System.Decimal Price
	{
		get { return (System.Decimal)queryValues["Price"]; }
		set { queryValues["Price"] = value; }
	}

	public System.Decimal Advance
	{
		get { return (System.Decimal)queryValues["Advance"]; }
		set { queryValues["Advance"] = value; }
	}

	public System.Int32 Royalty
	{
		get { return (System.Int32)queryValues["Royalty"]; }
		set { queryValues["Royalty"] = value; }
	}

	public System.Int32 YtdSales
	{
		get { return (System.Int32)queryValues["YtdSales"]; }
		set { queryValues["YtdSales"] = value; }
	}

	public System.String Notes
	{
		get { return (System.String)queryValues["Notes"]; }
		set { queryValues["Notes"] = value; }
	}

	public System.DateTime PublicationDate
	{
		get { return (System.DateTime)queryValues["PublicationDate"]; }
		set { queryValues["PublicationDate"] = value; }
	}

	public Publisher Publisher
	{
		get { return (Publisher)queryValues["Publisher"]; }
		set { queryValues["Publisher"] = value; }
	}

               
}


//-------- Typed Collections ----------------------------------------------

public class TitleList : ObjectListBase
{
	public TitleList()
	{
	}

	public TitleList(IList list)
	{
		((ArrayList)InnerList).AddRange(list);
	}

	public Title this[int index]
	{
		get { return (Title)InnerList[index]; }
	}

	public int Add(Title newTitle)
	{
		return base.Add(newTitle);
	}

	public void Remove(Title existingTitle)
	{
		base.Remove(existingTitle);
	}

	public bool Contains(Title existingTitle)
	{
		return base.Contains(existingTitle);
	}

	public int IndexOf(Title existingTitle)
	{
		return base.IndexOf(existingTitle);
	}
	
	public TitleList Find(string qualifierFormat, params object[] parameters)
	{
		TitleList resultSet = new TitleList();
		base.Find(resultSet, qualifierFormat, parameters);
		return resultSet;
	}

	public new Title FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (Title)base.FindUnique(qualifierFormat, parameters);
	}
	
	public new Title FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (Title)base.FindFirst(qualifierFormat, parameters);
	}
	
}


public class TitleRelation : ObjectRelationBase
{
	public TitleRelation(IEntityObject eo, string aRelation) : base(eo, aRelation)
	{
	}

	public Title this[int index]
	{
		get { return (Title)InnerList[index]; }
	}

	public int Add(Title newTitle)
	{
		return base.Add(newTitle);
	}

	public void Remove(Title existingTitle)
	{
		base.Remove(existingTitle);
	}

	public bool Contains(Title existingTitle)
	{
		return base.Contains(existingTitle);
	}

	public int IndexOf(Title existingTitle)
	{
		return base.IndexOf(existingTitle);
	}

	public TitleList GetReadOnlyList()
	{
		TitleList resultSet = new TitleList();
		base.CopyToListAndMakeReadOnly(resultSet);
		return resultSet;
	}
	
	public TitleList GetSortedList(string propName, SortDirection dir)
	{
		TitleList resultSet = new TitleList();
		base.CopyToListAndSort(resultSet, propName, dir);
		return resultSet;
	}

	public TitleList Find(string qualifierFormat, params object[] parameters)
	{
		TitleList resultSet = new TitleList();
		base.Find(resultSet, qualifierFormat, parameters);
		return resultSet;
	}

	public new Title FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (Title)base.FindUnique(qualifierFormat, parameters);
	}
	
	public new Title FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (Title)base.FindFirst(qualifierFormat, parameters);
	}

}


//-------- Object Factory ------------------------------------------------

public class TitleFactory : Neo.Framework.ObjectFactory
{
	public TitleFactory(ObjectContext context) : base(context, typeof(Title))
	{
	}

	public Title CreateObject(System.String arg0)
	{
		return (Title)base.CreateObject(new object[] { arg0 } );
	}
	
	public Title FindObject(System.String arg0)
	{
		return (Title)base.FindObject(new object[] { arg0 } );
	}

	public new TitleList FindAllObjects()
	{
		TitleList c = new TitleList();
		foreach(Title eo in base.FindAllObjects())
			c.Add(eo);
		return c;
	}
	
	public TitleTemplate GetQueryTemplate()
	{
		return new TitleTemplate(EntityMap);
	}
	
	public TitleList Find(TitleTemplate template)
	{
		TitleList c = new TitleList();
		foreach(Title eo in base.Find(template))
			c.Add(eo);
		return c;
	}

	public TitleList Find(FetchSpecification fetchSpecification)
	{
		TitleList c = new TitleList();
		foreach(Title eo in base.Find(fetchSpecification))
			c.Add(eo);
		return c;
	}
	
	public new TitleList Find(string qualifierFormat, params object[] parameters)
	{
		TitleList c = new TitleList();
		foreach(Title eo in base.Find(qualifierFormat, parameters))
			c.Add(eo);
		return c;
	}

	public new TitleList FindWithLimit(int limit, string qualifierFormat, params object[] parameters)
	{
		TitleList c = new TitleList();
		foreach(Title eo in base.Find(qualifierFormat, parameters))
			c.Add(eo);
		return c;
	}
	
	public new Title FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (Title)base.FindFirst(qualifierFormat, parameters);
	}

	public new Title FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (Title)base.FindUnique(qualifierFormat, parameters);
	}	

	
}


//-------- Entity Map -----------------------------------------------------

internal class TitleMap : EntityMap
{
    private static readonly string[] pkcolumns = { "title_id" };
    private static readonly string[] columns = { "title_id", "title", "type", "pub_id", "price", "advance", "royalty", "ytd_sales", "notes", "pubdate" };
    private static readonly string[] attributes = { "TitleId", "TheTitle", "Type", "PubId", "Price", "Advance", "Royalty", "YtdSales", "Notes", "PublicationDate" };
    private static readonly string[] relations = { "Publisher", "TitleAuthors" };
    
    private Type concreteObjectType = typeof(Title);				/* #custom# added */

    public override System.Type ObjectType
    {
        get { return typeof(Title); }
    }
    
    public override System.Type ConcreteObjectType
    {
        get { return concreteObjectType; }										 /* #custom# overridden */
        set { concreteObjectType = value; Factory.AddCustomType(value, this); }  /* #custom# overridden */
    }
    
    public override string TableName
    {
        get { return "titles"; }
    }
    
    public override string[] PrimaryKeyColumns
    {
        get { return pkcolumns; }
    }

    public override string[] Columns
    {
        get { return columns; }
    }

    public override string[] Attributes
    {
        get { return attributes; }
    }

    public override string[] Relations
    {
        get { return relations; }
    }

  	protected override IDictionary GetRelationInfos()
    {
    	IDictionary infos = new Hashtable(2);
  		infos.Add("Publisher", new RelationInfo(Factory, typeof(Publisher), typeof(Title), "pub_id", "pub_id"));
 		infos.Add("TitleAuthors", new RelationInfo(Factory, typeof(Title), typeof(TitleAuthor), "title_id", "title_id"));
		return infos;    	
    }
    
	public override IPkInitializer GetPkInitializer()
	{
		return new UserPkInitializer();
	}
	
	public override IEntityObject CreateInstance(DataRow row, ObjectContext context) /* #custom# overridden */
	{
		if(ConcreteObjectType != ObjectType)
			return base.CreateInstance(row, context);
		return new Title(row, context);
	}
    
     protected override void WriteBasicSchema(DataTable table)
    {
		DataColumn		c;
		
		c = table.Columns.Add("title_id", typeof(System.String));
		c.Unique = true;
		c = table.Columns.Add("title", typeof(System.String));
		c = table.Columns.Add("type", typeof(System.String));
		c = table.Columns.Add("pub_id", typeof(System.String));
		c.AllowDBNull = true;
		c = table.Columns.Add("price", typeof(System.Decimal));
		c.AllowDBNull = true;
		c = table.Columns.Add("advance", typeof(System.Decimal));
		c.AllowDBNull = true;
		c = table.Columns.Add("royalty", typeof(System.Int32));
		c.AllowDBNull = true;
		c = table.Columns.Add("ytd_sales", typeof(System.Int32));
		c.AllowDBNull = true;
		c = table.Columns.Add("notes", typeof(System.String));
		c.AllowDBNull = true;
		c = table.Columns.Add("pubdate", typeof(System.DateTime));
		table.PrimaryKey = new DataColumn[] { table.Columns["title_id"] };
	}
	
	protected override void WriteRelations(DataTable table)
	{
		DataRelation r;
		
		if(table.DataSet.Relations["publishers*titles.pub_id"] == null)
		{
			r = table.DataSet.Relations.Add("publishers*titles.pub_id", 
					table.DataSet.Tables["publishers"].Columns["pub_id"],
					table.DataSet.Tables["titles"].Columns["pub_id"]);
			r.ChildKeyConstraint.UpdateRule = Rule.Cascade;
			r.ChildKeyConstraint.DeleteRule = Rule.SetNull;
		}
 		if(table.DataSet.Relations["titles*titleauthor.title_id"] == null)
		{
			r = table.DataSet.Relations.Add("titles*titleauthor.title_id", 
					table.DataSet.Tables["titles"].Columns["title_id"],
					table.DataSet.Tables["titleauthor"].Columns["title_id"]);
			r.ChildKeyConstraint.UpdateRule = Rule.Cascade;
			r.ChildKeyConstraint.DeleteRule = Rule.Cascade;
		}
	}
}

}

/* This is a customised template.
 * It overrides some optional methods from EntityMap to allow for testing 
 * with Mocks and fast object creation. Search for #custom# below. 
 */


namespace Pubs4.Model
{
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using Neo.Core;
using Neo.Core.Util;
using Neo.Framework;

//-------- Base Class ----------------------------------------------------

public class PublisherBase : EntityObject
{
	public readonly TitleRelation Titles;
       
	protected PublisherBase(System.Data.DataRow aRow, Neo.Core.ObjectContext aContext) : base(aRow, aContext)
	{
		Titles = new TitleRelation(this, "Titles");
	}
	
	public virtual System.String PubId
	{
		get { return Row["pub_id"] as System.String; }
		set { Row["pub_id"] = (value != null) ? (object)value : (object)DBNull.Value; }
	}    

	public virtual System.String Name
	{
		get { return Row["pub_name"] as System.String; }
		set { Row["pub_name"] = (value != null) ? (object)value : (object)DBNull.Value; }
	}    

	public virtual System.String City
	{
		get { return Row["city"] as System.String; }
		set { Row["city"] = (value != null) ? (object)value : (object)DBNull.Value; }
	}    

	public virtual System.String State
	{
		get { return Row["state"] as System.String; }
		set { Row["state"] = (value != null) ? (object)value : (object)DBNull.Value; }
	}    

	public virtual System.String Country
	{
		get { return Row["country"] as System.String; }
		set { Row["country"] = (value != null) ? (object)value : (object)DBNull.Value; }
	}    



	public override object GetProperty(string propName)
	{
		if(propName == "PubId") 
			return PubId;
		if(propName == "Name") 
			return Name;
		if(propName == "City") 
			return City;
		if(propName == "State") 
			return State;
		if(propName == "Country") 
			return Country;

		return base.GetProperty(propName);
	}

}


//-------- Query Template ------------------------------------------------

public class PublisherTemplate : IFetchSpecification
{
	private IEntityMap entityMap;
	private ListDictionary queryValues;
	private int fetchLimit;
	private PropertyComparer[] sortOrderings;
	
	public PublisherTemplate(IEntityMap anEntityMap)
	{
		entityMap = anEntityMap;
		queryValues = new ListDictionary();
		fetchLimit = -1;
	}

	public IEntityMap EntityMap 
	{ 
		get { return entityMap; } 
	}

	public Qualifier Qualifier
	{
		get { return Qualifier.FromPropertyDictionary(queryValues); }
	}
	
	public Int32 FetchLimit
	{
		get { return fetchLimit; }
		set { fetchLimit = value; }
	}
	
	public PropertyComparer[] SortOrderings
	{
		get { return sortOrderings; }
		set { sortOrderings = value; }
	}
	
	public System.String PubId
	{
		get { return (System.String)queryValues["PubId"]; }
		set { queryValues["PubId"] = value; }
	}

	public System.String Name
	{
		get { return (System.String)queryValues["Name"]; }
		set { queryValues["Name"] = value; }
	}

	public System.String City
	{
		get { return (System.String)queryValues["City"]; }
		set { queryValues["City"] = value; }
	}

	public System.String State
	{
		get { return (System.String)queryValues["State"]; }
		set { queryValues["State"] = value; }
	}

	public System.String Country
	{
		get { return (System.String)queryValues["Country"]; }
		set { queryValues["Country"] = value; }
	}

               
}


//-------- Typed Collections ----------------------------------------------

public class PublisherList : ObjectListBase
{
	public PublisherList()
	{
	}

	public PublisherList(IList list)
	{
		((ArrayList)InnerList).AddRange(list);
	}

	public Publisher this[int index]
	{
		get { return (Publisher)InnerList[index]; }
	}

	public int Add(Publisher newPublisher)
	{
		return base.Add(newPublisher);
	}

	public void Remove(Publisher existingPublisher)
	{
		base.Remove(existingPublisher);
	}

	public bool Contains(Publisher existingPublisher)
	{
		return base.Contains(existingPublisher);
	}

	public int IndexOf(Publisher existingPublisher)
	{
		return base.IndexOf(existingPublisher);
	}
	
	public PublisherList Find(string qualifierFormat, params object[] parameters)
	{
		PublisherList resultSet = new PublisherList();
		base.Find(resultSet, qualifierFormat, parameters);
		return resultSet;
	}

	public new Publisher FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (Publisher)base.FindUnique(qualifierFormat, parameters);
	}
	
	public new Publisher FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (Publisher)base.FindFirst(qualifierFormat, parameters);
	}
	
}


public class PublisherRelation : ObjectRelationBase
{
	public PublisherRelation(IEntityObject eo, string aRelation) : base(eo, aRelation)
	{
	}

	public Publisher this[int index]
	{
		get { return (Publisher)InnerList[index]; }
	}

	public int Add(Publisher newPublisher)
	{
		return base.Add(newPublisher);
	}

	public void Remove(Publisher existingPublisher)
	{
		base.Remove(existingPublisher);
	}

	public bool Contains(Publisher existingPublisher)
	{
		return base.Contains(existingPublisher);
	}

	public int IndexOf(Publisher existingPublisher)
	{
		return base.IndexOf(existingPublisher);
	}

	public PublisherList GetReadOnlyList()
	{
		PublisherList resultSet = new PublisherList();
		base.CopyToListAndMakeReadOnly(resultSet);
		return resultSet;
	}
	
	public PublisherList GetSortedList(string propName, SortDirection dir)
	{
		PublisherList resultSet = new PublisherList();
		base.CopyToListAndSort(resultSet, propName, dir);
		return resultSet;
	}

	public PublisherList Find(string qualifierFormat, params object[] parameters)
	{
		PublisherList resultSet = new PublisherList();
		base.Find(resultSet, qualifierFormat, parameters);
		return resultSet;
	}

	public new Publisher FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (Publisher)base.FindUnique(qualifierFormat, parameters);
	}
	
	public new Publisher FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (Publisher)base.FindFirst(qualifierFormat, parameters);
	}

}


//-------- Object Factory ------------------------------------------------

public class PublisherFactory : Neo.Framework.ObjectFactory
{
	public PublisherFactory(ObjectContext context) : base(context, typeof(Publisher))
	{
	}

	public Publisher CreateObject(System.String arg0)
	{
		return (Publisher)base.CreateObject(new object[] { arg0 } );
	}
	
	public Publisher FindObject(System.String arg0)
	{
		return (Publisher)base.FindObject(new object[] { arg0 } );
	}

	public new PublisherList FindAllObjects()
	{
		PublisherList c = new PublisherList();
		foreach(Publisher eo in base.FindAllObjects())
			c.Add(eo);
		return c;
	}
	
	public PublisherTemplate GetQueryTemplate()
	{
		return new PublisherTemplate(EntityMap);
	}
	
	public PublisherList Find(PublisherTemplate template)
	{
		PublisherList c = new PublisherList();
		foreach(Publisher eo in base.Find(template))
			c.Add(eo);
		return c;
	}

	public PublisherList Find(FetchSpecification fetchSpecification)
	{
		PublisherList c = new PublisherList();
		foreach(Publisher eo in base.Find(fetchSpecification))
			c.Add(eo);
		return c;
	}
	
	public new PublisherList Find(string qualifierFormat, params object[] parameters)
	{
		PublisherList c = new PublisherList();
		foreach(Publisher eo in base.Find(qualifierFormat, parameters))
			c.Add(eo);
		return c;
	}

	public new PublisherList FindWithLimit(int limit, string qualifierFormat, params object[] parameters)
	{
		PublisherList c = new PublisherList();
		foreach(Publisher eo in base.Find(qualifierFormat, parameters))
			c.Add(eo);
		return c;
	}
	
	public new Publisher FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (Publisher)base.FindFirst(qualifierFormat, parameters);
	}

	public new Publisher FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (Publisher)base.FindUnique(qualifierFormat, parameters);
	}	

	
}


//-------- Entity Map -----------------------------------------------------

internal class PublisherMap : EntityMap
{
    private static readonly string[] pkcolumns = { "pub_id" };
    private static readonly string[] columns = { "pub_id", "pub_name", "city", "state", "country" };
    private static readonly string[] attributes = { "PubId", "Name", "City", "State", "Country" };
    private static readonly string[] relations = { "Titles" };
    
    private Type concreteObjectType = typeof(Publisher);				/* #custom# added */

    public override System.Type ObjectType
    {
        get { return typeof(Publisher); }
    }
    
    public override System.Type ConcreteObjectType
    {
        get { return concreteObjectType; }										 /* #custom# overridden */
        set { concreteObjectType = value; Factory.AddCustomType(value, this); }  /* #custom# overridden */
    }
    
    public override string TableName
    {
        get { return "publishers"; }
    }
    
    public override string[] PrimaryKeyColumns
    {
        get { return pkcolumns; }
    }

    public override string[] Columns
    {
        get { return columns; }
    }

    public override string[] Attributes
    {
        get { return attributes; }
    }

    public override string[] Relations
    {
        get { return relations; }
    }

  	protected override IDictionary GetRelationInfos()
    {
    	IDictionary infos = new Hashtable(1);
  		infos.Add("Titles", new RelationInfo(Factory, typeof(Publisher), typeof(Title), "pub_id", "pub_id"));
		return infos;    	
    }
    
	public override IPkInitializer GetPkInitializer()
	{
		return new UserPkInitializer();
	}
	
	public override IEntityObject CreateInstance(DataRow row, ObjectContext context) /* #custom# overridden */
	{
		if(ConcreteObjectType != ObjectType)
			return base.CreateInstance(row, context);
		return new Publisher(row, context);
	}
    
     protected override void WriteBasicSchema(DataTable table)
    {
		DataColumn		c;
		
		c = table.Columns.Add("pub_id", typeof(System.String));
		c.Unique = true;
		c = table.Columns.Add("pub_name", typeof(System.String));
		c.AllowDBNull = true;
		c = table.Columns.Add("city", typeof(System.String));
		c.AllowDBNull = true;
		c = table.Columns.Add("state", typeof(System.String));
		c.AllowDBNull = true;
		c = table.Columns.Add("country", typeof(System.String));
		c.AllowDBNull = true;
		table.PrimaryKey = new DataColumn[] { table.Columns["pub_id"] };
	}
	
	protected override void WriteRelations(DataTable table)
	{
		DataRelation r;
		
 		if(table.DataSet.Relations["publishers*titles.pub_id"] == null)
		{
			r = table.DataSet.Relations.Add("publishers*titles.pub_id", 
					table.DataSet.Tables["publishers"].Columns["pub_id"],
					table.DataSet.Tables["titles"].Columns["pub_id"]);
			r.ChildKeyConstraint.UpdateRule = Rule.Cascade;
			r.ChildKeyConstraint.DeleteRule = Rule.SetNull;
		}
	}
}

}

/* This is a customised template.
 * It overrides some optional methods from EntityMap to allow for testing 
 * with Mocks and fast object creation. Search for #custom# below. 
 */


namespace Pubs4.Model
{
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using Neo.Core;
using Neo.Core.Util;
using Neo.Framework;

//-------- Base Class ----------------------------------------------------

public class AuthorBase : EntityObject
{
	public readonly TitleAuthorRelation TitleAuthors;
       
	protected AuthorBase(System.Data.DataRow aRow, Neo.Core.ObjectContext aContext) : base(aRow, aContext)
	{
		TitleAuthors = new TitleAuthorRelation(this, "TitleAuthors");
	}
	
	public virtual System.String LastName
	{
		get { return Row["au_lname"] as System.String; }
		set { Row["au_lname"] = (value != null) ? (object)value : (object)DBNull.Value; }
	}    

	public virtual System.String FirstName
	{
		get { return Row["au_fname"] as System.String; }
		set { Row["au_fname"] = (value != null) ? (object)value : (object)DBNull.Value; }
	}    

	public virtual System.String Phone
	{
		get { return Row["phone"] as System.String; }
		set { Row["phone"] = (value != null) ? (object)value : (object)DBNull.Value; }
	}    

	public virtual System.Boolean ContractSigned
	{
		get { object v = Row["contract"]; return (System.Boolean)((v != DBNull.Value) ? v : HandleNullValueForProperty("ContractSigned")); }
		set { Row["contract"] = value; }
	}    



	public override object GetProperty(string propName)
	{
		if(propName == "LastName") 
			return LastName;
		if(propName == "FirstName") 
			return FirstName;
		if(propName == "Phone") 
			return Phone;
		if(propName == "ContractSigned") 
			return ContractSigned;

		return base.GetProperty(propName);
	}

}


//-------- Query Template ------------------------------------------------

public class AuthorTemplate : IFetchSpecification
{
	private IEntityMap entityMap;
	private ListDictionary queryValues;
	private int fetchLimit;
	private PropertyComparer[] sortOrderings;
	
	public AuthorTemplate(IEntityMap anEntityMap)
	{
		entityMap = anEntityMap;
		queryValues = new ListDictionary();
		fetchLimit = -1;
	}

	public IEntityMap EntityMap 
	{ 
		get { return entityMap; } 
	}

	public Qualifier Qualifier
	{
		get { return Qualifier.FromPropertyDictionary(queryValues); }
	}
	
	public Int32 FetchLimit
	{
		get { return fetchLimit; }
		set { fetchLimit = value; }
	}
	
	public PropertyComparer[] SortOrderings
	{
		get { return sortOrderings; }
		set { sortOrderings = value; }
	}
	
	public System.String LastName
	{
		get { return (System.String)queryValues["LastName"]; }
		set { queryValues["LastName"] = value; }
	}

	public System.String FirstName
	{
		get { return (System.String)queryValues["FirstName"]; }
		set { queryValues["FirstName"] = value; }
	}

	public System.String Phone
	{
		get { return (System.String)queryValues["Phone"]; }
		set { queryValues["Phone"] = value; }
	}

	public System.Boolean ContractSigned
	{
		get { return (System.Boolean)queryValues["ContractSigned"]; }
		set { queryValues["ContractSigned"] = value; }
	}

               
}


//-------- Typed Collections ----------------------------------------------

public class AuthorList : ObjectListBase
{
	public AuthorList()
	{
	}

	public AuthorList(IList list)
	{
		((ArrayList)InnerList).AddRange(list);
	}

	public Author this[int index]
	{
		get { return (Author)InnerList[index]; }
	}

	public int Add(Author newAuthor)
	{
		return base.Add(newAuthor);
	}

	public void Remove(Author existingAuthor)
	{
		base.Remove(existingAuthor);
	}

	public bool Contains(Author existingAuthor)
	{
		return base.Contains(existingAuthor);
	}

	public int IndexOf(Author existingAuthor)
	{
		return base.IndexOf(existingAuthor);
	}
	
	public AuthorList Find(string qualifierFormat, params object[] parameters)
	{
		AuthorList resultSet = new AuthorList();
		base.Find(resultSet, qualifierFormat, parameters);
		return resultSet;
	}

	public new Author FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (Author)base.FindUnique(qualifierFormat, parameters);
	}
	
	public new Author FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (Author)base.FindFirst(qualifierFormat, parameters);
	}
	
}


public class AuthorRelation : ObjectRelationBase
{
	public AuthorRelation(IEntityObject eo, string aRelation) : base(eo, aRelation)
	{
	}

	public Author this[int index]
	{
		get { return (Author)InnerList[index]; }
	}

	public int Add(Author newAuthor)
	{
		return base.Add(newAuthor);
	}

	public void Remove(Author existingAuthor)
	{
		base.Remove(existingAuthor);
	}

	public bool Contains(Author existingAuthor)
	{
		return base.Contains(existingAuthor);
	}

	public int IndexOf(Author existingAuthor)
	{
		return base.IndexOf(existingAuthor);
	}

	public AuthorList GetReadOnlyList()
	{
		AuthorList resultSet = new AuthorList();
		base.CopyToListAndMakeReadOnly(resultSet);
		return resultSet;
	}
	
	public AuthorList GetSortedList(string propName, SortDirection dir)
	{
		AuthorList resultSet = new AuthorList();
		base.CopyToListAndSort(resultSet, propName, dir);
		return resultSet;
	}

	public AuthorList Find(string qualifierFormat, params object[] parameters)
	{
		AuthorList resultSet = new AuthorList();
		base.Find(resultSet, qualifierFormat, parameters);
		return resultSet;
	}

	public new Author FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (Author)base.FindUnique(qualifierFormat, parameters);
	}
	
	public new Author FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (Author)base.FindFirst(qualifierFormat, parameters);
	}

}


//-------- Object Factory ------------------------------------------------

public class AuthorFactory : Neo.Framework.ObjectFactory
{
	public AuthorFactory(ObjectContext context) : base(context, typeof(Author))
	{
	}

	public Author CreateObject(System.String arg0)
	{
		return (Author)base.CreateObject(new object[] { arg0 } );
	}
	
	public Author FindObject(System.String arg0)
	{
		return (Author)base.FindObject(new object[] { arg0 } );
	}

	public new AuthorList FindAllObjects()
	{
		AuthorList c = new AuthorList();
		foreach(Author eo in base.FindAllObjects())
			c.Add(eo);
		return c;
	}
	
	public AuthorTemplate GetQueryTemplate()
	{
		return new AuthorTemplate(EntityMap);
	}
	
	public AuthorList Find(AuthorTemplate template)
	{
		AuthorList c = new AuthorList();
		foreach(Author eo in base.Find(template))
			c.Add(eo);
		return c;
	}

	public AuthorList Find(FetchSpecification fetchSpecification)
	{
		AuthorList c = new AuthorList();
		foreach(Author eo in base.Find(fetchSpecification))
			c.Add(eo);
		return c;
	}
	
	public new AuthorList Find(string qualifierFormat, params object[] parameters)
	{
		AuthorList c = new AuthorList();
		foreach(Author eo in base.Find(qualifierFormat, parameters))
			c.Add(eo);
		return c;
	}

	public new AuthorList FindWithLimit(int limit, string qualifierFormat, params object[] parameters)
	{
		AuthorList c = new AuthorList();
		foreach(Author eo in base.Find(qualifierFormat, parameters))
			c.Add(eo);
		return c;
	}
	
	public new Author FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (Author)base.FindFirst(qualifierFormat, parameters);
	}

	public new Author FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (Author)base.FindUnique(qualifierFormat, parameters);
	}	

	
}


//-------- Entity Map -----------------------------------------------------

internal class AuthorMap : EntityMap
{
    private static readonly string[] pkcolumns = { "au_id" };
    private static readonly string[] columns = { "au_id", "au_lname", "au_fname", "phone", "contract" };
    private static readonly string[] attributes = { "AuId", "LastName", "FirstName", "Phone", "ContractSigned" };
    private static readonly string[] relations = { "TitleAuthors" };
    
    private Type concreteObjectType = typeof(Author);				/* #custom# added */

    public override System.Type ObjectType
    {
        get { return typeof(Author); }
    }
    
    public override System.Type ConcreteObjectType
    {
        get { return concreteObjectType; }										 /* #custom# overridden */
        set { concreteObjectType = value; Factory.AddCustomType(value, this); }  /* #custom# overridden */
    }
    
    public override string TableName
    {
        get { return "authors"; }
    }
    
    public override string[] PrimaryKeyColumns
    {
        get { return pkcolumns; }
    }

    public override string[] Columns
    {
        get { return columns; }
    }

    public override string[] Attributes
    {
        get { return attributes; }
    }

    public override string[] Relations
    {
        get { return relations; }
    }

  	protected override IDictionary GetRelationInfos()
    {
    	IDictionary infos = new Hashtable(1);
  		infos.Add("TitleAuthors", new RelationInfo(Factory, typeof(Author), typeof(TitleAuthor), "au_id", "au_id"));
		return infos;    	
    }
    
	public override IPkInitializer GetPkInitializer()
	{
		return new UserPkInitializer();
	}
	
	public override IEntityObject CreateInstance(DataRow row, ObjectContext context) /* #custom# overridden */
	{
		if(ConcreteObjectType != ObjectType)
			return base.CreateInstance(row, context);
		return new Author(row, context);
	}
    
     protected override void WriteBasicSchema(DataTable table)
    {
		DataColumn		c;
		
		c = table.Columns.Add("au_id", typeof(System.String));
		c.Unique = true;
		c = table.Columns.Add("au_lname", typeof(System.String));
		c = table.Columns.Add("au_fname", typeof(System.String));
		c = table.Columns.Add("phone", typeof(System.String));
		c = table.Columns.Add("contract", typeof(System.Boolean));
		table.PrimaryKey = new DataColumn[] { table.Columns["au_id"] };
	}
	
	protected override void WriteRelations(DataTable table)
	{
		DataRelation r;
		
 		if(table.DataSet.Relations["authors*titleauthor.au_id"] == null)
		{
			r = table.DataSet.Relations.Add("authors*titleauthor.au_id", 
					table.DataSet.Tables["authors"].Columns["au_id"],
					table.DataSet.Tables["titleauthor"].Columns["au_id"]);
			r.ChildKeyConstraint.UpdateRule = Rule.Cascade;
			r.ChildKeyConstraint.DeleteRule = Rule.Cascade;
		}
	}
}

}

/* This is a customised template.
 * It overrides some optional methods from EntityMap to allow for testing 
 * with Mocks and fast object creation. Search for #custom# below. 
 */


namespace Pubs4.Model
{
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using Neo.Core;
using Neo.Core.Util;
using Neo.Framework;

//-------- Base Class ----------------------------------------------------

public class TitleAuthorBase : EntityObject
{
       
	protected TitleAuthorBase(System.Data.DataRow aRow, Neo.Core.ObjectContext aContext) : base(aRow, aContext)
	{
	}
	

	public virtual Title Title
	{
		get { object fk = Row["title_id"]; return (fk == DBNull.Value) ? null : (Title)GetRelatedObject("titles", fk); }
		set { SetRelatedObject(value, "title_id", "title_id" ); }
	}

	public virtual Author Author
	{
		get { object fk = Row["au_id"]; return (fk == DBNull.Value) ? null : (Author)GetRelatedObject("authors", fk); }
		set { SetRelatedObject(value, "au_id", "au_id" ); }
	}


	public override object GetProperty(string propName)
	{
		if(propName == "Title") 
			return Title;
		if(propName == "Author") 
			return Author;

		return base.GetProperty(propName);
	}

}


//-------- Query Template ------------------------------------------------

public class TitleAuthorTemplate : IFetchSpecification
{
	private IEntityMap entityMap;
	private ListDictionary queryValues;
	private int fetchLimit;
	private PropertyComparer[] sortOrderings;
	
	public TitleAuthorTemplate(IEntityMap anEntityMap)
	{
		entityMap = anEntityMap;
		queryValues = new ListDictionary();
		fetchLimit = -1;
	}

	public IEntityMap EntityMap 
	{ 
		get { return entityMap; } 
	}

	public Qualifier Qualifier
	{
		get { return Qualifier.FromPropertyDictionary(queryValues); }
	}
	
	public Int32 FetchLimit
	{
		get { return fetchLimit; }
		set { fetchLimit = value; }
	}
	
	public PropertyComparer[] SortOrderings
	{
		get { return sortOrderings; }
		set { sortOrderings = value; }
	}
	
	public Title Title
	{
		get { return (Title)queryValues["Title"]; }
		set { queryValues["Title"] = value; }
	}

	public Author Author
	{
		get { return (Author)queryValues["Author"]; }
		set { queryValues["Author"] = value; }
	}

               
}


//-------- Typed Collections ----------------------------------------------

public class TitleAuthorList : ObjectListBase
{
	public TitleAuthorList()
	{
	}

	public TitleAuthorList(IList list)
	{
		((ArrayList)InnerList).AddRange(list);
	}

	public TitleAuthor this[int index]
	{
		get { return (TitleAuthor)InnerList[index]; }
	}

	public int Add(TitleAuthor newTitleAuthor)
	{
		return base.Add(newTitleAuthor);
	}

	public void Remove(TitleAuthor existingTitleAuthor)
	{
		base.Remove(existingTitleAuthor);
	}

	public bool Contains(TitleAuthor existingTitleAuthor)
	{
		return base.Contains(existingTitleAuthor);
	}

	public int IndexOf(TitleAuthor existingTitleAuthor)
	{
		return base.IndexOf(existingTitleAuthor);
	}
	
	public TitleAuthorList Find(string qualifierFormat, params object[] parameters)
	{
		TitleAuthorList resultSet = new TitleAuthorList();
		base.Find(resultSet, qualifierFormat, parameters);
		return resultSet;
	}

	public new TitleAuthor FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (TitleAuthor)base.FindUnique(qualifierFormat, parameters);
	}
	
	public new TitleAuthor FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (TitleAuthor)base.FindFirst(qualifierFormat, parameters);
	}
	
}


public class TitleAuthorRelation : ObjectRelationBase
{
	public TitleAuthorRelation(IEntityObject eo, string aRelation) : base(eo, aRelation)
	{
	}

	public TitleAuthor this[int index]
	{
		get { return (TitleAuthor)InnerList[index]; }
	}

	public int Add(TitleAuthor newTitleAuthor)
	{
		return base.Add(newTitleAuthor);
	}

	public void Remove(TitleAuthor existingTitleAuthor)
	{
		base.Remove(existingTitleAuthor);
	}

	public bool Contains(TitleAuthor existingTitleAuthor)
	{
		return base.Contains(existingTitleAuthor);
	}

	public int IndexOf(TitleAuthor existingTitleAuthor)
	{
		return base.IndexOf(existingTitleAuthor);
	}

	public TitleAuthorList GetReadOnlyList()
	{
		TitleAuthorList resultSet = new TitleAuthorList();
		base.CopyToListAndMakeReadOnly(resultSet);
		return resultSet;
	}
	
	public TitleAuthorList GetSortedList(string propName, SortDirection dir)
	{
		TitleAuthorList resultSet = new TitleAuthorList();
		base.CopyToListAndSort(resultSet, propName, dir);
		return resultSet;
	}

	public TitleAuthorList Find(string qualifierFormat, params object[] parameters)
	{
		TitleAuthorList resultSet = new TitleAuthorList();
		base.Find(resultSet, qualifierFormat, parameters);
		return resultSet;
	}

	public new TitleAuthor FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (TitleAuthor)base.FindUnique(qualifierFormat, parameters);
	}
	
	public new TitleAuthor FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (TitleAuthor)base.FindFirst(qualifierFormat, parameters);
	}

}


//-------- Object Factory ------------------------------------------------

public class TitleAuthorFactory : Neo.Framework.ObjectFactory
{
	public TitleAuthorFactory(ObjectContext context) : base(context, typeof(TitleAuthor))
	{
	}

	public TitleAuthor CreateObject(Author arg0, Title arg1)
	{
		return (TitleAuthor)base.CreateObject(new object[] { arg0.Row["au_id"], arg1.Row["title_id"] } );
	}

	public TitleAuthor FindObject(Author arg0, Title arg1)
	{
		return (TitleAuthor)base.FindObject(new object[] { arg0.Row["au_id"], arg1.Row["title_id"] } );
	}

	public new TitleAuthorList FindAllObjects()
	{
		TitleAuthorList c = new TitleAuthorList();
		foreach(TitleAuthor eo in base.FindAllObjects())
			c.Add(eo);
		return c;
	}
	
	public TitleAuthorTemplate GetQueryTemplate()
	{
		return new TitleAuthorTemplate(EntityMap);
	}
	
	public TitleAuthorList Find(TitleAuthorTemplate template)
	{
		TitleAuthorList c = new TitleAuthorList();
		foreach(TitleAuthor eo in base.Find(template))
			c.Add(eo);
		return c;
	}

	public TitleAuthorList Find(FetchSpecification fetchSpecification)
	{
		TitleAuthorList c = new TitleAuthorList();
		foreach(TitleAuthor eo in base.Find(fetchSpecification))
			c.Add(eo);
		return c;
	}
	
	public new TitleAuthorList Find(string qualifierFormat, params object[] parameters)
	{
		TitleAuthorList c = new TitleAuthorList();
		foreach(TitleAuthor eo in base.Find(qualifierFormat, parameters))
			c.Add(eo);
		return c;
	}

	public new TitleAuthorList FindWithLimit(int limit, string qualifierFormat, params object[] parameters)
	{
		TitleAuthorList c = new TitleAuthorList();
		foreach(TitleAuthor eo in base.Find(qualifierFormat, parameters))
			c.Add(eo);
		return c;
	}
	
	public new TitleAuthor FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (TitleAuthor)base.FindFirst(qualifierFormat, parameters);
	}

	public new TitleAuthor FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (TitleAuthor)base.FindUnique(qualifierFormat, parameters);
	}	

	
}


//-------- Entity Map -----------------------------------------------------

internal class TitleAuthorMap : EntityMap
{
    private static readonly string[] pkcolumns = { "au_id", "title_id" };
    private static readonly string[] columns = { "au_id", "title_id" };
    private static readonly string[] attributes = { "AuId", "TitleId" };
    private static readonly string[] relations = { "Title", "Author" };
    
    private Type concreteObjectType = typeof(TitleAuthor);				/* #custom# added */

    public override System.Type ObjectType
    {
        get { return typeof(TitleAuthor); }
    }
    
    public override System.Type ConcreteObjectType
    {
        get { return concreteObjectType; }										 /* #custom# overridden */
        set { concreteObjectType = value; Factory.AddCustomType(value, this); }  /* #custom# overridden */
    }
    
    public override string TableName
    {
        get { return "titleauthor"; }
    }
    
    public override string[] PrimaryKeyColumns
    {
        get { return pkcolumns; }
    }

    public override string[] Columns
    {
        get { return columns; }
    }

    public override string[] Attributes
    {
        get { return attributes; }
    }

    public override string[] Relations
    {
        get { return relations; }
    }

  	protected override IDictionary GetRelationInfos()
    {
    	IDictionary infos = new Hashtable(2);
  		infos.Add("Title", new RelationInfo(Factory, typeof(Title), typeof(TitleAuthor), "title_id", "title_id"));
 		infos.Add("Author", new RelationInfo(Factory, typeof(Author), typeof(TitleAuthor), "au_id", "au_id"));
		return infos;    	
    }
    
	public override IPkInitializer GetPkInitializer()
	{
		return new UserPkInitializer();
	}
	
	public override IEntityObject CreateInstance(DataRow row, ObjectContext context) /* #custom# overridden */
	{
		if(ConcreteObjectType != ObjectType)
			return base.CreateInstance(row, context);
		return new TitleAuthor(row, context);
	}
    
     protected override void WriteBasicSchema(DataTable table)
    {
		DataColumn		c;
		
		c = table.Columns.Add("au_id", typeof(System.String));
		c = table.Columns.Add("title_id", typeof(System.String));
		table.PrimaryKey = new DataColumn[] { table.Columns["au_id"], table.Columns["title_id"] };
	}
	
	protected override void WriteRelations(DataTable table)
	{
		DataRelation r;
		
		if(table.DataSet.Relations["titles*titleauthor.title_id"] == null)
		{
			r = table.DataSet.Relations.Add("titles*titleauthor.title_id", 
					table.DataSet.Tables["titles"].Columns["title_id"],
					table.DataSet.Tables["titleauthor"].Columns["title_id"]);
			r.ChildKeyConstraint.UpdateRule = Rule.Cascade;
			r.ChildKeyConstraint.DeleteRule = Rule.Cascade;
		}
		if(table.DataSet.Relations["authors*titleauthor.au_id"] == null)
		{
			r = table.DataSet.Relations.Add("authors*titleauthor.au_id", 
					table.DataSet.Tables["authors"].Columns["au_id"],
					table.DataSet.Tables["titleauthor"].Columns["au_id"]);
			r.ChildKeyConstraint.UpdateRule = Rule.Cascade;
			r.ChildKeyConstraint.DeleteRule = Rule.Cascade;
		}
	}
}

}

/* This is a customised template.
 * It overrides some optional methods from EntityMap to allow for testing 
 * with Mocks and fast object creation. Search for #custom# below. 
 */


namespace Pubs4.Model
{
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using Neo.Core;
using Neo.Framework;

//-------- Base Class ----------------------------------------------------

public class JobBase : EntityObject
{
       
	protected JobBase(System.Data.DataRow aRow, Neo.Core.ObjectContext aContext) : base(aRow, aContext)
	{
	}
	
	public virtual System.Int16 JobId
	{
		get { object v = Row["job_id"]; return (System.Int16)((v != DBNull.Value) ? v : HandleNullValueForProperty("JobId")); }
		set { Row["job_id"] = value; }
	}    

	public virtual System.String Description
	{
		get { return Row["job_desc"] as System.String; }
		set { Row["job_desc"] = (value != null) ? (object)value : (object)DBNull.Value; }
	}    

	public virtual System.Int16 MinLevel
	{
		get { object v = Row["min_lvl"]; return (System.Int16)((v != DBNull.Value) ? v : HandleNullValueForProperty("MinLevel")); }
		set { Row["min_lvl"] = value; }
	}    

	public virtual System.Int16 MaxLevel
	{
		get { object v = Row["max_lvl"]; return (System.Int16)((v != DBNull.Value) ? v : HandleNullValueForProperty("MaxLevel")); }
		set { Row["max_lvl"] = value; }
	}    



	public override object GetProperty(string propName)
	{
		if(propName == "JobId") 
			return JobId;
		if(propName == "Description") 
			return Description;
		if(propName == "MinLevel") 
			return MinLevel;
		if(propName == "MaxLevel") 
			return MaxLevel;

		return base.GetProperty(propName);
	}

}


//-------- Query Template ------------------------------------------------

public class JobTemplate : IFetchSpecification
{
	private IEntityMap entityMap;
	private ListDictionary queryValues;
	private int fetchLimit;
	private PropertyComparer[] sortOrderings;
	
	public JobTemplate(IEntityMap anEntityMap)
	{
		entityMap = anEntityMap;
		queryValues = new ListDictionary();
		fetchLimit = -1;
	}

	public IEntityMap EntityMap 
	{ 
		get { return entityMap; } 
	}

	public Qualifier Qualifier
	{
		get { return Qualifier.FromPropertyDictionary(queryValues); }
	}
	
	public Int32 FetchLimit
	{
		get { return fetchLimit; }
		set { fetchLimit = value; }
	}
	
	public PropertyComparer[] SortOrderings
	{
		get { return sortOrderings; }
		set { sortOrderings = value; }
	}
	
	public System.Int16 JobId
	{
		get { return (System.Int16)queryValues["JobId"]; }
		set { queryValues["JobId"] = value; }
	}

	public System.String Description
	{
		get { return (System.String)queryValues["Description"]; }
		set { queryValues["Description"] = value; }
	}

	public System.Int16 MinLevel
	{
		get { return (System.Int16)queryValues["MinLevel"]; }
		set { queryValues["MinLevel"] = value; }
	}

	public System.Int16 MaxLevel
	{
		get { return (System.Int16)queryValues["MaxLevel"]; }
		set { queryValues["MaxLevel"] = value; }
	}

               
}


//-------- Typed Collections ----------------------------------------------

public class JobList : ObjectListBase
{
	public JobList()
	{
	}

	public JobList(IList list)
	{
		((ArrayList)InnerList).AddRange(list);
	}

	public Job this[int index]
	{
		get { return (Job)InnerList[index]; }
	}

	public int Add(Job newJob)
	{
		return base.Add(newJob);
	}

	public void Remove(Job existingJob)
	{
		base.Remove(existingJob);
	}

	public bool Contains(Job existingJob)
	{
		return base.Contains(existingJob);
	}

	public int IndexOf(Job existingJob)
	{
		return base.IndexOf(existingJob);
	}
	
	public JobList Find(string qualifierFormat, params object[] parameters)
	{
		JobList resultSet = new JobList();
		base.Find(resultSet, qualifierFormat, parameters);
		return resultSet;
	}

	public new Job FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (Job)base.FindUnique(qualifierFormat, parameters);
	}
	
	public new Job FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (Job)base.FindFirst(qualifierFormat, parameters);
	}
	
}


public class JobRelation : ObjectRelationBase
{
	public JobRelation(IEntityObject eo, string aRelation) : base(eo, aRelation)
	{
	}

	public Job this[int index]
	{
		get { return (Job)InnerList[index]; }
	}

	public int Add(Job newJob)
	{
		return base.Add(newJob);
	}

	public void Remove(Job existingJob)
	{
		base.Remove(existingJob);
	}

	public bool Contains(Job existingJob)
	{
		return base.Contains(existingJob);
	}

	public int IndexOf(Job existingJob)
	{
		return base.IndexOf(existingJob);
	}

	public JobList GetReadOnlyList()
	{
		JobList resultSet = new JobList();
		base.CopyToListAndMakeReadOnly(resultSet);
		return resultSet;
	}
	
	public JobList GetSortedList(string propName, SortDirection dir)
	{
		JobList resultSet = new JobList();
		base.CopyToListAndSort(resultSet, propName, dir);
		return resultSet;
	}

	public JobList Find(string qualifierFormat, params object[] parameters)
	{
		JobList resultSet = new JobList();
		base.Find(resultSet, qualifierFormat, parameters);
		return resultSet;
	}

	public new Job FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (Job)base.FindUnique(qualifierFormat, parameters);
	}
	
	public new Job FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (Job)base.FindFirst(qualifierFormat, parameters);
	}

}


//-------- Object Factory ------------------------------------------------

public class JobFactory : Neo.Framework.ObjectFactory
{
	public JobFactory(ObjectContext context) : base(context, typeof(Job))
	{
	}

	public Job CreateObject()
	{
		return (Job)base.CreateObject(null);
	}
	
	public Job FindObject(System.Int16 arg0)
	{
		return (Job)base.FindObject(new object[] { arg0 } );
	}

	public new JobList FindAllObjects()
	{
		JobList c = new JobList();
		foreach(Job eo in base.FindAllObjects())
			c.Add(eo);
		return c;
	}
	
	public JobTemplate GetQueryTemplate()
	{
		return new JobTemplate(EntityMap);
	}
	
	public JobList Find(JobTemplate template)
	{
		JobList c = new JobList();
		foreach(Job eo in base.Find(template))
			c.Add(eo);
		return c;
	}

	public JobList Find(FetchSpecification fetchSpecification)
	{
		JobList c = new JobList();
		foreach(Job eo in base.Find(fetchSpecification))
			c.Add(eo);
		return c;
	}
	
	public new JobList Find(string qualifierFormat, params object[] parameters)
	{
		JobList c = new JobList();
		foreach(Job eo in base.Find(qualifierFormat, parameters))
			c.Add(eo);
		return c;
	}

	public new JobList FindWithLimit(int limit, string qualifierFormat, params object[] parameters)
	{
		JobList c = new JobList();
		foreach(Job eo in base.Find(qualifierFormat, parameters))
			c.Add(eo);
		return c;
	}
	
	public new Job FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (Job)base.FindFirst(qualifierFormat, parameters);
	}

	public new Job FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (Job)base.FindUnique(qualifierFormat, parameters);
	}	

	
}


//-------- Entity Map -----------------------------------------------------

internal class JobMap : EntityMap
{
    private static readonly string[] pkcolumns = { "job_id" };
    private static readonly string[] columns = { "job_id", "job_desc", "min_lvl", "max_lvl" };
    private static readonly string[] attributes = { "JobId", "Description", "MinLevel", "MaxLevel" };
    private static readonly string[] relations = { };
    
    private Type concreteObjectType = typeof(Job);				/* #custom# added */

    public override System.Type ObjectType
    {
        get { return typeof(Job); }
    }
    
    public override System.Type ConcreteObjectType
    {
        get { return concreteObjectType; }										 /* #custom# overridden */
        set { concreteObjectType = value; Factory.AddCustomType(value, this); }  /* #custom# overridden */
    }
    
    public override string TableName
    {
        get { return "jobs"; }
    }
    
    public override string[] PrimaryKeyColumns
    {
        get { return pkcolumns; }
    }

    public override string[] Columns
    {
        get { return columns; }
    }

    public override string[] Attributes
    {
        get { return attributes; }
    }

    public override string[] Relations
    {
        get { return relations; }
    }

  	protected override IDictionary GetRelationInfos()
    {
    	IDictionary infos = new Hashtable(0);
 		return infos;    	
    }
    
	public override IPkInitializer GetPkInitializer()
	{
		return new NativePkInitializer();
	}
	
	public override IEntityObject CreateInstance(DataRow row, ObjectContext context) /* #custom# overridden */
	{
		if(ConcreteObjectType != ObjectType)
			return base.CreateInstance(row, context);
		return new Job(row, context);
	}
    
     protected override void WriteBasicSchema(DataTable table)
    {
		DataColumn		c;
		
		c = table.Columns.Add("job_id", typeof(System.Int16));
		c.Unique = true;
		c.AutoIncrement = true; c.AutoIncrementSeed = c.AutoIncrementStep = -1;
		c = table.Columns.Add("job_desc", typeof(System.String));
		c = table.Columns.Add("min_lvl", typeof(System.Int16));
		c = table.Columns.Add("max_lvl", typeof(System.Int16));
		table.PrimaryKey = new DataColumn[] { table.Columns["job_id"] };
	}
	
}

}

/* This is a customised template.
 * It overrides some optional methods from EntityMap to allow for testing 
 * with Mocks and fast object creation. Search for #custom# below. 
 */


namespace Pubs4.Model
{
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using Neo.Core;
using Neo.Framework;

//-------- Base Class ----------------------------------------------------

public class StoreBase : EntityObject
{
       
	protected StoreBase(System.Data.DataRow aRow, Neo.Core.ObjectContext aContext) : base(aRow, aContext)
	{
	}
	
	public virtual System.String Name
	{
		get { return Row["stor_name"] as System.String; }
		set { Row["stor_name"] = (value != null) ? (object)value : (object)DBNull.Value; }
	}    

	public virtual System.String Address
	{
		get { return Row["stor_address"] as System.String; }
		set { Row["stor_address"] = (value != null) ? (object)value : (object)DBNull.Value; }
	}    

	public virtual System.String City
	{
		get { return Row["city"] as System.String; }
		set { Row["city"] = (value != null) ? (object)value : (object)DBNull.Value; }
	}    

	public virtual System.String State
	{
		get { return Row["state"] as System.String; }
		set { Row["state"] = (value != null) ? (object)value : (object)DBNull.Value; }
	}    

	public virtual System.String Zip
	{
		get { return Row["zip"] as System.String; }
		set { Row["zip"] = (value != null) ? (object)value : (object)DBNull.Value; }
	}    



	public override object GetProperty(string propName)
	{
		if(propName == "Name") 
			return Name;
		if(propName == "Address") 
			return Address;
		if(propName == "City") 
			return City;
		if(propName == "State") 
			return State;
		if(propName == "Zip") 
			return Zip;

		return base.GetProperty(propName);
	}

}


//-------- Query Template ------------------------------------------------

public class StoreTemplate : IFetchSpecification
{
	private IEntityMap entityMap;
	private ListDictionary queryValues;
	private int fetchLimit;
	private PropertyComparer[] sortOrderings;
	
	public StoreTemplate(IEntityMap anEntityMap)
	{
		entityMap = anEntityMap;
		queryValues = new ListDictionary();
		fetchLimit = -1;
	}

	public IEntityMap EntityMap 
	{ 
		get { return entityMap; } 
	}

	public Qualifier Qualifier
	{
		get { return Qualifier.FromPropertyDictionary(queryValues); }
	}
	
	public Int32 FetchLimit
	{
		get { return fetchLimit; }
		set { fetchLimit = value; }
	}
	
	public PropertyComparer[] SortOrderings
	{
		get { return sortOrderings; }
		set { sortOrderings = value; }
	}
	
	public System.String Name
	{
		get { return (System.String)queryValues["Name"]; }
		set { queryValues["Name"] = value; }
	}

	public System.String Address
	{
		get { return (System.String)queryValues["Address"]; }
		set { queryValues["Address"] = value; }
	}

	public System.String City
	{
		get { return (System.String)queryValues["City"]; }
		set { queryValues["City"] = value; }
	}

	public System.String State
	{
		get { return (System.String)queryValues["State"]; }
		set { queryValues["State"] = value; }
	}

	public System.String Zip
	{
		get { return (System.String)queryValues["Zip"]; }
		set { queryValues["Zip"] = value; }
	}

               
}


//-------- Typed Collections ----------------------------------------------

public class StoreList : ObjectListBase
{
	public StoreList()
	{
	}

	public StoreList(IList list)
	{
		((ArrayList)InnerList).AddRange(list);
	}

	public Store this[int index]
	{
		get { return (Store)InnerList[index]; }
	}

	public int Add(Store newStore)
	{
		return base.Add(newStore);
	}

	public void Remove(Store existingStore)
	{
		base.Remove(existingStore);
	}

	public bool Contains(Store existingStore)
	{
		return base.Contains(existingStore);
	}

	public int IndexOf(Store existingStore)
	{
		return base.IndexOf(existingStore);
	}
	
	public StoreList Find(string qualifierFormat, params object[] parameters)
	{
		StoreList resultSet = new StoreList();
		base.Find(resultSet, qualifierFormat, parameters);
		return resultSet;
	}

	public new Store FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (Store)base.FindUnique(qualifierFormat, parameters);
	}
	
	public new Store FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (Store)base.FindFirst(qualifierFormat, parameters);
	}
	
}


public class StoreRelation : ObjectRelationBase
{
	public StoreRelation(IEntityObject eo, string aRelation) : base(eo, aRelation)
	{
	}

	public Store this[int index]
	{
		get { return (Store)InnerList[index]; }
	}

	public int Add(Store newStore)
	{
		return base.Add(newStore);
	}

	public void Remove(Store existingStore)
	{
		base.Remove(existingStore);
	}

	public bool Contains(Store existingStore)
	{
		return base.Contains(existingStore);
	}

	public int IndexOf(Store existingStore)
	{
		return base.IndexOf(existingStore);
	}

	public StoreList GetReadOnlyList()
	{
		StoreList resultSet = new StoreList();
		base.CopyToListAndMakeReadOnly(resultSet);
		return resultSet;
	}
	
	public StoreList GetSortedList(string propName, SortDirection dir)
	{
		StoreList resultSet = new StoreList();
		base.CopyToListAndSort(resultSet, propName, dir);
		return resultSet;
	}

	public StoreList Find(string qualifierFormat, params object[] parameters)
	{
		StoreList resultSet = new StoreList();
		base.Find(resultSet, qualifierFormat, parameters);
		return resultSet;
	}

	public new Store FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (Store)base.FindUnique(qualifierFormat, parameters);
	}
	
	public new Store FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (Store)base.FindFirst(qualifierFormat, parameters);
	}

}


//-------- Object Factory ------------------------------------------------

public class StoreFactory : Neo.Framework.ObjectFactory
{
	public StoreFactory(ObjectContext context) : base(context, typeof(Store))
	{
	}

	public Store CreateObject(System.String arg0)
	{
		return (Store)base.CreateObject(new object[] { arg0 } );
	}
	
	public Store FindObject(System.String arg0)
	{
		return (Store)base.FindObject(new object[] { arg0 } );
	}

	public new StoreList FindAllObjects()
	{
		StoreList c = new StoreList();
		foreach(Store eo in base.FindAllObjects())
			c.Add(eo);
		return c;
	}
	
	public StoreTemplate GetQueryTemplate()
	{
		return new StoreTemplate(EntityMap);
	}
	
	public StoreList Find(StoreTemplate template)
	{
		StoreList c = new StoreList();
		foreach(Store eo in base.Find(template))
			c.Add(eo);
		return c;
	}

	public StoreList Find(FetchSpecification fetchSpecification)
	{
		StoreList c = new StoreList();
		foreach(Store eo in base.Find(fetchSpecification))
			c.Add(eo);
		return c;
	}
	
	public new StoreList Find(string qualifierFormat, params object[] parameters)
	{
		StoreList c = new StoreList();
		foreach(Store eo in base.Find(qualifierFormat, parameters))
			c.Add(eo);
		return c;
	}

	public new StoreList FindWithLimit(int limit, string qualifierFormat, params object[] parameters)
	{
		StoreList c = new StoreList();
		foreach(Store eo in base.Find(qualifierFormat, parameters))
			c.Add(eo);
		return c;
	}
	
	public new Store FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (Store)base.FindFirst(qualifierFormat, parameters);
	}

	public new Store FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (Store)base.FindUnique(qualifierFormat, parameters);
	}	

	
}


//-------- Entity Map -----------------------------------------------------

internal class StoreMap : EntityMap
{
    private static readonly string[] pkcolumns = { "stor_id" };
    private static readonly string[] columns = { "stor_id", "stor_name", "stor_address", "city", "state", "zip" };
    private static readonly string[] attributes = { "StorId", "Name", "Address", "City", "State", "Zip" };
    private static readonly string[] relations = { };
    
    private Type concreteObjectType = typeof(Store);				/* #custom# added */

    public override System.Type ObjectType
    {
        get { return typeof(Store); }
    }
    
    public override System.Type ConcreteObjectType
    {
        get { return concreteObjectType; }										 /* #custom# overridden */
        set { concreteObjectType = value; Factory.AddCustomType(value, this); }  /* #custom# overridden */
    }
    
    public override string TableName
    {
        get { return "stores"; }
    }
    
    public override string[] PrimaryKeyColumns
    {
        get { return pkcolumns; }
    }

    public override string[] Columns
    {
        get { return columns; }
    }

    public override string[] Attributes
    {
        get { return attributes; }
    }

    public override string[] Relations
    {
        get { return relations; }
    }

  	protected override IDictionary GetRelationInfos()
    {
    	IDictionary infos = new Hashtable(0);
 		return infos;    	
    }
    
	public override IPkInitializer GetPkInitializer()
	{
		return new UserPkInitializer();
	}
	
	public override IEntityObject CreateInstance(DataRow row, ObjectContext context) /* #custom# overridden */
	{
		if(ConcreteObjectType != ObjectType)
			return base.CreateInstance(row, context);
		return new Store(row, context);
	}
    
     protected override void WriteBasicSchema(DataTable table)
    {
		DataColumn		c;
		
		c = table.Columns.Add("stor_id", typeof(System.String));
		c.Unique = true;
		c = table.Columns.Add("stor_name", typeof(System.String));
		c.AllowDBNull = true;
		c = table.Columns.Add("stor_address", typeof(System.String));
		c.AllowDBNull = true;
		c = table.Columns.Add("city", typeof(System.String));
		c.AllowDBNull = true;
		c = table.Columns.Add("state", typeof(System.String));
		c.AllowDBNull = true;
		c = table.Columns.Add("zip", typeof(System.String));
		c.AllowDBNull = true;
		table.PrimaryKey = new DataColumn[] { table.Columns["stor_id"] };
	}
	
}

}

/* This is a customised template.
 * It overrides some optional methods from EntityMap to allow for testing 
 * with Mocks and fast object creation. Search for #custom# below. 
 */


namespace Pubs4.Model
{
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using Neo.Core;
using Neo.Core.Util;
using Neo.Framework;

//-------- Base Class ----------------------------------------------------

public class DiscountBase : EntityObject
{
       
	protected DiscountBase(System.Data.DataRow aRow, Neo.Core.ObjectContext aContext) : base(aRow, aContext)
	{
	}
	
	public virtual System.String DiscountType
	{
		get { return Row["discounttype"] as System.String; }
		set { Row["discounttype"] = (value != null) ? (object)value : (object)DBNull.Value; }
	}    

	public virtual System.Decimal Value
	{
		get { object v = Row["discount"]; return (System.Decimal)((v != DBNull.Value) ? v : HandleNullValueForProperty("Value")); }
		set { Row["discount"] = value; }
	}    


	public virtual Store Store
	{
		get { object fk = Row["stor_id"]; return (fk == DBNull.Value) ? null : (Store)GetRelatedObject("stores", fk); }
		set { SetRelatedObject(value, "stor_id", "stor_id" ); }
	}


	public override object GetProperty(string propName)
	{
		if(propName == "DiscountType") 
			return DiscountType;
		if(propName == "Value") 
			return Value;
		if(propName == "Store") 
			return Store;

		return base.GetProperty(propName);
	}

}


//-------- Query Template ------------------------------------------------

public class DiscountTemplate : IFetchSpecification
{
	private IEntityMap entityMap;
	private ListDictionary queryValues;
	private int fetchLimit;
	private PropertyComparer[] sortOrderings;
	
	public DiscountTemplate(IEntityMap anEntityMap)
	{
		entityMap = anEntityMap;
		queryValues = new ListDictionary();
		fetchLimit = -1;
	}

	public IEntityMap EntityMap 
	{ 
		get { return entityMap; } 
	}

	public Qualifier Qualifier
	{
		get { return Qualifier.FromPropertyDictionary(queryValues); }
	}
	
	public Int32 FetchLimit
	{
		get { return fetchLimit; }
		set { fetchLimit = value; }
	}
	
	public PropertyComparer[] SortOrderings
	{
		get { return sortOrderings; }
		set { sortOrderings = value; }
	}
	
	public System.String DiscountType
	{
		get { return (System.String)queryValues["DiscountType"]; }
		set { queryValues["DiscountType"] = value; }
	}

	public System.Decimal Value
	{
		get { return (System.Decimal)queryValues["Value"]; }
		set { queryValues["Value"] = value; }
	}

	public Store Store
	{
		get { return (Store)queryValues["Store"]; }
		set { queryValues["Store"] = value; }
	}

               
}


//-------- Typed Collections ----------------------------------------------

public class DiscountList : ObjectListBase
{
	public DiscountList()
	{
	}

	public DiscountList(IList list)
	{
		((ArrayList)InnerList).AddRange(list);
	}

	public Discount this[int index]
	{
		get { return (Discount)InnerList[index]; }
	}

	public int Add(Discount newDiscount)
	{
		return base.Add(newDiscount);
	}

	public void Remove(Discount existingDiscount)
	{
		base.Remove(existingDiscount);
	}

	public bool Contains(Discount existingDiscount)
	{
		return base.Contains(existingDiscount);
	}

	public int IndexOf(Discount existingDiscount)
	{
		return base.IndexOf(existingDiscount);
	}
	
	public DiscountList Find(string qualifierFormat, params object[] parameters)
	{
		DiscountList resultSet = new DiscountList();
		base.Find(resultSet, qualifierFormat, parameters);
		return resultSet;
	}

	public new Discount FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (Discount)base.FindUnique(qualifierFormat, parameters);
	}
	
	public new Discount FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (Discount)base.FindFirst(qualifierFormat, parameters);
	}
	
}


public class DiscountRelation : ObjectRelationBase
{
	public DiscountRelation(IEntityObject eo, string aRelation) : base(eo, aRelation)
	{
	}

	public Discount this[int index]
	{
		get { return (Discount)InnerList[index]; }
	}

	public int Add(Discount newDiscount)
	{
		return base.Add(newDiscount);
	}

	public void Remove(Discount existingDiscount)
	{
		base.Remove(existingDiscount);
	}

	public bool Contains(Discount existingDiscount)
	{
		return base.Contains(existingDiscount);
	}

	public int IndexOf(Discount existingDiscount)
	{
		return base.IndexOf(existingDiscount);
	}

	public DiscountList GetReadOnlyList()
	{
		DiscountList resultSet = new DiscountList();
		base.CopyToListAndMakeReadOnly(resultSet);
		return resultSet;
	}
	
	public DiscountList GetSortedList(string propName, SortDirection dir)
	{
		DiscountList resultSet = new DiscountList();
		base.CopyToListAndSort(resultSet, propName, dir);
		return resultSet;
	}

	public DiscountList Find(string qualifierFormat, params object[] parameters)
	{
		DiscountList resultSet = new DiscountList();
		base.Find(resultSet, qualifierFormat, parameters);
		return resultSet;
	}

	public new Discount FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (Discount)base.FindUnique(qualifierFormat, parameters);
	}
	
	public new Discount FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (Discount)base.FindFirst(qualifierFormat, parameters);
	}

}


//-------- Object Factory ------------------------------------------------

public class DiscountFactory : Neo.Framework.ObjectFactory
{
	public DiscountFactory(ObjectContext context) : base(context, typeof(Discount))
	{
	}

	public Discount CreateObject(System.String arg0)
	{
		return (Discount)base.CreateObject(new object[] { arg0 } );
	}
	
	public Discount FindObject(System.String arg0)
	{
		return (Discount)base.FindObject(new object[] { arg0 } );
	}

	public new DiscountList FindAllObjects()
	{
		DiscountList c = new DiscountList();
		foreach(Discount eo in base.FindAllObjects())
			c.Add(eo);
		return c;
	}
	
	public DiscountTemplate GetQueryTemplate()
	{
		return new DiscountTemplate(EntityMap);
	}
	
	public DiscountList Find(DiscountTemplate template)
	{
		DiscountList c = new DiscountList();
		foreach(Discount eo in base.Find(template))
			c.Add(eo);
		return c;
	}

	public DiscountList Find(FetchSpecification fetchSpecification)
	{
		DiscountList c = new DiscountList();
		foreach(Discount eo in base.Find(fetchSpecification))
			c.Add(eo);
		return c;
	}
	
	public new DiscountList Find(string qualifierFormat, params object[] parameters)
	{
		DiscountList c = new DiscountList();
		foreach(Discount eo in base.Find(qualifierFormat, parameters))
			c.Add(eo);
		return c;
	}

	public new DiscountList FindWithLimit(int limit, string qualifierFormat, params object[] parameters)
	{
		DiscountList c = new DiscountList();
		foreach(Discount eo in base.Find(qualifierFormat, parameters))
			c.Add(eo);
		return c;
	}
	
	public new Discount FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (Discount)base.FindFirst(qualifierFormat, parameters);
	}

	public new Discount FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (Discount)base.FindUnique(qualifierFormat, parameters);
	}	

	
}


//-------- Entity Map -----------------------------------------------------

internal class DiscountMap : EntityMap
{
    private static readonly string[] pkcolumns = { "discounttype" };
    private static readonly string[] columns = { "discounttype", "stor_id", "discount" };
    private static readonly string[] attributes = { "DiscountType", "StorId", "Value" };
    private static readonly string[] relations = { "Store" };
    
    private Type concreteObjectType = typeof(Discount);				/* #custom# added */

    public override System.Type ObjectType
    {
        get { return typeof(Discount); }
    }
    
    public override System.Type ConcreteObjectType
    {
        get { return concreteObjectType; }										 /* #custom# overridden */
        set { concreteObjectType = value; Factory.AddCustomType(value, this); }  /* #custom# overridden */
    }
    
    public override string TableName
    {
        get { return "discounts"; }
    }
    
    public override string[] PrimaryKeyColumns
    {
        get { return pkcolumns; }
    }

    public override string[] Columns
    {
        get { return columns; }
    }

    public override string[] Attributes
    {
        get { return attributes; }
    }

    public override string[] Relations
    {
        get { return relations; }
    }

  	protected override IDictionary GetRelationInfos()
    {
    	IDictionary infos = new Hashtable(1);
  		infos.Add("Store", new RelationInfo(Factory, typeof(Store), typeof(Discount), "stor_id", "stor_id"));
		return infos;    	
    }
    
	public override IPkInitializer GetPkInitializer()
	{
		return new UserPkInitializer();
	}
	
	public override IEntityObject CreateInstance(DataRow row, ObjectContext context) /* #custom# overridden */
	{
		if(ConcreteObjectType != ObjectType)
			return base.CreateInstance(row, context);
		return new Discount(row, context);
	}
    
     protected override void WriteBasicSchema(DataTable table)
    {
		DataColumn		c;
		
		c = table.Columns.Add("discounttype", typeof(System.String));
		c.Unique = true;
		c = table.Columns.Add("stor_id", typeof(System.String));
		c.AllowDBNull = true;
		c = table.Columns.Add("discount", typeof(System.Decimal));
		c.AllowDBNull = true;
		table.PrimaryKey = new DataColumn[] { table.Columns["discounttype"] };
	}
	
	protected override void WriteRelations(DataTable table)
	{
		DataRelation r;
		
		if(table.DataSet.Relations["stores*discounts.stor_id"] == null)
		{
			r = table.DataSet.Relations.Add("stores*discounts.stor_id", 
					table.DataSet.Tables["stores"].Columns["stor_id"],
					table.DataSet.Tables["discounts"].Columns["stor_id"]);
			r.ChildKeyConstraint.UpdateRule = Rule.None;
			r.ChildKeyConstraint.DeleteRule = Rule.None;
		}
	}
}

}

