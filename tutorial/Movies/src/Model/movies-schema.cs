#region Person support classes

namespace Movies.Model
{
#region Using statements

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using Neo.Core;
using Neo.Core.Util;
using Neo.Framework;

#endregion

#region PersonBase

public class PersonBase : EntityObject
{
	protected readonly MovieActorLinkRelation MovieActorLinks;
       
	protected PersonBase(System.Data.DataRow aRow, Neo.Core.ObjectContext aContext) : base(aRow, aContext)
	{
		MovieActorLinks = new MovieActorLinkRelation(this, "MovieActorLinks");
	}	
	
	public virtual System.String Name
	{
		get { return Row["name"] as System.String; }
		set { Row["name"] = (value != null) ? (object)value : (object)DBNull.Value; }
	}    

	public override object GetProperty(string propName)
	{
		if(propName == "Name") 
			return Name;

		return base.GetProperty(propName);
	}

}

#endregion

#region PersonTemplate

public class PersonTemplate : IFetchSpecification
{
	private IEntityMap entityMap;
	private ListDictionary queryValues;
	private int fetchLimit;
	private PropertyComparer[] sortOrderings;
	
	public PersonTemplate(IEntityMap anEntityMap)
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

}

#endregion

#region PersonCollections

#region PersonList

//-------- Typed Collections ----------------------------------------------

public class PersonList : ObjectListBase
{
	public PersonList()
	{
	}

	public PersonList(IList list)
	{
		((ArrayList)InnerList).AddRange(list);
	}

	public Person this[int index]
	{
		get { return (Person)InnerList[index]; }
	}

	public int Add(Person newPerson)
	{
		return base.Add(newPerson);
	}

	public void Remove(Person existingPerson)
	{
		base.Remove(existingPerson);
	}

	public bool Contains(Person existingPerson)
	{
		return base.Contains(existingPerson);
	}

	public int IndexOf(Person existingPerson)
	{
		return base.IndexOf(existingPerson);
	}
	
	public PersonList Find(string qualifierFormat, params object[] parameters)
	{
		PersonList resultSet = new PersonList();
		base.Find(resultSet, qualifierFormat, parameters);
		return resultSet;
	}

	public new Person FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (Person)base.FindUnique(qualifierFormat, parameters);
	}
	
	public new Person FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (Person)base.FindFirst(qualifierFormat, parameters);
	}
	
}

#endregion 

#region PersonRelation

public class PersonRelation : ObjectRelationBase
{
	public PersonRelation(IEntityObject eo, string aRelation) : base(eo, aRelation)
	{
	}

	public Person this[int index]
	{
		get { return (Person)InnerList[index]; }
	}

	public int Add(Person newPerson)
	{
		return base.Add(newPerson);
	}

	public void Remove(Person existingPerson)
	{
		base.Remove(existingPerson);
	}

	public bool Contains(Person existingPerson)
	{
		return base.Contains(existingPerson);
	}

	public int IndexOf(Person existingPerson)
	{
		return base.IndexOf(existingPerson);
	}

	public PersonList GetReadOnlyList()
	{
		PersonList resultSet = new PersonList();
		base.CopyToListAndMakeReadOnly(resultSet);
		return resultSet;
	}
	
	public PersonList GetSortedList(string propName, SortDirection dir)
	{
		PersonList resultSet = new PersonList();
		base.CopyToListAndSort(resultSet, propName, dir);
		return resultSet;
	}

	public PersonList Find(string qualifierFormat, params object[] parameters)
	{
		PersonList resultSet = new PersonList();
		base.Find(resultSet, qualifierFormat, parameters);
		return resultSet;
	}

	public new Person FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (Person)base.FindUnique(qualifierFormat, parameters);
	}
	
	public new Person FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (Person)base.FindFirst(qualifierFormat, parameters);
	}

}

#endregion

#endregion

#region PersonFactory

public class PersonFactory : Neo.Framework.ObjectFactory
{
	public PersonFactory(ObjectContext context) : base(context, typeof(Person))
	{
	}

	public Person CreateObject()
	{
		return (Person)base.CreateObject(null);
	}

	public Person FindObject(System.Int32 arg0)
	{
		return (Person)base.FindObject(new object[] { arg0 } );
	}

	public new PersonList FindAllObjects()
	{
		PersonList c = new PersonList();
		foreach(Person eo in base.FindAllObjects())
			c.Add(eo);
		return c;
	}
	
