namespace SmartHomeWWW.Utils
{
    public class ColourGradient
    {
        public ColourGradient(Colour colour1, Colour colour2)
        {
            _colour1 = colour1;
            //_colour2 = colour2;

            _diffR = colour2.R - colour1.R;
            _diffG = colour2.G - colour1.G;
            _diffB = colour2.B - colour1.B;
        }

        private readonly Colour _colour1;
        //private readonly Colour _colour2;

        private readonly int _diffR;
        private readonly int _diffG;
        private readonly int _diffB;

        public Colour GetIntermediate(double p)
        {
            var l = Math.Clamp(p, 0.0, 1.0);
            return new()
            {
                R = (int)(_colour1.R + (_diffR * l)),
                G = (int)(_colour1.G + (_diffG * l)),
                B = (int)(_colour1.B + (_diffB * l)),
            };
        }
    }
}
