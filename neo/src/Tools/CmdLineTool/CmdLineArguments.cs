/*
 * Based on Arguments class described at:
 *
 * http://www.codeproject.com/csharp/Command_Line.asp
 */


using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace Neo.CmdLineTool {
	/// <summary>
	/// Arguments class
	/// </summary>
	public class CmdLineArguments{
		// Variables
		private StringDictionary Parameters;
		private ArrayList		 filenames;

		// Constructor
		public CmdLineArguments(string[] Args){
			Parameters=new StringDictionary();
			Regex Spliter=new Regex(@"^-{1,2}|^/|=|:",RegexOptions.IgnoreCase|RegexOptions.Compiled);
			Regex Remover= new Regex(@"^['""]?(.*?)['""]?$",RegexOptions.IgnoreCase|RegexOptions.Compiled);
			string Parameter=null;
			string[] Parts;
			bool readingFilenames = false;

			// Valid parameters forms:
			// {-,/,--}param{ ,=,:}((",')value(",'))
			// Examples: -param1 value1 --param2 /param3:"Test-:-work" /param4=happy -param5 '--=nice=--'
			filenames = new ArrayList();
			foreach(string Txt in Args){
				if(readingFilenames){
					filenames.Add(Txt);
					continue;
					}
				// Look for new parameters (-,/ or --) and a possible enclosed value (=,:)
				Parts=Spliter.Split(Txt,3);
				switch(Parts.Length){
					// Found a value (for the last parameter found (space separator))
					case 1:
						if(Parameter!=null){
							if(!Parameters.ContainsKey(Parameter)){
								Parts[0]=Remover.Replace(Parts[0],"$1");
								Parameters.Add(Parameter,Parts[0]);
								}
							Parameter=null;
							}
						else{
						    // No parameter waiting, we must be hitting the filenames...
		  				    filenames.Add(Txt);
						    readingFilenames = true;
						    }
						break;
					// Found just a parameter
					case 2:
						// The last parameter is still waiting. With no value, set it to true.
						if(Parameter!=null){
							if(!Parameters.ContainsKey(Parameter)) Parameters.Add(Parameter,"true");
							}
						Parameter=Parts[1];
						break;
					// Parameter with enclosed value
					case 3:
						// The last parameter is still waiting. With no value, set it to true.
						if(Parameter!=null){
							if(!Parameters.ContainsKey(Parameter)) Parameters.Add(Parameter,"true");
							}
						Parameter=Parts[1];
						// Remove possible enclosing characters (",')
						if(!Parameters.ContainsKey(Parameter)){
							Parts[2]=Remover.Replace(Parts[2],"$1");
							Parameters.Add(Parameter,Parts[2]);
							}
						Parameter=null;
						break;
					}
				}
			// In case a parameter is still waiting
			if(Parameter!=null){
				if(!Parameters.ContainsKey(Parameter)) Parameters.Add(Parameter,"true");
				}
			}

		// Retrieve a parameter value if it exists
		public string this [string Param]{
			get{
				return(Parameters[Param]);
				}
			}

		// Retrieve the list of filenames
		public ArrayList Filenames{
			get{
				return(filenames);
			    }
		    }
		}
	}