	public PersonTemplate GetQueryTemplate()
	{
		return new PersonTemplate(EntityMap);
	}
	
	public PersonList Find(PersonTemplate template)
	{
		PersonList c = new PersonList();
		foreach(Person eo in base.Find(template))
			c.Add(eo);
		return c;
	}

	public PersonList Find(FetchSpecification fetchSpecification)
	{
		PersonList c = new PersonList();
		foreach(Person eo in base.Find(fetchSpecification))
			c.Add(eo);
		return c;
	}
	
	public new PersonList Find(string qualifierFormat, params object[] parameters)
	{
		PersonList c = new PersonList();
		foreach(Person eo in base.Find(qualifierFormat, parameters))
			c.Add(eo);
		return c;
	}

	public new PersonList FindWithLimit(int limit, string qualifierFormat, params object[] parameters)
	{
		PersonList c = new PersonList();
		foreach(Person eo in base.Find(qualifierFormat, parameters))
			c.Add(eo);
		return c;
	}
	
	public new Person FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (Person)base.FindFirst(qualifierFormat, parameters);
	}

	public new Person FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (Person)base.FindUnique(qualifierFormat, parameters);
	}	
	
}

#endregion

#region PersonEntityMap

internal class PersonMap : EntityMap
{
    private static readonly string[] pkcolumns = { "person_id" };
    private static readonly string[] columns = { "person_id", "name" };
    private static readonly string[] attributes = { "PersonId", "Name" };
    private static readonly string[] relations = { "MovieActorLinks" };

    public override System.Type ObjectType
    {
        get { return typeof(Person); }
    }
    
    public override string TableName
    {
        get { return "persons"; }
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
  		infos.Add("MovieActorLinks", new RelationInfo(Factory, typeof(Person), typeof(MovieActorLink), "person_id", "actor_id"));
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
		return new Person(row, context);
	}
    
    protected override void WriteBasicSchema(DataTable table)
    {
		DataColumn		c;
		
		c = table.Columns.Add("person_id", typeof(System.Int32));
		c.Unique = true;
		c.AutoIncrement = true; c.AutoIncrementSeed = c.AutoIncrementStep = -1;
		c = table.Columns.Add("name", typeof(System.String));
		table.PrimaryKey = new DataColumn[] { table.Columns["person_id"] };
	}
	
	protected override void WriteRelations(DataTable table)
	{
		DataRelation r;
		
 		if(table.DataSet.Relations["persons*movie_actor.actor_id"] == null)
		{
			r = table.DataSet.Relations.Add("persons*movie_actor.actor_id", 
					table.DataSet.Tables["persons"].Columns["person_id"],
					table.DataSet.Tables["movie_actor"].Columns["actor_id"]);
			r.ChildKeyConstraint.UpdateRule = Rule.Cascade;
			r.ChildKeyConstraint.DeleteRule = Rule.Cascade;
		}
	}
}

#endregion

}

#endregion

#region Movie support classes

namespace Movies.Model
{
#region Using statements

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using Neo.Core;
using Neo.Core.Util;
using Neo.Framework;

#endregion

#region MovieBase

public class MovieBase : EntityObject
{
	protected readonly MovieActorLinkRelation MovieActorLinks;
       
	protected MovieBase(System.Data.DataRow aRow, Neo.Core.ObjectContext aContext) : base(aRow, aContext)
	{
		MovieActorLinks = new MovieActorLinkRelation(this, "MovieActorLinks");
	}	
	
	public virtual System.String Title
	{
		get { return Row["title"] as System.String; }
		set { Row["title"] = (value != null) ? (object)value : (object)DBNull.Value; }
	}    

	public virtual System.Int32 Year
	{
		get { object v = Row["year"]; return (System.Int32)((v != DBNull.Value) ? v : HandleNullValueForProperty("Year")); }
		set { Row["year"] = value; }
	}    

	public virtual Person Director
	{
		get { object fk = Row["director_id"]; return (fk == DBNull.Value) ? null : (Person)GetRelatedObject("persons", fk); }
		set { SetRelatedObject(value, "director_id", "person_id" ); }
	}

	public override object GetProperty(string propName)
	{
		if(propName == "Title") 
			return Title;
		if(propName == "Year") 
			return Year;
		if(propName == "Director") 
			return Director;

		return base.GetProperty(propName);
	}

}

#endregion

#region MovieTemplate

public class MovieTemplate : IFetchSpecification
{
	private IEntityMap entityMap;
	private ListDictionary queryValues;
	private int fetchLimit;
	private PropertyComparer[] sortOrderings;
	
