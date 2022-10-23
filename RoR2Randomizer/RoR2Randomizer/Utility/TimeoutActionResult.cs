namespace RoR2Randomizer.Utility
{
    public readonly struct TimeoutActionResult
    {
        public readonly float TimeElapsed;
        public readonly TimeoutActionResultState State;

        public TimeoutActionResult(float timeElapsed, TimeoutActionResultState state)
        {
            TimeElapsed = timeElapsed;
            State = state;
        }
    }
}
