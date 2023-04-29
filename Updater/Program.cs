using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Updater
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Invalid arguments");
                return;
            }

            string currentExePath = args[0];
            string newExePath = args[1];
            int parentProcessId = int.Parse(args[2]);

            try
            {
                Process parentProcess = null;
                try
                {
                    parentProcess = Process.GetProcessById(parentProcessId);
                }
                catch (ArgumentException)
                {
                    // Process does not exist, continue with the update
                }

                if (parentProcess != null)
                {
                    // Add a timeout for waiting the process to exit
                    if (!parentProcess.WaitForExit(10000))
                    {
                        DialogResult result = MessageBox.Show("The application is still running. Do you want to terminate it?", "Application still running", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            parentProcess.Kill();
                            parentProcess.WaitForExit();
                        }
                        else
                        {
                            MessageBox.Show("Update canceled.", "Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while waiting for the application to exit: " + ex.Message, "Updater", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            bool updateSuccessful = false;

            try
            {
                string backupExePath = GetBackupExePath(currentExePath);
                if (File.Exists(backupExePath))
                {
                    File.Delete(backupExePath);
                }

                File.Move(currentExePath, backupExePath);
                File.Move(newExePath, currentExePath);

                updateSuccessful = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred during the update: " + ex.Message, "Updater", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (updateSuccessful)
            {
                Process.Start(currentExePath);
            }
        }

        static string GetBackupExePath(string currentExePath)
        {
            try
            {
                Version version = AssemblyName.GetAssemblyName(currentExePath).Version;
                string directoryName = Path.GetDirectoryName(currentExePath);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(currentExePath);
                return Path.Combine(directoryName, $"{fileNameWithoutExtension}_v{version}.exe.bak");
            }
            catch (Exception)
            {
                return currentExePath + ".bak";
            }
        }
    }
}
