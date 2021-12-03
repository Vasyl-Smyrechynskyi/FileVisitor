using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileVisitor
{
    class Program
    {
        private static readonly IList<string> _names = new List<string>();
        private static string _excludeFilterString;
        private static bool _stopSearch = false;
        private static FileSystemVisitor _systemVisitor;

        static void Main(string[] _)
        {
            Console.WriteLine("Please enter path to the start directory");
            var path = Console.ReadLine();

            Console.WriteLine("Please enter filter string");
            var filterString = Console.ReadLine();

            Console.WriteLine("Please enter filter string to exclude files and directories or press 'Enter'");
            _excludeFilterString = Console.ReadLine();

            _systemVisitor = new FileSystemVisitor(
                path,
                string.IsNullOrWhiteSpace(filterString) ? null : name => name.Contains(filterString));
            _systemVisitor.Start += (_, _) => Console.WriteLine("Started");
            _systemVisitor.Finish += FinishHandler;
            _systemVisitor.FileFound += FoundItemEventHandler;
            _systemVisitor.DirectoryFound += FoundItemEventHandler;
            _systemVisitor.FilteredDirectoryFound += FoundItemEventHandler;
            _systemVisitor.FilteredFileFound += FoundItemEventHandler;

            Task.Run(() =>
            {
                foreach (var name in _systemVisitor)
                {
                    _names.Add(name);
                }
            });

            Console.WriteLine("Press any key to stop the search");
            Console.ReadKey();

            _stopSearch = true;

            Console.ReadLine();
        }

        private static void FinishHandler(object sender, EventArgs e)
        {
            Console.WriteLine("Finished");
            foreach (var name in _names)
            {
                Console.WriteLine($"Item name - {name}");
            }
        }

        private static void FoundItemEventHandler(object obj, FoundItemEventArgs e)
        {
            e.Exclude = _excludeFilterString;
            e.StopSearch = _stopSearch;
        }
    }
}
