namespace PlayWithMaps;

public class DbPath : IPath
{
    public string GetDatabasePath(string filename)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filename); ;
        return path;
    }

    public void DeleteFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
