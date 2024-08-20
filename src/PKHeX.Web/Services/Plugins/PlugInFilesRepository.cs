using TG.Blazor.IndexedDB;

namespace PKHeX.Web.Services.Plugins;

public class PlugInFilesRepository(
    IndexedDBManager db)
{
    public async Task CreateOrUpdate(LoadedPlugIn plugIn, File file)
    {
        if (string.IsNullOrWhiteSpace(file.FileName) || file.Data.Length == 0) return;
        
        var existing = await GetFile(plugIn.Id, file.FileName);
        var record = new StoreRecord<FileRepresentation>
        {
            Storename = Schema.Name,
            Data = FileRepresentation.Create(plugIn, file)
        };
        
        if (existing is null)
        {
            await db.AddRecord(record);   
        }
        else
        {
            await db.UpdateRecord(record);
        }
    }

    public async Task<File?> GetFile(string plugInId, string fileName)
    {
        var file = await db.GetRecordById<string, FileRepresentation>(Schema.Name,
            FileRepresentation.KeyFor(plugInId, fileName));

        if (file is null) return null;

        return new File(file.Data, file.FileName);
    }

    public async Task Remove(string plugInId, string fileName)
    {
        await db.DeleteRecord(Schema.Name, FileRepresentation.KeyFor(plugInId, fileName));
    }

    public async Task RemoveAllFrom(LoadedPlugIn plugIn)
    {
        var query = new StoreIndexQuery<string>
        {
            Storename = Schema.Name,
            IndexName = PlugInIdIndex.Name,
            QueryValue = plugIn.Id,
        };
        var files = await db.GetAllRecordsByIndex<string, FileRepresentation>(query);
        var deleteTasks = files
            .Where(f => f != null)
            .Select(f => db.DeleteRecord(Schema.Name, f.Key));
        
        await Task.WhenAll(deleteTasks);
    }

    public record File(byte[] Data, string FileName);

    public class FileRepresentation
    {
        public string Key { get; set; } = default!;
        public byte[] Data { get; set; } = default!;
        public string FileName { get; set; } = default!;
        public string PlugInId { get; set; } = default!;

        public static FileRepresentation Create(LoadedPlugIn plugIn, File file) => new()
        {
            Key = KeyFor(plugIn.Id, file.FileName),
            Data = file.Data,
            FileName = file.FileName,
            PlugInId = plugIn.Id,
        };

        public static string KeyFor(string plugInId, string fileName) => $"{plugInId}#{fileName}";
    }

    private static readonly IndexSpec PlugInIdIndex = new IndexSpec
    {
        Name = "plugInId", 
        KeyPath = "plugInId", 
        Auto = false
    };
    
    public static readonly StoreSchema Schema = new()
    {
        Name = "PlugInFiles",
        PrimaryKey = new IndexSpec { Name = "key", KeyPath = "key", Unique = true, Auto = false },
        Indexes = [PlugInIdIndex]
    };
}