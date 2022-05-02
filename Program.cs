using System;
using System.IO;

namespace BackupMaster
{
    internal class Program
    {
        private static string workingDirectory = Environment.CurrentDirectory;
        private static string targetDirectory;
        private static int maxFilesInDifferences = 5000;

        static FileStream filesToRemove;

        static string Data(string path)
        {
            return path + "\\_BackupMasterData";
            
        }

        static string PathEnd(string path, string fromPath)
        {
            return path.Substring(fromPath.Length);
        }
        
        public static void Main(string[] args)
        {
            Console.WriteLine("Stage 1: Loading Data...");
            LoadData();
            Console.WriteLine($"Stage 1 complete!\n{workingDirectory} set as original directory and {targetDirectory} is set as target.\n");
            
            /*
             //Next step make console program!
            foreach (string arg in args)
            {
                Console.WriteLine(arg);
            }
            */
            
            Console.WriteLine("=============");
            Console.WriteLine("Stage 2: Preparing Backup DATA directory...");
            if (Directory.Exists(Data(workingDirectory)+"\\DATA"))
            {
                Directory.Delete(Data(workingDirectory)+"\\DATA", true);
            }
            Directory.CreateDirectory(Data(workingDirectory)+"\\DATA");
            Console.WriteLine("Stage 2 complete!\n");
            
            Console.WriteLine("Stage 3: Getting difference...");
            filesToRemove = File.Open(Data(workingDirectory)+"\\FilesToRemove.txt", FileMode.Create);
            GetDifferences(workingDirectory, targetDirectory);
            filesToRemove.Close();
            Console.WriteLine("Stage 3 complete!\n");
            
            Console.WriteLine("Stage 4: Synching from backup directory...");
            MoveFromBackup();
            Console.WriteLine("Stage 4 complete!\n");
            // print_all_files(workingDirectory);

            // Directory.CreateDirectory(Data(workingDirectory));
            
            
            Console.WriteLine("Stage 5: Saving data...");
            SaveData();
            Console.WriteLine("Stage 5 complete!\n");
            
            Console.WriteLine("FINISH!\nPress ENTER to close this window\n");
            Console.ReadLine();
        }

        static void GetDifferences(string workingPath, string targetPath)
        {
            // string[] differentFiles=new string[maxFilesInDifferences];
            // int differentFileIndex=0
            
            string targetFile, clearFile, backupPath=Data(workingDirectory)+"\\DATA"+PathEnd(workingPath, workingDirectory);
            // Console.WriteLine(backupPath);
            
            // Directory.CreateDirectory(backupPath);
            
            Console.WriteLine($">> Starting for \n{workingPath} | {targetPath}.\n Backup directory is {backupPath}\n");
            
            Console.WriteLine("!Part 1: searching new and modified files...");
            
            foreach (string file in Directory.GetFiles(workingPath))
            {
                clearFile=Path.GetFileName(file);
                targetFile = targetPath + "\\" + clearFile;
                
                /*
                Console.WriteLine("==========\n"+targetFile);
                Console.WriteLine(!File.Exists(targetFile));
                Console.Write(File.GetLastWriteTime(file));
                Console.Write("| |");
                Console.WriteLine(File.GetLastWriteTime(targetFile));
                Console.WriteLine("=============");
                */
                
                Console.Write($"-{file} is... ");
                
                if (!File.Exists(targetFile) || File.GetLastWriteTime(file)!=File.GetLastWriteTime(targetFile))
                {
                    Console.WriteLine("added to backup list.");
                    if (!Directory.Exists(backupPath)) {
                        Directory.CreateDirectory(backupPath);
                    }
                    File.Copy(file, backupPath+"\\"+Path.GetFileName(file), true);
                    // Console.WriteLine(file+'\n'+backupPath+"\\"+Path.GetFileName(file));
                }
                else
                {
                    Console.WriteLine("already up-to-date.");
                }
            }
            Console.WriteLine("!Part 2: searching removed files...");

            foreach (string file in Directory.GetFiles(targetPath))
            {
                clearFile=Path.GetFileName(file);
                targetFile = workingPath + "\\" + clearFile;
                if (!File.Exists(targetFile))
                {
                    // differentFiles[differentFileIndex] = file;
                    // differentFileIndex++;
                    
                    /*
                    if (filesToRemove.Length>0)
                    {
                        file = "\n" + file;
                    }
                    */
                    Console.WriteLine($"-{file} is removed...");
                    
                    Byte[] file_b = new System.Text.UTF8Encoding(true).GetBytes((filesToRemove.Length>0? Environment.NewLine:"")+file);
                    filesToRemove.Write(file_b, 0, file_b.Length);
                }
            }

            Console.WriteLine("!Part 3: checking the subdirectories...");

            foreach (string newPath in Directory.GetDirectories(workingPath))
            {
                if (newPath==Data(workingDirectory) || newPath==Data(targetDirectory))
                {
                    return;
                }

                clearFile = targetPath + PathEnd(newPath, workingPath);

                if (Directory.Exists(clearFile))
                {
                    GetDifferences(newPath, clearFile);
                    /*
                    foreach (string file in GetDifferences(newPath, clearFile))
                    {
                        // differentFiles[differentFileIndex] = file;
                        // differentFileIndex++;
                    }
                    // GetDifferences(newPath, clearFile);
                    */

                }
                else
                {
                    Console.WriteLine($"-{newPath} is new. Copying to backup...");
                    Copy(newPath, backupPath+PathEnd(newPath, workingPath));
                }

                
                /*
                Console.WriteLine(newPath);
                Console.WriteLine(targetPath+PathEnd(newPath, workingPath));
                Console.WriteLine("");
                */
            }
            
            Console.WriteLine($">> {workingPath}|{targetPath} checked!");

            // return differentFiles;
        }

