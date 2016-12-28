# folderhash
Command-line recursive MD5 calculation of files in a folder, or just listing all files.

I hacked this together. Please don't read the code ;)

# Use case
I have a lot of photos which are backed up in various places. To make sure that they haven't been modified, I thought it would be a good idea to keep a hash of each file. Maybe think of this tool as a helper for finding out what's changed in two folders.

```
> FolderHash.exe -m -f C:\Photos -o hashes.txt
```

- `-m`: Perform MD5 check on files. Takes longer, especially for larger folders.
- `-f`: Folder to hash
- `-o`: Write report to this file
- `-t`: Number of threads to use. Defaults to number of logical processors on your computer.

If you do this in two different folders with similar files, you can use a diff tool to find out which files are different.
