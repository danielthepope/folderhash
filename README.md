# folderhash
Command-line recursive MD5 calculation of files in a folder. 

I hacked this together. Please don't read the code ;)

# Use case
I have a lot of photos which are backed up in various places. To make sure that they haven't been modified, I thought it would be a good idea to keep a hash of each file. Maybe think of this tool as a helper for finding out what's changed in two folders.

```
> FolderHash.exe -f C:\Photos -o hashes.txt`
```

- `-f`: Folder to hash
- `-o`: Write report to this file
- `-t`: Number of threads to use. Defaults to number of logical processors on your computer.


