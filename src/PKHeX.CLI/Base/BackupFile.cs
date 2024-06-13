namespace PKHeX.CLI.Base;

public record BackupFile(string FilePath)
{
    private readonly int _unixTimestamp = Convert.ToInt32(FilePath.Split('.')[^2]);

    public DateTimeOffset Date => DateTimeOffset.FromUnixTimeSeconds(_unixTimestamp);

    public string Hash => Hasher.ComputeHashOf(FilePath);

    public static List<BackupFile> GetBackupFilesFor(string originalFile) =>
        Directory.GetFiles(BackupPath(originalFile), "*.backup", SearchOption.AllDirectories)
            .Where(p => IsBackupFile(p, originalFile))
            .Select(f => new BackupFile(f))
            .OrderByDescending(b => b.Date)
            .ToList();

    public static bool BackupExists(string path, out string? existingBackupPath)
    {
        var file = GetBackupFilesFor(path).FirstOrDefault(f => f.Hash == Hasher.ComputeHashOf(path));
        existingBackupPath = file?.FilePath;
        
        return file is not null;
    }

    private static String BackupPath(string path) => Path.GetDirectoryName(path)!;

    private static bool IsBackupFile(string path, string originalPath)
    {
        var originalFile = Path.GetFileName(originalPath);
        var backupFile = Path.GetFileName(path);

        return backupFile.StartsWith(originalFile)
               && backupFile.EndsWith(".backup")
               && int.TryParse(backupFile.Split('.')[^2], out var _);
    }

    public static class Name
    {
        public static string FromPath(string path) => $"{path}.{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.backup";
    }
}