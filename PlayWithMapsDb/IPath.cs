namespace PlayWithMaps;

    public interface IPath
    {
        string GetDatabasePath(string filename = "MyDb.db");

        void DeleteFile(string path);
    }