	public MovieTemplate(IEntityMap anEntityMap)
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
	
	public System.String Title
	{
		get { return (System.String)queryValues["Title"]; }
		set { queryValues["Title"] = value; }
	}

	public System.Int32 Year
	{
		get { return (System.Int32)queryValues["Year"]; }
		set { queryValues["Year"] = value; }
	}

	public Person Director
	{
		get { return (Person)queryValues["Director"]; }
		set { queryValues["Director"] = value; }
	}

}

#endregion

#region MovieCollections

#region MovieList

//-------- Typed Collections ----------------------------------------------

public class MovieList : ObjectListBase
{
	public MovieList()
	{
	}

	public MovieList(IList list)
	{
		((ArrayList)InnerList).AddRange(list);
	}

	public Movie this[int index]
	{
		get { return (Movie)InnerList[index]; }
	}

	public int Add(Movie newMovie)
	{
		return base.Add(newMovie);
	}

	public void Remove(Movie existingMovie)
	{
		base.Remove(existingMovie);
	}

	public bool Contains(Movie existingMovie)
	{
		return base.Contains(existingMovie);
	}

	public int IndexOf(Movie existingMovie)
	{
		return base.IndexOf(existingMovie);
	}
	
	public MovieList Find(string qualifierFormat, params object[] parameters)
	{
		MovieList resultSet = new MovieList();
		base.Find(resultSet, qualifierFormat, parameters);
		return resultSet;
	}

	public new Movie FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (Movie)base.FindUnique(qualifierFormat, parameters);
	}
	
	public new Movie FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (Movie)base.FindFirst(qualifierFormat, parameters);
	}
	
}

#endregion 

#region MovieRelation

public class MovieRelation : ObjectRelationBase
{
	public MovieRelation(IEntityObject eo, string aRelation) : base(eo, aRelation)
	{
	}

	public Movie this[int index]
	{
		get { return (Movie)InnerList[index]; }
	}

	public int Add(Movie newMovie)
	{
		return base.Add(newMovie);
	}

	public void Remove(Movie existingMovie)
	{
		base.Remove(existingMovie);
	}

	public bool Contains(Movie existingMovie)
	{
		return base.Contains(existingMovie);
	}

	public int IndexOf(Movie existingMovie)
	{
		return base.IndexOf(existingMovie);
	}

	public MovieList GetReadOnlyList()
	{
		MovieList resultSet = new MovieList();
		base.CopyToListAndMakeReadOnly(resultSet);
		return resultSet;
	}
	
	public MovieList GetSortedList(string propName, SortDirection dir)
	{
		MovieList resultSet = new MovieList();
		base.CopyToListAndSort(resultSet, propName, dir);
		return resultSet;
	}

	public MovieList Find(string qualifierFormat, params object[] parameters)
	{
		MovieList resultSet = new MovieList();
		base.Find(resultSet, qualifierFormat, parameters);
		return resultSet;
	}

	public new Movie FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (Movie)base.FindUnique(qualifierFormat, parameters);
	}
	
	public new Movie FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (Movie)base.FindFirst(qualifierFormat, parameters);
	}

}

#endregion

#endregion

#region MovieFactory

public class MovieFactory : Neo.Framework.ObjectFactory
{
	public MovieFactory(ObjectContext context) : base(context, typeof(Movie))
	{
	}

	public Movie CreateObject()
	{
		return (Movie)base.CreateObject(null);
	}

	public Movie FindObject(System.Int32 arg0)
	{
		return (Movie)base.FindObject(new object[] { arg0 } );
	}

	public new MovieList FindAllObjects()
	{
		MovieList c = new MovieList();
		foreach(Movie eo in base.FindAllObjects())
			c.Add(eo);
		return c;
	}
	
	public MovieTemplate GetQueryTemplate()
	{
		return new MovieTemplate(EntityMap);
	}
	
	public MovieList Find(MovieTemplate template)
	{
		MovieList c = new MovieList();
		foreach(Movie eo in base.Find(template))
			c.Add(eo);
		return c;
	}

	public MovieList Find(FetchSpecification fetchSpecification)
	{
		MovieList c = new MovieList();
		foreach(Movie eo in base.Find(fetchSpecification))
			c.Add(eo);
		return c;
	}
	
