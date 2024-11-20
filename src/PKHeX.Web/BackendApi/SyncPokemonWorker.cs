using PKHeX.Facade.Pokemons;
using PKHeX.Web.BackendApi.Representation;

namespace PKHeX.Web.BackendApi;

public class SyncPokemonWorker(
    SyncPokemonQueue queue,
    MyPokemonRepository myPokemonRepository,
    ILogger<SyncPokemonQueue> logger)
{
    public static event Func<PokemonUploadedArgs, Task>? PokemonUploaded;
    public static event Func<PokemonUploadStartedArgs, Task>? PokemonUploadStarted;
    public static event Func<EventArgs, Task>? PokemonUploadEnded;

    private readonly PeriodicTimer _timer = new(TimeSpan.FromMilliseconds(850));

    private bool _running;
    private Task? _worker;

    /// <summary>
    /// Currently in progress Pokemon upload.
    /// </summary>
    public Pokemon? PokemonSyncing { get; private set; }

    public void Start()
    {
        _running = true;
        _worker = Run();
    }

    private async Task Run()
    {
        while (_running)
        {
            await _timer.WaitForNextTickAsync();
            var (pokemon, priority) = queue.Dequeue();

            if (pokemon is null) continue;

            await SafeDeliverPokemonUploadStarted(queue.Size);

            while (pokemon is not null && priority is not null)
            {
                // sets the current pokemon being uploaded
                PokemonSyncing = pokemon;
                
                var metadata = await TryToUpload(pokemon, priority.Value);
                
                // clears the current pokemon being uploaded
                PokemonSyncing = null;
                
                if (metadata is not null)
                {
                    await SafeDeliverPokemonUploaded(pokemon, metadata);
                }

                (pokemon, priority) = queue.Dequeue();
            }
            
            await SafeDeliverPokemonUploadEnded();
        }
    }

    private async Task<PokemonMetadataRepresentation?> TryToUpload(Pokemon pokemon, int priority)
    {
        // Only used when running manual checks
        // await Task.Delay(1000);
        // return new PokemonMetadataRepresentation
        // {
        //     Gender = pokemon.Gender,
        //     Nickname = pokemon.Nickname,
        //     Moves = pokemon.Moves.Values.Select(m => m.Move).ToList()
        // };
        
        try
        {
            var metadata = await myPokemonRepository.Upload(pokemon);
            return metadata;
        }
        catch (Exception e)
        {
            if (priority < MaxUploadAttempts)
            {
                queue.Enqueue(pokemon, priority + 1);
            }
            else
            {
                logger.LogError(e, "Failed to upload pokemon");
            }

            return null;
        }
    }

    private async Task SafeDeliverPokemonUploaded(Pokemon pokemon, PokemonMetadataRepresentation metadata)
    {
        var args = new PokemonUploadedArgs(pokemon, metadata);
        try
        {
            if (PokemonUploaded is not null)
            {
                await PokemonUploaded(args);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to deliver event");
        }
    }

    private async Task SafeDeliverPokemonUploadStarted(int queueSize)
    {
        var args = new PokemonUploadStartedArgs(queueSize);
        try
        {
            if (PokemonUploadStarted is not null)
            {
                await PokemonUploadStarted(args);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to deliver event");
        }
    }

    private async Task SafeDeliverPokemonUploadEnded()
    {
        var args = EventArgs.Empty;
        try
        {
            if (PokemonUploadEnded is not null)
            {
                await PokemonUploadEnded(args);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to deliver event");
        }
    }

    private const int MaxUploadAttempts = 3;
}

public record PokemonUploadedArgs(
    Pokemon Pokemon,
    PokemonMetadataRepresentation Metadata);

public record PokemonUploadStartedArgs(
    int QueueSize);