using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic; 

delegate bool FilePathFunc(string sourcePath);

namespace unzip_file
{
    class Program
    {
static void Main(string[] args)
        {
            string absZipFilePath = null;
            string tarExtractionDir = null;
            if(args.Length < 1 || args.Length > 2)
            {
                Console.WriteLine("at least 1 command line arguments " + 
                                  "required: args[0] == absZipFilePath | " +
                                  "optional: args[1] == tarExtractionDir!");
                return;
            }else if(args.Length == 1)
            {
                absZipFilePath   = args[0];
                var absZipFileInfo = new FileInfo(absZipFilePath);
                if( !absZipFileInfo.Exists )
                {
                    throw new IOException(String.Format("absZipFilePath '{0}' does not exist!!!", absZipFilePath));

                }
                var baseZipDirInfo = new FileInfo(absZipFilePath).Directory;
                if( !baseZipDirInfo.Exists )
                {
                    throw new IOException(String.Format("baseZipDirInfo '{0}' does not exist!!!", baseZipDirInfo.FullName));
                }
                tarExtractionDir = Path.Join(baseZipDirInfo.FullName, Path.GetFileNameWithoutExtension(absZipFilePath));
            }else if (args.Length == 2)
            {
                absZipFilePath   = args[0];
                tarExtractionDir = args[1];
            }


            bool success = unZipFile(absZipFilePath, tarExtractionDir);
            Console.WriteLine("un-zipping archive " + (success ? "was successful" : "failed") + "!");
        }

        static bool unZipFile(string absZipFilePath, string absExtractionDir)
        {
            // string zipPath = "/home/hippo/Documents/tests_tar/test.zip";

            if((new DirectoryInfo(absExtractionDir)).Exists)
            {
                absExtractionDir = askForAlternativeNonExistingDirName(absExtractionDir);
                if(String.IsNullOrEmpty(absExtractionDir))
                {
                    return false;
                }
                if((new DirectoryInfo(absExtractionDir)).Exists)
                {
                    // user hat es wohl geschafft einen directory-name zu waehlen, der zu einem bereits existierenden dirPath fuehrt -> fehler im algorithmus!!!
                    Console.WriteLine("something went wrong...");
                    return false;
                }
                var di = Directory.CreateDirectory(absExtractionDir);
                if( !di.Exists )
                {
                    Console.WriteLine(String.Format("could not create absExtractionDir '{0}'!!!", absExtractionDir));
                    return false;
                }
            }

            // if( !new DirectoryInfo(absExtractionDir).Exists )
            // {
            //     var di = Directory.CreateDirectory(absExtractionDir);
            //     if( !di.Exists )
            //     {
            //         throw new IOException(String.Format("absExtractionDir '{0}' does not exist and could not be created!!!", absExtractionDir));
            //     }
            // }

            bool totalSuccess = true;
            using (ZipArchive archive = ZipFile.OpenRead(absZipFilePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string absEntryTarPath = Path.Join(absExtractionDir, entry.FullName);
                    string absEntryTarDir = new FileInfo(absEntryTarPath).Directory.FullName;

                    bool tarDirExists = createDirectoryIfNotExistent(absExtractionDir, absEntryTarDir);
                    bool entry_success = true;
                    if( tarDirExists )
                    {
                        entry.ExtractToFile(absEntryTarPath);
                        entry_success = new FileInfo(absEntryTarPath).Exists;
                    }else{
                        entry_success = false;
                    }

                    if( !entry_success )
                    {
                        Console.WriteLine(String.Format("Could not extract archive-entry '{0}' to '{1}'!!!" + Environment.NewLine, entry.FullName, absEntryTarPath));
                    }else{
                        Console.WriteLine(String.Format("Successfully extracted archive-entry '{0}' to '{1}'!" + Environment.NewLine, entry.FullName, absEntryTarPath));
                    }
                    totalSuccess = entry_success && totalSuccess;
                }
            }
            return totalSuccess;
        }
        static bool createDirectoryIfNotExistent(string baseDirPath, string completeDirPath)
        {
            if(!completeDirPath.StartsWith(baseDirPath))
            {
                throw new ArgumentException(String.Format("in createDirectory: completeDirPath ('{0}') must start with baseDirPath ('{1}')!!!", 
                                                          completeDirPath, baseDirPath));
            }
            var dirs = completeDirPath.Substring(baseDirPath.Length).Split(Path.DirectorySeparatorChar);
            return createDirectoryIfNotExistent(baseDirPath, dirs);
        }
        static bool createDirectoryIfNotExistent(string baseDirPath, string[] dirsToAdd)
        {
            bool totalSuccess = true;
            
            string tarDirPath = baseDirPath;
            foreach(var dir in dirsToAdd)
            {
                tarDirPath = Path.Join(tarDirPath, dir);
                var di = new DirectoryInfo(tarDirPath);
                if( !di.Exists )
                {
                    di = Directory.CreateDirectory(tarDirPath);
                    totalSuccess = di.Exists && totalSuccess;
                    if(!totalSuccess)
                    {
                        return false;
                    }
                }
            }
            return totalSuccess;
        }

        static string askForAlternativeNonExistingDirName(string tarDir)
        {
            var di = new DirectoryInfo(tarDir);
            if( !di.Exists )
            {
                return tarDir;
            }
            string baseDir = di.Parent?.FullName;
            if(String.IsNullOrEmpty(baseDir))
            {
                baseDir = di.Root.FullName;
            }

            if(String.IsNullOrEmpty(baseDir))
            {
                // duerfte nach meinem verstaendnis nicht passieren...
                return null;
            }

            var cntr = 0;
            do{
                var newTarName = askUserForDirName();
                if( !String.IsNullOrEmpty(newTarName) )
                {
                    tarDir = Path.Join(baseDir, newTarName);
                    di = new DirectoryInfo(tarDir);
                }else{
                    return null;
                }
            }while(++cntr < 3 && di.Exists);

            if(cntr >= 3)
            {
                // user did not select valid dir-name!
                return null;
            }

            return tarDir;
        }
        static string askUserForDirName()
        {
            return "";
        }
        static string askForAlternativeFileName(string p)
        {
            return p;
        }

        static bool aksIfUserWantsToReplaceZipFilePath()
        {
            return true;
        }
    }
}