	public new MovieList Find(string qualifierFormat, params object[] parameters)
	{
		MovieList c = new MovieList();
		foreach(Movie eo in base.Find(qualifierFormat, parameters))
			c.Add(eo);
		return c;
	}

	public new MovieList FindWithLimit(int limit, string qualifierFormat, params object[] parameters)
	{
		MovieList c = new MovieList();
		foreach(Movie eo in base.Find(qualifierFormat, parameters))
			c.Add(eo);
		return c;
	}
	
	public new Movie FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (Movie)base.FindFirst(qualifierFormat, parameters);
	}

	public new Movie FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (Movie)base.FindUnique(qualifierFormat, parameters);
	}	
	
}

#endregion

#region MovieEntityMap

internal class MovieMap : EntityMap
{
    private static readonly string[] pkcolumns = { "movie_id" };
    private static readonly string[] columns = { "movie_id", "title", "year", "director_id" };
    private static readonly string[] attributes = { "MovieId", "Title", "Year", "DirectorId" };
    private static readonly string[] relations = { "Director", "MovieActorLinks" };

    public override System.Type ObjectType
    {
        get { return typeof(Movie); }
    }
    
    public override string TableName
    {
        get { return "movies"; }
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
  		infos.Add("Director", new RelationInfo(Factory, typeof(Person), typeof(Movie), "person_id", "director_id"));
 		infos.Add("MovieActorLinks", new RelationInfo(Factory, typeof(Movie), typeof(MovieActorLink), "movie_id", "movie_id"));
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
		return new Movie(row, context);
	}
    
    protected override void WriteBasicSchema(DataTable table)
    {
		DataColumn		c;
		
		c = table.Columns.Add("movie_id", typeof(System.Int32));
		c.Unique = true;
		c.AutoIncrement = true; c.AutoIncrementSeed = c.AutoIncrementStep = -1;
		c = table.Columns.Add("title", typeof(System.String));
		c = table.Columns.Add("year", typeof(System.Int32));
		c.AllowDBNull = true;
		c = table.Columns.Add("director_id", typeof(System.Int32));
		table.PrimaryKey = new DataColumn[] { table.Columns["movie_id"] };
	}
	
	protected override void WriteRelations(DataTable table)
	{
		DataRelation r;
		
		if(table.DataSet.Relations["persons*movies.director_id"] == null)
		{
			r = table.DataSet.Relations.Add("persons*movies.director_id", 
					table.DataSet.Tables["persons"].Columns["person_id"],
					table.DataSet.Tables["movies"].Columns["director_id"]);
			r.ChildKeyConstraint.UpdateRule = Rule.None;
			r.ChildKeyConstraint.DeleteRule = Rule.None;
		}
 		if(table.DataSet.Relations["movies*movie_actor.movie_id"] == null)
		{
			r = table.DataSet.Relations.Add("movies*movie_actor.movie_id", 
					table.DataSet.Tables["movies"].Columns["movie_id"],
					table.DataSet.Tables["movie_actor"].Columns["movie_id"]);
			r.ChildKeyConstraint.UpdateRule = Rule.Cascade;
			r.ChildKeyConstraint.DeleteRule = Rule.Cascade;
		}
	}
}

#endregion

}

#endregion

#region MovieActorLink support classes

namespace Movies.Model
{
#region Using statements

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using Neo.Core;
using Neo.Core.Util;
using Neo.Framework;

#endregion

#region MovieActorLinkBase

public class MovieActorLinkBase : EntityObject
{
       
	protected MovieActorLinkBase(System.Data.DataRow aRow, Neo.Core.ObjectContext aContext) : base(aRow, aContext)
	{
	}	
	
	public virtual Movie Movie
	{
		get { object fk = Row["movie_id"]; return (fk == DBNull.Value) ? null : (Movie)GetRelatedObject("movies", fk); }
		set { SetRelatedObject(value, "movie_id", "movie_id" ); }
	}

	public virtual Person Actor
	{
		get { object fk = Row["actor_id"]; return (fk == DBNull.Value) ? null : (Person)GetRelatedObject("persons", fk); }
		set { SetRelatedObject(value, "actor_id", "person_id" ); }
	}

