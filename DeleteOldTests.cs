using System;
using NUnit.Framework;
using System.Text;
using System.IO;

namespace CodeBureau.DeleteOld.Tests
{

	#region Class DeleteOldTests

	/// <summary>
	/// Summary description for DeleteOldTests.
	/// </summary>
	[TestFixture]
	public class DeleteOldTests 
	{
		#region Private Fields

		#endregion
        
		#region Constructor
        
		public DeleteOldTests()
		{
			//TODO: Enter Test Fixture construction implementation
		}
        
		#endregion
        
		#region Setup / TearDown
        
		[SetUp]
		public void Setup()
		{
			SetupTempFileTree();
		}

		[TearDown]
		public void TearDown() 
		{
		
			RemoveTempFileTree();
		}	

		/// <summary>
		/// Set up a temporary file tree for testing
		/// </summary>
		private void SetupTempFileTree() 
		{
			StreamWriter sw;
			string path = Path.Combine(Path.GetTempPath(), "DeleteOldtests") ;
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			for (int i = 0; i < 50; i++) 
			{
				string fileName = Path.Combine(path, string.Format("file{0}.xml", i));
				sw = File.CreateText(fileName);
				sw.Close();
				if (i > 39)
					File.SetLastWriteTime(fileName, DateTime.Now.AddDays(-10).AddMinutes(-2));
			}

			for (int i = 0; i < 30; i++) 
			{
				string fileName = Path.Combine(path, string.Format("file{0}.txt", i));
				sw = File.CreateText(fileName);
				sw.Close();
			}

			string subPath = Path.Combine(path, "subfolder");
			if (!Directory.Exists(subPath))
				Directory.CreateDirectory(subPath);

			for (int i = 0; i < 20; i++) 
			{
				string fileName = Path.Combine(subPath, string.Format("file{0}.xsd", i));
				sw = File.CreateText(fileName);
				sw.Close();
				if (i > 13)
					File.SetLastWriteTime(fileName, DateTime.Now.AddDays(-10).AddMinutes(-1));
			}

		}

		private void RemoveTempFileTree() 
		{

			string path = Path.Combine(Path.GetTempPath(), "DeleteOldtests") ;
			foreach(FileInfo file in new DirectoryInfo(path).GetFiles())
			{
				file.Delete();
			}
			string subPath = Path.Combine(path, "subfolder");
			if (Directory.Exists(subPath))
				Directory.Delete(subPath, true);


		}

		#endregion

		#region Helper methods for arguments

		private string[] GetHelpArguments() 
		{
			return new string[] {"--h"};
		}

		private string[] GetCurrentFolder0SecondArguments() 
		{
			return new string[] {"-a", "0", "-t", "s"};
		}

		private string[] GetCurrentFolder0SecondFilterArguments() 
		{
			return new string[] {"-a", "0", "-t", "s", "-f", "*.x*"};
		}

		private string[] GetCurrentFolder10DayFilterRecursiveArguments() 
		{
			return new string[] {"-a", "10", "-t", "d", "-f", "*.x*", "--s"};
		}

		private string[] GetCurrentFolder0SecondRecursiveRemoveEmptyArguments() 
		{
			return new string[] {"-a", "0", "-t", "s", "--s", "--r"};
		}

		private string[] GetSimulateCurrentFolder0SecondRecursiveRemoveEmptyArguments() 
		{
			return new string[] {"-a", "0", "-t", "s", "--s", "--r", "--n"};
		}

		private string[] GetInvalidArguments1() 
		{
			return new string[] {"-a", "10", "-t", "d", "-f", "*.x*", "--s", "--r", "--z"};
		}

		private string[] GetInvalidArguments2() 
		{
			return new string[] {"-a", "10", "-t", "d", "-f", "*.x*", "--s", "--r", "-p", @"c:\vfchtugierujkjhsdfsdf\ffklksdfs\jlkjkvvdd1"};
		}

		private string[] GetQuietArguments() 
		{
			return new string[] {"-a", "10", "--q"};
		}
		#endregion

		#region Tests

		/// <summary>
		/// Gets the usage.
		/// </summary>
		[Test]
		public void GetUsage() 
		{
			Assert.IsNotNull(DeleteOld.Usage());	
		}

		/// <summary>
		/// Gets the help usage from custom stream and ensures is consistent with that
		/// sent to client.
		/// </summary>
		[Test]
		public void GetHelpUsage() 
		{
			string[] args = GetHelpArguments();
			MemoryStream ms = new MemoryStream();
			Arguments arguments = new Arguments(args);
			DeleteOld DeleteOld = new DeleteOld(arguments, ms, ms);
			Assert.IsFalse(DeleteOld.Validated);
			string output = System.Text.Encoding.Default.GetString(ms.ToArray());

			Assert.AreEqual(DeleteOld.Usage(), output);
		}

