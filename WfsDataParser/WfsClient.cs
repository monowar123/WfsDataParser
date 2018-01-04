using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WfsDataParser
{
    public class WfsClient
    {
        string wfsAddress;
        string gdalDirectory;

        public WfsClient(string wfsAddress)
        {
            this.wfsAddress = wfsAddress;
            this.gdalDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GDAL");
        }

        public void CreateShape(string outputFile)
        {
            string temporaryBatchPath = GetBatch();

            string[] fileLines = new string[2];
            fileLines[0] = string.Format("ogr2ogr -overwrite -f \"ESRI Shapefile\" \"{0}\" \"WFS:{1}\"", outputFile, wfsAddress);
            fileLines[1] = "exit";
            File.AppendAllLines(temporaryBatchPath, fileLines);

            RunBatch(temporaryBatchPath);
        }

        public void InsertIntoDb(string connString, string tableName = "")
        {
            string temporaryBatchPath = GetBatch();
            string pgConnection = GetConnectionForPG(connString);

            string[] fileLines = new string[2];
            if (tableName == "")
            {
                fileLines[0] = string.Format("ogr2ogr -overwrite -f PostgreSQL {0} \"WFS:{1}\"", pgConnection, wfsAddress);
            }
            else
            {
                fileLines[0] = string.Format("ogr2ogr -overwrite -f PostgreSQL {0} \"WFS:{1}\" -nln {2}", pgConnection, wfsAddress, tableName);
            }
            fileLines[1] = "exit";
            File.AppendAllLines(temporaryBatchPath, fileLines);

            RunBatch(temporaryBatchPath);
        }

        private string GetBatch()
        {
            string batchPath = gdalDirectory + "\\SDKShell.bat";
            string tempBatchName = Guid.NewGuid().ToString() + ".bat";
            string temporaryBatchPath = Path.Combine(gdalDirectory, tempBatchName);

            string[] fileLines = File.ReadAllLines(batchPath);

            string replacingLine = "%comspec% /k \"%SDK_ROOT%" + tempBatchName + "\" setenv %1";

            for (int count = 0; count < fileLines.Length; count++)
            {
                if (fileLines[count].StartsWith("%comspec% /k")) fileLines[count] = replacingLine;
            }
            File.WriteAllLines(temporaryBatchPath, fileLines);

            return temporaryBatchPath;
        }

        private void RunBatch(string batchFile)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = batchFile;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;

            Process process = new Process();
            process.StartInfo = info;
            process.Start();
            process.WaitForExit();

            string processOutput = process.StandardError.ReadToEnd();

            Console.WriteLine(processOutput);

            File.Delete(batchFile);
        }

        private string GetConnectionForPG(string connectionString)
        {
            string returnStr = string.Empty;
            string[] connecPortions = connectionString.Split(';');
            List<string> list = new List<string>();

            foreach (string portion in connecPortions)
            {
                if (portion.ToLower().Trim().StartsWith("server") || portion.ToLower().Trim().StartsWith("host"))
                {
                    string[] hostPortion = portion.Split('=');
                    list.Add(string.Format("host={0}", hostPortion[1]));
                }

                else if (portion.ToLower().Trim().StartsWith("database"))
                {
                    string[] hostPortion = portion.Split('=');
                    list.Add(string.Format("dbname={0}", hostPortion[1]));
                }

                else if (portion.ToLower().Trim().StartsWith("user"))
                {
                    string[] hostPortion = portion.Split('=');
                    list.Add(string.Format("user={0}", hostPortion[1]));
                }

                else if (portion.ToLower().Trim().StartsWith("password"))
                {
                    string[] hostPortion = portion.Split('=');
                    list.Add(string.Format("password={0}", hostPortion[1]));
                }

                else if (portion.ToLower().Trim().StartsWith("port"))
                {
                    string[] hostPortion = portion.Split('=');
                    list.Add(string.Format("port={0}", hostPortion[1]));
                }
            }

            returnStr = string.Join(" ", list.ToArray());

            return string.Format("PG:\"{0}\"", returnStr);
        }
    }
}
