using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.IO.Compression;

namespace QuickVR
{

    public static class QuickZipManager
    {
        
        /// <summary>
        /// Creates a zip file from a single file. 
        /// </summary>
        /// <param name="pathSrc"></param>
        /// <param name="pathDst"></param>
        public static void CreateFromFile(string pathSrc, string pathDst)
        {
            if (IsFilePath(pathSrc))
            {
                using (FileStream fs = new FileStream(pathDst, FileMode.Create))
                using (ZipArchive arch = new ZipArchive(fs, ZipArchiveMode.Create))
                {
#if NET_2_0
                    arch.CreateEntryFromFile(pathSrc, Path.GetFileName(pathSrc));
#elif NET_4_6
                    arch.CreateEntry(pathSrc);
#endif
                }
            }
            else
            {
                Debug.LogError("pathSrc is not a file path. Use CreateFromDirectory instead. ");
            }
        }

        /// <summary>
        /// Creates a zip file from a directory. 
        /// </summary>
        /// <param name="pathSrc"></param>
        /// <param name="pathDst"></param>
        public static void CreateFromDirectory(string pathSrc, string pathDst)
        {
#if NET_2_0
            if (IsDirectoryPath(pathSrc))
            {
                ZipFile.CreateFromDirectory(pathSrc, pathDst);
            }
            else
            {
                Debug.LogError("pathSrc is not a directory path. Use CreateFromFile instead. ");
            }
#endif
        }

        private static bool IsFilePath(string path)
        {
            return !IsDirectoryPath(path);
        }

        private static bool IsDirectoryPath(string path)
        {
            // get the file attributes for file or directory
            FileAttributes attr = File.GetAttributes(path);

            //detect whether its a directory or file
            return (attr & FileAttributes.Directory) == FileAttributes.Directory;
        }

    }

}


