using System;
using System.Data;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Management;
using Neo.Core;


namespace Neo.Framework
{
	/*  Some handy classes to generate primary keys. CodeGen/Norque uses these. */
	

	//--------------------------------------------------------------------------------------
	//	User Primary Keys (User provides key)
	//--------------------------------------------------------------------------------------

	public class UserPkInitializer : IPkInitializer
	{
		public void InitializeRow(DataRow row, object argument)
		{
			DataColumn[] pkcolumns;
			object[]	 pkvalues;
			
			if(argument == null)
				throw new ArgumentException(String.Format("Entity uses user PK but no values were provided."));
			pkcolumns = row.Table.PrimaryKey;
			pkvalues = (object[])argument;
			if(pkcolumns.Length != pkvalues.Length)
				throw new ArgumentException(String.Format("Invalid PK; expected values for {0} columns, got {1} values", pkcolumns.Length, pkvalues.Length));
			for(int i = 0; i < pkvalues.Length; i++)
				row[pkcolumns[i]] = pkvalues[i];
		}
	}


	//--------------------------------------------------------------------------------------
	//	Native Primary Keys (Database generates them)
	//--------------------------------------------------------------------------------------

	public class NativePkInitializer : IPkInitializer
	{
		public void InitializeRow(DataRow row, object argument)
		{
			// Nothing to do. We use autoincrement columns to do the work.
		}
	}
	

	//--------------------------------------------------------------------------------------
	//	Primary Keys provided by Broker Service (Sequence table)
	//--------------------------------------------------------------------------------------
	
	public class IdBrokerPkInitializer : IPkInitializer
	{
		public void InitializeRow(DataRow row, object argument)
		{
			throw new InvalidOperationException("IdBroker not implemented yet.");		
		}
	}


	//--------------------------------------------------------------------------------------
	//	Globally Unique Primary Keys (created by the client, .NET version)
	//--------------------------------------------------------------------------------------

	public class NewGuidPkInitializer : IPkInitializer
	{
		public void InitializeRow(DataRow row, object argument)
		{
			DataColumn[] pkcolumns;

			pkcolumns = row.Table.PrimaryKey;
			if((pkcolumns.Length != 1) || (pkcolumns[0].DataType != typeof(Guid)))
				throw new InvalidOperationException("Invalid PK for GUID type.");
			row[pkcolumns[0]] = Guid.NewGuid();
		}
	}


	//--------------------------------------------------------------------------------------
	//	Globally Unique Primary Keys (created by the client, custom version)
	//--------------------------------------------------------------------------------------

	// Currently uses 16 bytes as follows: seconds since 1-Jan-2000 (4 bytes), rolling 
	// sequence (2 bytes), process id (2 bytes), lower half of ip address in network 
	// order (2 bytes), mac address (6 bytes)

	//   0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15
	//   T  T  T  T  S  S  P  P  I  I  M  M  M  M  M  M 
 
	public class GuidPkInitializer : IPkInitializer
	{
		private static long	sequenceNumber = 0;
		private static byte[] guidbase;
		
		public void InitializeRow(DataRow row, object argument)
		{
			DataColumn[] pkcolumns;

			pkcolumns = row.Table.PrimaryKey;
			if((pkcolumns.Length != 1) || (pkcolumns[0].DataType != typeof(Guid)))
				throw new InvalidOperationException("Invalid PK for GUID type.");
			row[pkcolumns[0]] = new Guid(getGuid());

		}


		public static byte[] getGuid()
		{
			byte[] guid = new byte[16];

			if(guidbase == null)
			{
				guidbase = new byte[16];
				writeGuidBase(guidbase);
			}
			guidbase.CopyTo(guid, 0);
			writeGuidDyn(guid);
			return guid;
		}


		private static void writeGuidBase(byte[] bytes)
		{
			bool foundMacAddress = false;

			string qstring = "SELECT * FROM Win32_NetworkAdapterConfiguration where IPEnabled = true";
			foreach(ManagementObject mo in new ManagementObjectSearcher(qstring).Get())
			{
				int idx = 6; // first two bytes will be overwritten, that's intentional
				string ipaddr = ((string[])mo["IPAddress"])[0];
				if((ipaddr != null) && (ipaddr != String.Empty))
				{
					foreach(string group in ipaddr.Split('.'))
						bytes[idx++] = (byte)Convert.ToInt16(group, 10);
				}
				string macaddr = ((string)mo["MacAddress"]);
				if((macaddr != null) || (macaddr != String.Empty))
				{
					foreach(string group in macaddr.Split(':'))
						bytes[idx++] = (byte)Convert.ToInt16(group, 16);
					foundMacAddress = true;
					break;
				}
			}
			if(foundMacAddress == false)
				throw new InvalidOperationException("Failed to generate a GUID; cannot not find a Network Adapter with a valid MAC address.");

			int pid = Process.GetCurrentProcess().Id;
			if(pid > 0xFFFF)
				throw new InvalidOperationException("Failed to generate a GUID; process id larger than 2^16.");
			bytes[ 6] = (byte) (pid & 0x00FF);
			bytes[ 7] = (byte)((pid & 0xFF00) >> 8);
		}


		private static void writeGuidDyn(byte[] bytes)
		{
			// a bit lame but 2^64 will surely not be reached........
			Interlocked.Increment(ref sequenceNumber); 
			bytes[ 4] = (byte) (sequenceNumber & 0x00FF);
			bytes[ 5] = (byte)((sequenceNumber & 0xFF00) >> 8);

			int secs = (int)((DateTime.Now - new DateTime(2000, 1, 1)).TotalSeconds);
			bytes[ 0] = (byte)((secs & 0xFF000000) >> 24);
			bytes[ 1] = (byte)((secs & 0x00FF0000) >> 16);
			bytes[ 2] = (byte)((secs & 0x0000FF00) >> 8);
			bytes[ 3] = (byte) (secs & 0x000000FF);
		}

	}

}