		[Test]
		public void Delete_Files_CurrentFolder_Only_RegardlessOfAge() 
		{

			Environment.CurrentDirectory = Path.Combine(Path.GetTempPath(), "DeleteOldtests");
			string[] args = GetCurrentFolder0SecondArguments();
			MemoryStream ms = new MemoryStream();
			Arguments arguments = new Arguments(args);
			DeleteOld DeleteOld = new DeleteOld(arguments, ms, ms);
			Assert.IsTrue(DeleteOld.Validated);
			DeleteOld.ProcessCommand();
			string output = System.Text.Encoding.Default.GetString(ms.ToArray());

			//Ensure this folder is empty but exists
			string path = Path.Combine(Path.GetTempPath(), "DeleteOldtests");
			Assert.IsTrue(Directory.Exists(path));
			Assert.IsTrue(Directory.GetFiles(path).Length == 0);

			//Ensure sub folder has not been deleted and 20 files exist
			string subPath = Path.Combine(path, "subfolder");
			Assert.IsTrue(Directory.Exists(subPath));
			Assert.IsTrue(Directory.GetFiles(subPath).Length == 20);
			

		}

		/// <summary>
		/// Refer to SetupTempFileTree() for scenario
		/// Delete filtered content in the current folder (only) regardless of age
		/// </summary>
		[Test]
		public void Delete_Filtered_CurrentFolder_Only_RegardlessOfAge() 
		{

			Environment.CurrentDirectory = Path.Combine(Path.GetTempPath(), "DeleteOldtests");
			string[] args = GetCurrentFolder0SecondFilterArguments();
			MemoryStream ms = new MemoryStream();
			Arguments arguments = new Arguments(args);
			DeleteOld DeleteOld = new DeleteOld(arguments, ms, ms);
			Assert.IsTrue(DeleteOld.Validated);
			DeleteOld.ProcessCommand();
			string output = System.Text.Encoding.Default.GetString(ms.ToArray());

			//Ensure this folder is empty but exists
			string path = Path.Combine(Path.GetTempPath(), "DeleteOldtests");
			Assert.IsTrue(Directory.Exists(path));
			Assert.IsTrue(Directory.GetFiles(path).Length == 30);

			//Ensure sub folder has not been deleted and 20 files exist
			string subPath = Path.Combine(path, "subfolder");
			Assert.IsTrue(Directory.Exists(subPath));
			Assert.IsTrue(Directory.GetFiles(subPath).Length == 20);

		}

		/// <summary>
		/// Refer to SetupTempFileTree() for scenario
		/// Delete filtered content in the current folder recursively with an age of 10 days or older
		/// </summary>
		[Test]
		public void Delete_Filtered_CurrentFolder_Recursive_10DaysOld() 
		{

			Environment.CurrentDirectory = Path.Combine(Path.GetTempPath(), "DeleteOldtests");
			string[] args = GetCurrentFolder10DayFilterRecursiveArguments();
			MemoryStream ms = new MemoryStream();
			Arguments arguments = new Arguments(args);
			DeleteOld DeleteOld = new DeleteOld(arguments, ms, ms);
			Assert.IsTrue(DeleteOld.Validated);
			DeleteOld.ProcessCommand();
			string output = System.Text.Encoding.Default.GetString(ms.ToArray());

			//Ensure this folder is empty but exists
			string path = Path.Combine(Path.GetTempPath(), "DeleteOldtests");
			Assert.IsTrue(Directory.Exists(path));
			Assert.IsTrue(Directory.GetFiles(path).Length == 70);

			//Ensure sub folder has not been deleted and 6 files exist
			string subPath = Path.Combine(path, "subfolder");
			Assert.IsTrue(Directory.Exists(subPath));
			Assert.IsTrue(Directory.GetFiles(subPath).Length == 14, "Expected 14 files to remain");

		}

