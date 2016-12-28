using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace FolderHash
{
    class Program
    {
        class Options
        {
            [Option('t', "threads", DefaultValue = 1, HelpText = "Number of threads to use in calculations. 0 = Number of cores")]
            public int Threads { get; set; }

            [Option('f', "folder", Required = true, HelpText = "Root folder for calculations")]
            public string Folder { get; set; }

            [Option('o', "output", HelpText = "File to write the report. If unspecified, it prints to console.")]
            public string OutputFile { get; set; }

            [Option('m', "md5", DefaultValue = false, HelpText = "Perform MD5 analysis")]
            public bool Md5 { get; set; }

            [Option('r', "review-options", DefaultValue = false, HelpText = "Review the options set without running anything")]
            public bool ReviewOptions { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this,
                  (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
            }
        }

        static void ReviewOptions(Options o)
        {
            Console.WriteLine("Threads:    {0}", o.Threads);
            Console.WriteLine("Folder:     {0}", o.Folder);
            Console.WriteLine("OutputFile: {0}", o.OutputFile);
            Console.WriteLine("Md5:        {0}", o.Md5);
            Console.WriteLine("Review:     {0}", o.ReviewOptions);
        }

        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                if (options.ReviewOptions)
                {
                    ReviewOptions(options);
                    return;
                }
                if (!Directory.Exists(options.Folder))
                {
                    Console.WriteLine("That isn't a directory. Try again");
                    return;
                }

                new Program(options.Folder, options.Threads, options.OutputFile, options.Md5).run();
            }
        }

        private List<string> files;
        private string folder;
        private int threadCount;
        private string outputFile;
        private bool md5;

        public Program(string folder, int threadCount, string outputFile, bool md5)
        {
            this.folder = folder;
            this.threadCount = threadCount < 1 ? Environment.ProcessorCount : threadCount;
            this.outputFile = outputFile;
            this.md5 = md5;
        }

        public void run()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            files = listAllFiles(folder);
            if (!md5)
            {
                files.Sort();
                using (StreamWriter writer = new StreamWriter(outputFile))
                {
                    foreach (string file in files)
                    {
                        writer.WriteLine(file.Substring(folder.Length));
                    }
                }
                return;
            }
            Distributor distributor = new Distributor(files, threadCount);
            List<Calculator> calculators = new List<Calculator>();
            List<Thread> threads = new List<Thread>();
            SortedDictionary<string, string> results = new SortedDictionary<string, string>();
            for (int i = 0; i < threadCount; i++)
            {
                calculators.Add(new Calculator(distributor.getDistribution(i)));
                threads.Add(new Thread(new ThreadStart(calculators[i].calculate)));
                threads[i].Start();
            }
            for (int i = 0; i < threadCount; i++)
            {
                threads[i].Join();
                foreach(string file in calculators[i].Result.Keys)
                {
                    results.Add(file, calculators[i].Result[file]);
                }
            }
            if (outputFile != null)
            {
                using (StreamWriter writer = new StreamWriter(outputFile))
                {
                    foreach (string key in results.Keys)
                    {
                        writer.WriteLine("{0} -> {1}", key.Substring(folder.Length), results[key]);
                    }
                }
                Console.WriteLine("Written to {0}", outputFile);
            } else
            {
                foreach (string key in results.Keys)
                {
                    Console.WriteLine("{0} -> {1}", key.Substring(folder.Length), results[key]);
                }
            }
            
            timer.Stop();
            Console.WriteLine("Process took {0}ms", timer.ElapsedMilliseconds);
        }

        public List<string> listAllFiles(string folder)
        {
            List<string> files = new List<string>();
            foreach (string f in Directory.GetFiles(folder))
            {
                files.Add(f);
            }
            foreach (string d in Directory.GetDirectories(folder))
            {
                files.AddRange(listAllFiles(d));
            }
            return files;
        }
    }

    internal class Calculator
    {
        private List<string> files;
        public Dictionary<string, string> Result { get; set; }

        public Calculator(List<string> files)
        {
            this.files = files;
        }

        public void calculate()
        {
            Result = new Dictionary<string, string>();
            foreach(string fileName in files)
            {
                string hash = md5(fileName);
                Result.Add(fileName, hash);
            }
        }

        public string md5(string fileName)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] hashBytes;
            using (FileStream file = new FileStream(fileName, FileMode.Open))
            {
                hashBytes = md5.ComputeHash(file);
            }
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            string hash = sb.ToString();
            return hash;
        }
    }

    internal class Distributor
    {
        private List<string> files;
        private int threads;
        private List<string>[] dLists;

        public Distributor(List<string> files, int threads)
        {
            this.files = files;
            this.threads = threads;
            dLists = new List<string>[threads];
            for (int i = 0; i < threads; i++)
            {
                dLists[i] = new List<string>();
            }
            for (int i = 0; i < files.Count; i++)
            {
                dLists[i % threads].Add(files[i]);
            }
        }

        public List<string> getDistribution(int i)
        {
            return dLists[i];
        }
    }
}