        static void MoveFromBackup()
        {
            string path = Data(workingDirectory) + "\\DATA";
            // Console.WriteLine(">>>"+path);
            
            Console.WriteLine("!Part 1: copying files...");
            foreach (string file in Directory.GetFiles(path))
            {
            
                /*
                Console.WriteLine(file);
                Console.WriteLine(targetDirectory+"\\"+Path.GetFileName(file));
                */
            
                File.Copy(file, targetDirectory+"\\"+Path.GetFileName(file), true);
            }
            
            Console.WriteLine("!Part 2: copying subdirectories...");
            foreach (string file in Directory.GetDirectories(path))
            {
                Copy(file, targetDirectory+PathEnd(file, path));
                
                /*
                Console.WriteLine(file);
                Console.WriteLine(targetDirectory+PathEnd(file, path));
                */
                
            }

            Console.WriteLine("!Part 3: removing files...");
            foreach (string file in File.ReadAllLines(Data(workingDirectory)+"\\filesToRemove.txt"))
            {
                // Console.WriteLine(file);
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
                
            }
            
            
        
        }

        static void Copy(string path, string dstPath)
        {
            if (path==Data(workingDirectory) || path==Data(targetDirectory))
            {
                return;
            }
            // Console.WriteLine($"\nIN <{path}>:");
            // Console.WriteLine($"\nIN <{dstPath}>:");
            string[] files = Directory.GetFiles(path);

            if (files.Length>0)
            {
                Directory.CreateDirectory(dstPath);
                foreach (string file in files)
                {
                
                    /*
                    Console.WriteLine(file);
                    Console.WriteLine(dstPath+"\\"+Path.GetFileName(file));
                    */
                
                    File.Copy(file, dstPath+"\\"+Path.GetFileName(file), true);
                }        
            }
            
            foreach (string file in Directory.GetDirectories(path))
            {
                // print_all_files(file);
                Copy(file, dstPath+PathEnd(file, path));
                
                // Console.WriteLine(file);
                // Console.WriteLine(dstPath+PathEnd(file, path));
            }
        }

        static  void print_all_files(string path)
        {
            if (path==Data(workingDirectory) || path==Data(targetDirectory))
            {
                return;
            }
            Console.WriteLine($"\nIN <{path}>:");
            foreach (string file in Directory.GetFiles(path))
            {
                Console.WriteLine(file);
            }

            foreach (string file in Directory.GetDirectories(path))
            {
                print_all_files(file);
            }
            Console.WriteLine("^===========^\n");
        }

        static void LoadData()
        {
            if (File.Exists(Data(workingDirectory) + "\\pref.txt"))
            {
                string[] prefs = File.ReadAllLines(Data(workingDirectory) + "\\pref.txt");
                // Console.WriteLine(prefs);
                targetDirectory = prefs[0];
                workingDirectory = prefs[1];
                maxFilesInDifferences = Convert.ToInt32(prefs[2]);
            }
            else
            {
                Console.Write("Please enter target directory down\n-");
                targetDirectory = Console.ReadLine().Replace("\"", "");
            }
        }
        static void SaveData()
        {
            string[] prefs = { targetDirectory, workingDirectory , maxFilesInDifferences.ToString()};
            File.WriteAllLines(Data(workingDirectory) + "\\pref.txt", prefs);
            if (!Directory.Exists(Data(targetDirectory)))
            {
                Directory.CreateDirectory(Data(targetDirectory));
            }
            File.WriteAllLines(Data(targetDirectory) + "\\pref.txt", prefs);
        }
    }
}