using System.Diagnostics;
using System.IO;
using System.Linq;
using Darkbox.Core.Interfaces;
using Darkbox.Core.Services;

namespace Darkbox.Core.Services;

public class FileSystemService : IFileSystemService
{
    public IEnumerable<string> GetDrives() => 
        DriveInfo.GetDrives()
            .Where(d => d.IsReady)
            .Select(d => d.RootDirectory.FullName);
    
    public IEnumerable<string> GetDirectories(string path)
    {
        try
        {
            return Directory.GetDirectories(path);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return Enumerable.Empty<string>();
        }
    }

    public bool HasSubDirectories(string path)
    {
        try
        {
            return Directory.EnumerateDirectories(path).Any();
        }
        catch
        {
            return false;
        }
    }
}