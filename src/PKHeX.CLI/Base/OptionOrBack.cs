namespace PKHeX.CLI.Base;

record class OptionOrBack
{
    public static IEnumerable<OptionOrBack> WithValues<T>(IEnumerable<T> options, Func<T, string>? display = null) =>
        options.Select(v => new Option<T>(v, display))
            .Cast<OptionOrBack>()
            .Append(Back.Instance);

    public record Option<T>(
        T Value,
        Func<T, string>? Display = null) : OptionOrBack
    {
        public override string ToString() => Display?.Invoke(Value) 
            ?? Value?.ToString() 
            ?? "-- no display available --";
    }

    public record class Back : OptionOrBack
    {
        private Back() { }
        public override string ToString() => "[bold darkgreen]< Back[/]";

        public static readonly Back Instance = new();
    }
}