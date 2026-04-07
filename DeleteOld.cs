using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

namespace CodeBureau.DeleteOld
{
	/// <summary>
	/// Summary description for DeleteOld.
	/// </summary>
	public class DeleteOld
	{

		#region Private Variables

		Arguments	_arguments;
		bool		_validated			= false;
		bool		_recurseSubFolders	= false;
		bool		_removeEmptyFolders = false;
		bool		_showHelp			= false;
		string		_path				= Environment.CurrentDirectory;
		string		_filter				= "*.*";
		int			_age				= int.MaxValue;
		string		_timeFrame			= AgeIncrement.Day;
		bool		_quietMode			= false;
		bool		_simulateOnly		= false;
		DateTime	_deleteOlderThan;
		DateTime	_runTime;
		StreamWriter	_outputStream;
		StreamWriter	_errorOutputStream;

		#endregion

		#region Constructors

		public DeleteOld(Arguments args)
		{
			_outputStream = new StreamWriter(Console.OpenStandardOutput());
			_outputStream.AutoFlush = true;
			_errorOutputStream = new StreamWriter(Console.OpenStandardError());
			_errorOutputStream.AutoFlush = true;

			_arguments = args;
			if (!ParseArguments() || _showHelp) 
			{
				ShowUsage();
				_validated = false;
			}
			else 
				_validated = true;
				
		}

		public DeleteOld(Arguments args, Stream outputStream, Stream errorOutputStream)
		{
			_outputStream = new StreamWriter(outputStream);
			_outputStream.AutoFlush = true;
			_errorOutputStream = new StreamWriter(errorOutputStream);
			_errorOutputStream.AutoFlush = true;

			_arguments = args;
			if (!ParseArguments() || _showHelp) 
			{
				ShowUsage();
				_validated = false;
			}
			else 
				_validated = true;

		}

		#endregion

		#region Public Static Methods 

		/// <summary>
		/// Program entry point
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public static int Main(string[] args) 
		{
			DeleteOld app = new DeleteOld(new Arguments(args));
			if (app._validated) 
			{
				return app.ProcessCommand();
			}
			else
				return -1;
		}

