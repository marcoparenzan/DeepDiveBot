namespace BotWpfClient
{
    public static class ByteArrayExtension
    {
        public static bool IsEmpty(this byte[] that)
        {
            if (that == null) return true;
            if (that.Length == 0) return true;
            return false;
        }

        public static bool IsNotEmpty(this byte[] that)
        {
            if (that == null) return false;
            if (that.Length == 0) return false;
            return true;
        }

        public static byte[] IfEmpty(this byte[] that, byte[] defaultValue)
        {
            return that.IsEmpty() ? defaultValue : that;
        }
    }
}