		/// <summary>
		/// Refer to SetupTempFileTree() for scenario
		/// Delete items in the current folder (recursively) regardless of age and remove empty
		/// folders (within the tree)
		/// </summary>
		[Test]
		public void Delete_CurrentFolder_Recursive_RemoveEmpty_RegardlessOfAge() 
		{

			Environment.CurrentDirectory = Path.Combine(Path.GetTempPath(), "DeleteOldtests");
			string[] args = GetCurrentFolder0SecondRecursiveRemoveEmptyArguments();
			MemoryStream ms = new MemoryStream();
			Arguments arguments = new Arguments(args);
			DeleteOld DeleteOld = new DeleteOld(arguments, ms, ms);
			Assert.IsTrue(DeleteOld.Validated);
			DeleteOld.ProcessCommand();
			string output = System.Text.Encoding.Default.GetString(ms.ToArray());

			//Ensure this folder is empty but exists
			string path = Path.Combine(Path.GetTempPath(), "DeleteOldtests");
			Assert.IsTrue(Directory.Exists(path));
			Assert.IsTrue(Directory.GetFiles(path).Length == 0);

			//Ensure sub folder has not been deleted and 6 files exist
			string subPath = Path.Combine(path, "subfolder");
			Assert.IsFalse(Directory.Exists(subPath));

		}

		/// <summary>
		/// Inject an invalid argument into the command (amongst other valid arguments).  
		/// This should fail validation and return an expected message.
		/// </summary>
		[Test]
		public void InjectInvalidArgument() 
		{

			Environment.CurrentDirectory = Path.Combine(Path.GetTempPath(), "DeleteOldtests");
			string[] args = GetInvalidArguments1();
			MemoryStream ms = new MemoryStream();
			Arguments arguments = new Arguments(args);
			DeleteOld DeleteOld = new DeleteOld(arguments, ms, ms);
			Assert.IsFalse(DeleteOld.Validated);
			string output = System.Text.Encoding.Default.GetString(ms.ToArray());

			//Ensure output is correct
			Assert.IsTrue(output.StartsWith("Invalid argument specified : z"));

		}

		[Test]
		public void DirectoryNotExist() 
		{

			Environment.CurrentDirectory = Path.Combine(Path.GetTempPath(), "DeleteOldtests");
			string[] args = GetInvalidArguments2();
			MemoryStream ms = new MemoryStream();
			Arguments arguments = new Arguments(args);

			DeleteOld DeleteOld = new DeleteOld(arguments, ms, ms);
			Assert.IsFalse(DeleteOld.Validated);
			string output = System.Text.Encoding.Default.GetString(ms.ToArray());

			//Ensure output is correct
			Assert.IsTrue(output.StartsWith(@"Directory does not exist : c:\vfchtugierujkjhsdfsdf\ffklksdfs\jlkjkvvdd1"));

		}

		/// <summary>
		/// Refer to SetupTempFileTree() for scenario
		/// Delete items in the current folder (recursively) regardless of age and remove empty
		/// folders (within the tree)
		/// </summary>
		[Test]
		public void Simulate_Delete_CurrentFolder_Recursive_RemoveEmpty_RegardlessOfAge() 
		{

			Environment.CurrentDirectory = Path.Combine(Path.GetTempPath(), "DeleteOldtests");
			string[] args = GetSimulateCurrentFolder0SecondRecursiveRemoveEmptyArguments();
			MemoryStream ms = new MemoryStream();
			Arguments arguments = new Arguments(args);
			DeleteOld DeleteOld = new DeleteOld(arguments, ms, ms);
			Assert.IsTrue(DeleteOld.Validated);
			DeleteOld.ProcessCommand();
			string output = System.Text.Encoding.Default.GetString(ms.ToArray());

			//Ensure this folder exists with all 80 files
			string path = Path.Combine(Path.GetTempPath(), "DeleteOldtests");
			Assert.IsTrue(Directory.Exists(path));
			Assert.IsTrue(Directory.GetFiles(path).Length == 80);

			//Ensure sub folder has not been deleted and all 20 files exist
			string subPath = Path.Combine(path, "subfolder");
			Assert.IsTrue(Directory.Exists(subPath));
			Assert.IsTrue(Directory.GetFiles(subPath).Length == 20);

		}

		[Test]
		public void RunQuiet() 
		{

			Environment.CurrentDirectory = Path.Combine(Path.GetTempPath(), "DeleteOldtests");
			string[] args = GetQuietArguments();
			MemoryStream ms = new MemoryStream();
			Arguments arguments = new Arguments(args);
			DeleteOld DeleteOld = new DeleteOld(arguments, ms, ms);
			Assert.IsTrue(DeleteOld.Validated);
			DeleteOld.ProcessCommand();
			string output = System.Text.Encoding.Default.GetString(ms.ToArray());

			Assert.IsTrue(output.Length == 0);


		}
		#endregion
        
	}
    
	#endregion

}