		/// <summary>
		/// Get's a usage string for the application.
		/// </summary>
		/// <returns></returns>
		public static string Usage() 
		{
			
			StringBuilder sb = new StringBuilder();
			sb.Append("\n");
			sb.Append("Deletes files older than a specified age\n\n");
			sb.Append("DeleteOld [-p] [-f] -a [-t] [--s] [--r] [--q] [--n]\n\n");
			sb.Append("-p\tSpecifies path to search (enclose long paths in \"\").  Default is current directory.\n");
			sb.Append("-f\tSpecifies filter to search.  Default is *.*\n");
			sb.Append("-a\tYoungest age of files to delete.  Used in conjunction with TimeFrame.\n");
			sb.Append("-t\tTimeFrame.  One of the following:\n");
			sb.Append("\t\ts = Second\tn = Minute\th = Hour\n");
			sb.Append("\t\td = day\t\tm = Month\ty = Year\n");
			sb.Append("\tDefault is d (Day).\n");
			sb.Append("--s\tRecurse Sub Folders.  Default is Off.\n");
			sb.Append("--r\tRemove empty folders.  Default is Off.\n");
			sb.Append("--q\tQuiet Mode (this suppresses any output).\n");
			sb.Append("--n\tDo nothing - simulate only what 'would occur'\n\n");
			sb.Append("e.g. The following deletes all files matching *.dat in c:\\temp older than (last modified) 10 days ago (based on current Date and Time).\n");
			sb.Append("It also recurses and removes any empty folders within the tree and pipes output to a text file.\n\n");
			sb.Append("DeleteOld -p \"C:\\temp\\LoanNET\" -f *.dat -a 10  -t d --s --r >c:\\out.txt\n\n");
			
			return sb.ToString();

		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Parse input arguments for completeness
		/// </summary>
		/// <returns></returns>
		private bool ParseArguments() 
		{

			//Help
			if (_arguments[ArgumentNames.Help] == bool.TrueString) 
			{
				_showHelp = true;
				return false;
			}

			//Quiet
			if (_arguments[ArgumentNames.Quiet] == bool.TrueString)
				_quietMode = true;

			//Simulate Only
			if (_arguments[ArgumentNames.Nothing] == bool.TrueString)
				_simulateOnly = true;

			//Mandatory Parameters
			//Age
			if (_arguments[ArgumentNames.Age] != null) 
			{
				try 
				{
					_age = int.Parse(_arguments[ArgumentNames.Age]); 
					if (_age < 0 || _age == int.MaxValue)
						return false;
				}
				catch (Exception) 
				{
					if (! _quietMode)
						_errorOutputStream.WriteLine("Age was invalid");
						//Console.Error.WriteLine();
					return false;
				}
			}
			else 
			{
				if (! _quietMode)
					_errorOutputStream.WriteLine("Age must be supplied");
				return false;
			}

			//Optional Parameters
			//Path
			if (_arguments[ArgumentNames.Path] != null)
				_path = _arguments[ArgumentNames.Path];

			if (!Directory.Exists(_path)) 
			{
				_errorOutputStream.WriteLine("Directory does not exist : {0}", _path);
				return false;
			}

			//Filter
			if (_arguments[ArgumentNames.Filter] != null)
				_filter = _arguments[ArgumentNames.Filter];

			//TimeFrame
			if (_arguments[ArgumentNames.AgeIncrement] != null) 
			{
				if (Regex.IsMatch(_arguments[ArgumentNames.AgeIncrement], @"[s|n|d|m|y]{1}"))				
					_timeFrame = _arguments[ArgumentNames.AgeIncrement];
				else 
				{
					if (! _quietMode)
						_errorOutputStream.WriteLine("Time Frame was invalid");
					return false;
				}
			}

			//Recurse SubFolders
			if (_arguments[ArgumentNames.Recurse] == bool.TrueString)
				_recurseSubFolders = true;

			//Remove Empty Folders
			if (_arguments[ArgumentNames.RemoveEmptyFolders] == bool.TrueString)
				_removeEmptyFolders = true;

			//Check for 'any' invalid arguments
			foreach(string arg in _arguments.InnerStringDictionary.Keys) 
			{
				if (! Regex.IsMatch(arg, String.Format("[{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}]", ArgumentNames.Path,
					ArgumentNames.Age,
					ArgumentNames.AgeIncrement,
					ArgumentNames.Filter,
					ArgumentNames.Help,
					ArgumentNames.Nothing,
					ArgumentNames.Quiet,
					ArgumentNames.Recurse,
					ArgumentNames.RemoveEmptyFolders)))
				{
					_errorOutputStream.WriteLine("Invalid argument specified : {0}", arg);
					return false;
				}

			}

			return true;
		}

		/// <summary>
		/// Sets the delete time benchmark.
		/// </summary>
		private void SetDeleteTimeBenchmark() 
		{
			if (_timeFrame == AgeIncrement.Second) 
				_deleteOlderThan = _runTime.AddSeconds(- _age);				
			if (_timeFrame == AgeIncrement.Minute)
				_deleteOlderThan = _runTime.AddMinutes(- _age);				
			if (_timeFrame == AgeIncrement.Hour)
				_deleteOlderThan = _runTime.AddHours(- _age);				
			if (_timeFrame == AgeIncrement.Day)
				_deleteOlderThan = _runTime.AddDays(- _age);				
			if (_timeFrame == AgeIncrement.Month)
				_deleteOlderThan = _runTime.AddMonths(- _age);				
			if (_timeFrame == AgeIncrement.Year)
				_deleteOlderThan = _runTime.AddYears(- _age);				

		}

		/// <summary>
		/// Deletes the files (potentially recursively) from given folder.
		/// </summary>
		/// <param name="folder">Folder.</param>
		/// <returns></returns>
		private int DeleteFilesFromFolder(DirectoryInfo folder, bool simulateOnly)
		{
			foreach(FileInfo file in folder.GetFiles(_filter))
			{
				if (file.LastWriteTime < _deleteOlderThan) 
				{
					try 
					{
						if (! _quietMode)
							_outputStream.WriteLine("Deleting {0}", file.FullName);
						if (! _simulateOnly)
							file.Delete();
					}
					catch (Exception ex)
					{
						if (! _quietMode)
							_errorOutputStream.WriteLine("Could not delete file {0} : Reason {1}", file.FullName, ex.Message);
					}
				}
			}

			if (_recurseSubFolders) 
			{
				foreach(DirectoryInfo subFolder in folder.GetDirectories()) 
				{
					DeleteFilesFromFolder(subFolder, simulateOnly);
				}
			}

			if (_removeEmptyFolders) 
			{
				if (folder.GetFiles().Length == 0 && folder.GetDirectories().Length == 0) 
				{
					if (! _quietMode)
						_outputStream.WriteLine("Deleting empty folder {0}", folder.FullName);

					try 
					{
						if (! _simulateOnly)
							folder.Delete(true);
					}
					catch (Exception ex) 
					{
						if (! _quietMode)
							_errorOutputStream.WriteLine("Could not delete folder {0} : Reason {1}", folder.FullName, ex.Message);
					}
				}
			}

			return 0;
		}
											
		/// <summary>
		/// Shows the usage string.
		/// </summary>
		private void ShowUsage() 
		{
			_outputStream.Write(Usage());
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Process the console command
		/// </summary>
		/// <returns></returns>
		public int ProcessCommand() 
		{
			if (Directory.Exists(_path)) 
			{
				_runTime = DateTime.Now;
				SetDeleteTimeBenchmark();
				if (! _quietMode) 
				{
					if (_simulateOnly) 
						_outputStream.WriteLine("SIMULATION ONLY:\nDeleting {0} from {1} older than {2}", _filter, _path, _deleteOlderThan.ToString("dd/MM/yyyy hh:mm:ss"));
					else
						_outputStream.WriteLine("Deleting {0} from {1} older than {2}", _filter, _path, _deleteOlderThan.ToString("dd/MM/yyyy hh:mm:ss"));
				}
				return DeleteFilesFromFolder(new DirectoryInfo(_path), _simulateOnly);
			}
			else
				_errorOutputStream.WriteLine("Directory does not exist : {0}", _path);
				ShowUsage();
				return -1;

		}

		#endregion

		#region Public Attributes

		/// <summary>
		/// Return whether the parameters validated OK
		/// </summary>
		public bool Validated 
		{
			get {return _validated;}
		}

		#endregion

	}

	#region Constants

	/// <summary>
	/// Argument Names constants
	/// </summary>
	public class ArgumentNames
	{
		private ArgumentNames() {}

		public static readonly string Path = "p";
		public static readonly string Recurse = "s";
		public static readonly string Help = "h";
		public static readonly string RemoveEmptyFolders = "r";
		public static readonly string Age = "a";
		public static readonly string AgeIncrement = "t";
		public static readonly string Filter = "f";
		public static readonly string Quiet = "q";
		public static readonly string Nothing = "n";

	}

	/// <summary>
	/// Age Increment (TimeFrame) constants
	/// </summary>
	public class AgeIncrement
	{
		private AgeIncrement() {}

		public static readonly string Second = "s";
		public static readonly string Minute = "n";
		public static readonly string Hour = "h";
		public static readonly string Day = "d";
		public static readonly string Month = "m";
		public static readonly string Year = "y";

	}

	#endregion

}
