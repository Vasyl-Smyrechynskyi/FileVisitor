using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;

namespace FileVisitor
{
    public class FileSystemVisitor : IEnumerable<string>
    {
        private readonly string _startPath;
        private readonly Func<string, bool> _searchPredicate;
        private bool _stopSearch;
        public event EventHandler Start;
        public event EventHandler Finish;
        public event EventHandler<FoundItemEventArgs> FileFound;
        public event EventHandler<FoundItemEventArgs> DirectoryFound;
        public event EventHandler<FoundItemEventArgs> FilteredFileFound;
        public event EventHandler<FoundItemEventArgs> FilteredDirectoryFound;

        public FileSystemVisitor(string startPath, Func<string, bool> searchPredicate)
        {
            _startPath = startPath;
            _searchPredicate = searchPredicate;
        }

        public IEnumerator<string> GetEnumerator()
        {
            var enumerator = FindFilesAndDirectories();
            while (enumerator.MoveNext() && !_stopSearch)
            {
                yield return enumerator.Current;
            }

            Finish?.Invoke(this, null);
        }

        private IEnumerator<string> FindFilesAndDirectories()
        {
            Start?.Invoke(this, null);

            return Directory.GetFileSystemEntries(_startPath, "*.*", SearchOption.AllDirectories)
                .Where((name) =>
                {
                    if (Directory.Exists(name))
                    {
                        return CheckIfEligible(name, true);
                    }

                    return CheckIfEligible(name);
                })
                .GetEnumerator();
        }

        private bool CheckIfEligible(string name, bool isDirectory = false)
        {
            if (_searchPredicate != null && !(_searchPredicate?.Invoke(name) == true))
                return false;

            var args = new FoundItemEventArgs();

            if (isDirectory && _searchPredicate is null)
            {
                DirectoryFound?.Invoke(this, args);
            }
            else if (_searchPredicate is null)
            {
                FileFound?.Invoke(this, args);
            }

            if (isDirectory && _searchPredicate != null)
            {
                FilteredDirectoryFound?.Invoke(this, args);
            }
            else if (_searchPredicate != null)
            {
                FilteredFileFound?.Invoke(this, args);
            }

            _stopSearch = args.StopSearch;

            if (!string.IsNullOrWhiteSpace(args.Exclude) && name.Contains(args.Exclude))
                return false;

            return true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return FindFilesAndDirectories();
        }
    }

    public class FoundItemEventArgs : EventArgs
    {
        public bool StopSearch { get; set; }

        public string Exclude { get; set; }
    }
}
