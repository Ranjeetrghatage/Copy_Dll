using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;  // Start a new Process important code

class Program
{
    static void Main(string[] args)
    {
        if (!IsRunAsAdmin())
        {
            // Start the application with elevated permissions
            ProcessStartInfo proc = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Assembly.GetExecutingAssembly().Location,
                Verb = "runas",
            };

            try
            {
                Process.Start(proc);  // creating a new instance hence remember if anything is needed to the code....
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to restart as admin: {ex.Message}");
            }
        }
        else
        {
            PerformFileOperations();
        }

    }

    static bool IsRunAsAdmin()
    {
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    static void PerformFileOperations()
    {
        List<string> folders = new List<string> { "System32", "SysWOW64" };
        string[] dllNames = { "mahindraSeedKey.dll", "mahindraSeedKey_1_1.dll", "mahindraSeedKey_V1_1.dll" };

        var drives = DriveInfo.GetDrives().Where(d => d.IsReady).ToList();

        foreach (var drive in drives)
        {
            foreach (var folder in folders)
            {
                string folderPath = Path.Combine(drive.RootDirectory.FullName, "Windows", folder);
                if (Directory.Exists(folderPath))
                {
                    foreach (var dllName in dllNames)
                    {
                        string filePath = Path.Combine(folderPath, dllName);
                        if (File.Exists(filePath))
                        {
                            Console.WriteLine($"File already exists: {filePath}");
                        }
                        else
                        {
                            string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                            string dllFolder = Path.Combine(documentsFolder, "Dll_Folder");
                            string sourceFilePath = Path.Combine(dllFolder, dllName);

                            if (File.Exists(sourceFilePath))
                            {
                                try
                                {
                                    // Ensure the target directory exists
                                    Directory.CreateDirectory(folderPath);

                                    // Copy the DLL from Documents to the target path
                                    File.Copy(sourceFilePath, filePath, true);
                                    Console.WriteLine($"Copied {sourceFilePath} to {filePath}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Failed to copy file: {ex.Message}");
                                    Console.ReadLine();
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Source file not found: {sourceFilePath}");
                            }
                        }
                    }
                }
            }
        }
    }
}
