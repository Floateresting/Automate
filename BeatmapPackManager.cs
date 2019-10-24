using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace ConsoleOsu {
    /// <summary>
    /// Class for managing beatmap packs
    /// </summary>
    class BeatmapPackManager {
        #region Private Members

        /// <summary>
        /// Directory path to osu!
        /// </summary>
        private readonly string osuDir;

        /// <summary>
        /// Directory path to downloaded pack
        /// </summary>
        private readonly string downloadDir;

        /// <summary>
        /// Name of downloaded pack
        /// </summary>
        private string newPackName;

        /// <summary>
        /// Path of downloaded pack
        /// </summary>
        private string packPath;
        #endregion

        #region Conctructor

        /// <summary>
        /// Default Contructor
        /// </summary>
        public BeatmapPackManager(string osuDir, string downloadDir) {
            this.osuDir = osuDir;
            this.downloadDir = downloadDir;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Extract the pack to songs folder
        /// </summary>
        /// <returns>new beatmap set ids</returns>
        public List<string> ExtractRar() {
            // Try to get the beatmap pack file
            string packRar = "";
            try {
                packRar = Directory.GetFiles(downloadDir, "Beatmap Pack*.rar")[0];
            } catch(IndexOutOfRangeException) {
                throw new Exception($"Can't find any beatmap pack in '{downloadDir}'");
            }

            // if 'Songs' folder doesn't exist
            if(!Directory.Exists(osuDir + @"\songs\")) {
                throw new Exception($"osu! directory '{osuDir}' not valid: can't find 'Songs' folder");
            }
            string songDir = osuDir + @"\songs\";

            packPath = packRar;

            newPackName = Path.GetFileNameWithoutExtension(packRar);
            Log($"Found {newPackName}, press enter to confirm");
            Console.ReadLine();

            Regex rx = new Regex(@"[0-9]+");
            if(rx.IsMatch(newPackName)) {
                newPackName = rx.Match(newPackName).Value;
            } else {
                Log("Can't find pack id, please enter manually");
                Console.Write("Pack id: ");
                newPackName = Console.ReadLine();
            }


            Log($"Extracting '{Path.GetFileName(packRar)}'");

            // Use unrar.exe to extract the pack
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.StandardInput.WriteLine($"unrar x -o+ \"{packRar}\" \"{songDir}\"");
            process.StandardInput.Flush();
            process.StandardInput.Close();
            process.WaitForExit();

            // Get all downloaded beatmap sets folder names
            List<string> beatmapSetIds = new List<string>();
            foreach(string oszFile in Directory.GetFiles(songDir, "*.osz")) {
                beatmapSetIds.Add(Path.GetFileNameWithoutExtension(oszFile));
            }

            Log("Waiting for osu!.exe to extract .osz files");
            Process.Start(osuDir + "osu!.exe");
            while(Directory.GetFiles(songDir, "*.osz").Length > 0) { }

            // Close osu!.exe
            Log("Waiting for osu!.exe to write osu!.db");
            Thread.Sleep(10000);
            Process.GetProcessesByName("osu!")[0].CloseMainWindow();
            Thread.Sleep(2000);
            return beatmapSetIds;
        }

        /// <summary>
        /// Search in osu!.db and get all hashes
        /// </summary>
        /// <param name="folderNames">List of song folder's names</param>
        /// <returns>List of hashes found in osu!.db</returns>
        public List<string> GetBeatmapHashes(List<string> folderNames) {
            Log("Getting beatmap hashes from osu!.db");

            // Get osu database
            OsuDatabaseParser parser = new OsuDatabaseParser(osuDir + "osu!.db");
            OsuDatabase database = parser.ParseOsuDatabase();
            List<string> hashes = new List<string>();
            bool allBeatmapsFound = true;

            // Write errors in to a text file
            using(StreamWriter sw = new StreamWriter(File.Open("ErrorList.txt", FileMode.Append))) {
                sw.WriteLine("Beatmap Pack #" + newPackName);
                foreach(string folderName in folderNames) {
                    // Find the beatmap set id from its folder in 'songs'
                    Regex rx = new Regex(@"^[0-9]+");
                    int id = int.Parse(rx.Match(folderName).Value);

                    List<Beatmap> beatmaps = new List<Beatmap>();
                    // if the beatmap set is in osu!.db
                    if(database.Beatmaps.TryGetValue(id, out beatmaps)) {
                        foreach(Beatmap beatmap in beatmaps) {
                            hashes.Add(beatmap.Hash);
                        }
                        Log($"Added {folderName} to collection");
                    } else {
                        Log("Can't find '" + folderName + "'");
                        sw.WriteLine("\t" + folderName);
                        allBeatmapsFound = false;
                    }
                }
            }
            if(!allBeatmapsFound) {
                Log("Not all beatmaps are found, press enter to continue");
                Console.ReadLine();
            }
            return hashes;
        }

        public void CollectionBackup() {
            File.Copy(osuDir + "collection.db", $"{Environment.CurrentDirectory}\\CollectionBackup\\{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.db");
            Log("Collection.db backup saved in " + Environment.CurrentDirectory + @"\CollectionBackup\");
        }

        /// <summary>
        /// Add a new collection in collection.db
        /// </summary>
        /// <param name="hashes">Beatmap hashes to add</param>
        /// <returns>New edited collection database</returns>
        public CollectionDatabase EditCollection(List<string> hashes) {
            Log("Editing 'collection.db'");
            CollectionDatabaseParser parser = new CollectionDatabaseParser(osuDir + "collection.db");
            CollectionDatabase database = parser.ParseCollectionDatabase();
            database.AddCollection("Pack " + newPackName, hashes);
            return database;
        }

        /// <summary>
        /// Save the collection
        /// </summary>
        /// <param name="database">Edited collection database</param>
        public void SaveCollection(CollectionDatabase database) {
            Log("Saving 'collection.db'");
            CollectionDatabaseWriter writer = new CollectionDatabaseWriter(osuDir + "collection.db", database);
            writer.WriteCollection();
            using(StreamWriter sw = new StreamWriter(File.Open("History.txt", FileMode.Append))) {
                sw.Write(DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]"));
                sw.WriteLine($"\t Added 'Pack {newPackName}'");
            }
            File.Delete(packPath);
            Log("Finished, press enter to exit");
        }
        #endregion

        #region Debug Methods

        public void EnumerateCollection() {
            CollectionDatabaseParser parser = new CollectionDatabaseParser(osuDir + "collection.db");
            CollectionDatabase database = parser.ParseCollectionDatabase();
            foreach(Collection collection in database.Collections) {
                Console.WriteLine(collection.Name);
            }
        }
        #endregion

        public static void Log(string log) => Console.WriteLine($"[{DateTime.Now.ToString("hh:mm:ss")}] {log}");
    }
}