	public override object GetProperty(string propName)
	{
		if(propName == "Movie") 
			return Movie;
		if(propName == "Actor") 
			return Actor;

		return base.GetProperty(propName);
	}

}

#endregion

#region MovieActorLinkTemplate

public class MovieActorLinkTemplate : IFetchSpecification
{
	private IEntityMap entityMap;
	private ListDictionary queryValues;
	private int fetchLimit;
	private PropertyComparer[] sortOrderings;
	
	public MovieActorLinkTemplate(IEntityMap anEntityMap)
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
	
	public Movie Movie
	{
		get { return (Movie)queryValues["Movie"]; }
		set { queryValues["Movie"] = value; }
	}

	public Person Actor
	{
		get { return (Person)queryValues["Actor"]; }
		set { queryValues["Actor"] = value; }
	}

}

#endregion

#region MovieActorLinkCollections

#region MovieActorLinkList

//-------- Typed Collections ----------------------------------------------

public class MovieActorLinkList : ObjectListBase
{
	public MovieActorLinkList()
	{
	}

	public MovieActorLinkList(IList list)
	{
		((ArrayList)InnerList).AddRange(list);
	}

	public MovieActorLink this[int index]
	{
		get { return (MovieActorLink)InnerList[index]; }
	}

	public int Add(MovieActorLink newMovieActorLink)
	{
		return base.Add(newMovieActorLink);
	}

	public void Remove(MovieActorLink existingMovieActorLink)
	{
		base.Remove(existingMovieActorLink);
	}

	public bool Contains(MovieActorLink existingMovieActorLink)
	{
		return base.Contains(existingMovieActorLink);
	}

	public int IndexOf(MovieActorLink existingMovieActorLink)
	{
		return base.IndexOf(existingMovieActorLink);
	}
	
	public MovieActorLinkList Find(string qualifierFormat, params object[] parameters)
	{
		MovieActorLinkList resultSet = new MovieActorLinkList();
		base.Find(resultSet, qualifierFormat, parameters);
		return resultSet;
	}

	public new MovieActorLink FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (MovieActorLink)base.FindUnique(qualifierFormat, parameters);
	}
	
	public new MovieActorLink FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (MovieActorLink)base.FindFirst(qualifierFormat, parameters);
	}
	
}

#endregion 

#region MovieActorLinkRelation

public class MovieActorLinkRelation : ObjectRelationBase
{
	public MovieActorLinkRelation(IEntityObject eo, string aRelation) : base(eo, aRelation)
	{
	}

	public MovieActorLink this[int index]
	{
		get { return (MovieActorLink)InnerList[index]; }
	}

	public int Add(MovieActorLink newMovieActorLink)
	{
		return base.Add(newMovieActorLink);
	}

	public void Remove(MovieActorLink existingMovieActorLink)
	{
		base.Remove(existingMovieActorLink);
	}

	public bool Contains(MovieActorLink existingMovieActorLink)
	{
		return base.Contains(existingMovieActorLink);
	}

	public int IndexOf(MovieActorLink existingMovieActorLink)
	{
		return base.IndexOf(existingMovieActorLink);
	}

	public MovieActorLinkList GetReadOnlyList()
	{
		MovieActorLinkList resultSet = new MovieActorLinkList();
		base.CopyToListAndMakeReadOnly(resultSet);
		return resultSet;
	}
	
	public MovieActorLinkList GetSortedList(string propName, SortDirection dir)
	{
		MovieActorLinkList resultSet = new MovieActorLinkList();
		base.CopyToListAndSort(resultSet, propName, dir);
		return resultSet;
	}

	public MovieActorLinkList Find(string qualifierFormat, params object[] parameters)
	{
		MovieActorLinkList resultSet = new MovieActorLinkList();
		base.Find(resultSet, qualifierFormat, parameters);
		return resultSet;
	}

	public new MovieActorLink FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (MovieActorLink)base.FindUnique(qualifierFormat, parameters);
	}
	
	public new MovieActorLink FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (MovieActorLink)base.FindFirst(qualifierFormat, parameters);
	}

}

#endregion

#endregion

#region MovieActorLinkFactory

public class MovieActorLinkFactory : Neo.Framework.ObjectFactory
{
	public MovieActorLinkFactory(ObjectContext context) : base(context, typeof(MovieActorLink))
	{
	}

	public MovieActorLink CreateObject(Movie arg0, Person arg1)
	{
		return (MovieActorLink)base.CreateObject(new object[] { arg0.Row["movie_id"], arg1.Row["person_id"] } );
	}

