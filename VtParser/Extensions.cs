namespace VtParser
{
    public static class Extensions
    {
        public static string GetActionName(this Actions action)
        {
            return Tables.ActionNames[(int) action];
        }

        public static string GetStateName(this States state)
        {
            return Tables.StateNames[(int)state];
        }
    }
}
