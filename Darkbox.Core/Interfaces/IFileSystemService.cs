using System.Collections.Generic;

namespace Darkbox.Core.Interfaces;

public interface IFileSystemService
{
    IEnumerable<string> GetDrives();
    IEnumerable<string> GetDirectories(string path);
    bool HasSubDirectories(string path);
}