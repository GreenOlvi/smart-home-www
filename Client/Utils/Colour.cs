namespace SmartHomeWWW.Utils
{
    public struct Colour
    {
        public int R { get; init; }
        public int G { get; init; }
        public int B { get; init; }

        public string ToCss() => $"rgb({R} {G} {B})";
    }
}
