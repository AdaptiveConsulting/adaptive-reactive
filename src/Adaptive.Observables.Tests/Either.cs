namespace Adaptive.Observables.Tests
{
    public sealed class Either<TLeft, TRight>
    {
        public TLeft Left { get; private set; }
        public TRight Right { get; private set; }
        public bool IsLeft { get; private set; }

        public Either(TLeft left)
        {
            Left = left;
            IsLeft = true;
        }

        public Either(TRight right)
        {
            Right = right;
            IsLeft = false;
        }
    }
}