	public MovieActorLink FindObject(Movie arg0, Person arg1)
	{
		return (MovieActorLink)base.FindObject(new object[] { arg0.Row["movie_id"], arg1.Row["person_id"] } );
	}

	public new MovieActorLinkList FindAllObjects()
	{
		MovieActorLinkList c = new MovieActorLinkList();
		foreach(MovieActorLink eo in base.FindAllObjects())
			c.Add(eo);
		return c;
	}
	
	public MovieActorLinkTemplate GetQueryTemplate()
	{
		return new MovieActorLinkTemplate(EntityMap);
	}
	
	public MovieActorLinkList Find(MovieActorLinkTemplate template)
	{
		MovieActorLinkList c = new MovieActorLinkList();
		foreach(MovieActorLink eo in base.Find(template))
			c.Add(eo);
		return c;
	}

	public MovieActorLinkList Find(FetchSpecification fetchSpecification)
	{
		MovieActorLinkList c = new MovieActorLinkList();
		foreach(MovieActorLink eo in base.Find(fetchSpecification))
			c.Add(eo);
		return c;
	}
	
	public new MovieActorLinkList Find(string qualifierFormat, params object[] parameters)
	{
		MovieActorLinkList c = new MovieActorLinkList();
		foreach(MovieActorLink eo in base.Find(qualifierFormat, parameters))
			c.Add(eo);
		return c;
	}

	public new MovieActorLinkList FindWithLimit(int limit, string qualifierFormat, params object[] parameters)
	{
		MovieActorLinkList c = new MovieActorLinkList();
		foreach(MovieActorLink eo in base.Find(qualifierFormat, parameters))
			c.Add(eo);
		return c;
	}
	
	public new MovieActorLink FindFirst(string qualifierFormat, params object[] parameters)
	{
		return (MovieActorLink)base.FindFirst(qualifierFormat, parameters);
	}

	public new MovieActorLink FindUnique(string qualifierFormat, params object[] parameters)
	{
		return (MovieActorLink)base.FindUnique(qualifierFormat, parameters);
	}	
	
}

#endregion

#region MovieActorLinkEntityMap

internal class MovieActorLinkMap : EntityMap
{
    private static readonly string[] pkcolumns = { "movie_id", "actor_id" };
    private static readonly string[] columns = { "movie_id", "actor_id" };
    private static readonly string[] attributes = { "MovieId", "ActorId" };
    private static readonly string[] relations = { "Movie", "Actor" };

    public override System.Type ObjectType
    {
        get { return typeof(MovieActorLink); }
    }
    
    public override string TableName
    {
        get { return "movie_actor"; }
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
  		infos.Add("Movie", new RelationInfo(Factory, typeof(Movie), typeof(MovieActorLink), "movie_id", "movie_id"));
 		infos.Add("Actor", new RelationInfo(Factory, typeof(Person), typeof(MovieActorLink), "person_id", "actor_id"));
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
		return new MovieActorLink(row, context);
	}
    
    protected override void WriteBasicSchema(DataTable table)
    {
		DataColumn		c;
		
		c = table.Columns.Add("movie_id", typeof(System.Int32));
		c = table.Columns.Add("actor_id", typeof(System.Int32));
		table.PrimaryKey = new DataColumn[] { table.Columns["movie_id"], table.Columns["actor_id"] };
	}
	
	protected override void WriteRelations(DataTable table)
	{
		DataRelation r;
		
		if(table.DataSet.Relations["movies*movie_actor.movie_id"] == null)
		{
			r = table.DataSet.Relations.Add("movies*movie_actor.movie_id", 
					table.DataSet.Tables["movies"].Columns["movie_id"],
					table.DataSet.Tables["movie_actor"].Columns["movie_id"]);
			r.ChildKeyConstraint.UpdateRule = Rule.Cascade;
			r.ChildKeyConstraint.DeleteRule = Rule.Cascade;
		}
		if(table.DataSet.Relations["persons*movie_actor.actor_id"] == null)
		{
			r = table.DataSet.Relations.Add("persons*movie_actor.actor_id", 
					table.DataSet.Tables["persons"].Columns["person_id"],
					table.DataSet.Tables["movie_actor"].Columns["actor_id"]);
			r.ChildKeyConstraint.UpdateRule = Rule.Cascade;
			r.ChildKeyConstraint.DeleteRule = Rule.Cascade;
		}
	}
}

#endregion

}

#endregion

