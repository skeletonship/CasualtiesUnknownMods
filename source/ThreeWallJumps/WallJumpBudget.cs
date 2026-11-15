namespace ThreeWallJumps
{
    // One budget for the currently touched wall side, not two permanent budgets.
    internal sealed class WallJumpBudget
    {
        internal const int Limit = 3;
        internal int Side { get; private set; }
        internal int Used { get; private set; }

        internal void Reset()
        {
            Side = 0;
            Used = 0;
        }

        internal void Touch(int side)
        {
            if (side != 0 && side != Side)
            {
                Side = side;
                Used = 0;
            }
        }

        internal bool CanJump(int side) => side != Side || Used < Limit;

        internal void RecordJump(int side)
        {
            Touch(side);
            // Climbing claws can allow unlimited jumps; don't overflow the count.
            if (Used < Limit)
                Used++;
        }
    }
}
