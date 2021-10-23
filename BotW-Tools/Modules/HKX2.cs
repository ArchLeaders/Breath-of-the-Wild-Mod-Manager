﻿using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Botw.Modules
{
    public class HKX2
    {
        static string mtlFile = null;

        /// <summary>
        /// <c>Botw.Modules.HKX2.Create</c> creates a HKX2 type file from target obj file.
        /// <para>
        /// <see cref="Create(string, string, string)"/>
        /// </para>
        /// <see href="https://github.com/ArchLeaders/Breath-of-the-Wild-Basic-Mod-Creator/blob/master/Docs/Botw_Tools/Modules/HKX2/Create.md">GitHub Documentation</see>
        /// <list type="bullet">
        /// <item><description><para><paramref name="obj"/>: Full path to .obj file</para></description></item>
        /// <item><description><para><paramref name="type"/>: HKX2 Type. E.g. HKRB, HKSC, SHKSC, HKNM2, SHKNM2</para></description></item>
        /// <item><description><para><paramref name="outFile"/>: Full path to output file</para></description></item>
        /// </list>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type"> </param>
        /// <param name="outFile"></param>
        /// <returns>Task</returns>
        public static async Task Create(string obj, string type, string outFile = null)
        {
            // FUTURE: Handle BFRES, DAE, FBX, BFMDL
            // Write CCaNM Files
            if (!File.Exists($"{Data.path}\\.HKX2\\CCaNM.exe"))
            {
                await Resource.Extract.Embed("CCaNM.resource", $"{Data.path}\\.HKX2\\cc.zip");
                await Task.Run(() => ZipFile.ExtractToDirectory($"{Data.path}\\.HKX2\\cc.zip", $"{Data.path}\\.HKX2"));

                File.Delete($"{Data.path}\\.HKX2\\cc.zip");
            }

            // Set out file when string outFile = null
            if (outFile is null)
            {
                outFile = Data.GetPath(obj) + Data.GetName(obj, true) + Data.GetExtension(obj).Replace(".obj", ".hkrb");
            }

            // Copy .obj file to working directory-
            await Task.Run(() => File.Copy(obj, Data.path + "\\.HKX2\\" + Data.GetName(obj)));

            // Get obj name from mtl file
            foreach (var item in File.ReadAllLines(obj))
            {
                // check if the line contains mtllib (this string is proceded by the obj name)
                if (item.StartsWith("mtllib"))
                {
                    // Filter name from line
                    mtlFile = Data.GetPath(obj) + item.Replace("mtllib ", "");
                    break;
                }
            }

            // Copy .mtl file to working directory.
            await Task.Run(() => File.Copy(mtlFile, Data.path + "\\.HKX2\\" + Data.GetName(mtlFile)));

            // Run CCaNM process to create HKX2 file
            await Data.Process($"{Data.path}\\.HKX2\\CCaNM.exe", "\"" + Data.path + "\\.HKX2\\" + Data.GetName(obj) +
                "\" " + type, false, false, Data.path + "\\.HKX2");

            // Call ReturnFile
            await ReturnFile(obj, mtlFile);

            async Task ReturnFile(string obj, string mtlFile)
            {
                // Return new files and delete old
                await Task.Run(() => File.Delete(Data.path + "\\.HKX2\\" + Data.GetName(obj)));
                await Task.Run(() => File.Delete(Data.path + "\\.HKX2\\" + Data.GetName(mtlFile)));
                await Task.Run(() => File.Move(Data.path + "\\.HKX2\\" + Data.GetName(obj) + ".hkrb",
                    outFile));
            }
        }
    }
}
