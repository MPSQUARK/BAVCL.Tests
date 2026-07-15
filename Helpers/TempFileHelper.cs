namespace BAVCL.Tests.Helpers;

public static class TempFileHelper
{
    private static readonly string Root = Path.Combine(Path.GetTempPath(), "bavcl-tests");

    public static string CreateTempDirectory()
    {
        var dir = Path.Combine(Root, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    public static void Cleanup(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }
}
