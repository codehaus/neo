//------------------------------------------------------------------------
// Generated by Neo on 17/08/2004 18:03:36 for THOUGHTWORKS\edoernenburg
//
// This file was autogenerated but you can (and are meant to) edit it as 
// it will not be overwritten unless explicitly requested.
//------------------------------------------------------------------------

using System;
using System.Data;
using Neo.Core;


namespace Movies.Model
{

public class Movie : MovieBase
{
	protected internal Movie(DataRow aRow, ObjectContext aContext) : 
		       base(aRow, aContext)
	{
	}
		        

	protected override object HandleNullValueForProperty(string propName)
	{
		if(propName == "Year")
			return 0;
		return base.HandleNullValueForProperty(propName);
	}


	public void AddActor(Person person)
	{
		new MovieActorLinkFactory(this.Context).CreateObject(this, person);
	}

	public PersonList Actors
	{
		get 
		{
			PersonList result = new PersonList();
			foreach(MovieActorLink l in MovieActorLinks)
				result.Add(l.Actor);
			return result;
		}
	}

}

}

 	
