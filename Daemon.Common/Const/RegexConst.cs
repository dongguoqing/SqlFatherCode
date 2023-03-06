namespace Daemon.Common.Const
{
    public static class RegexConst
    {
        public static string EmailRegex = @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";

        public static string PhoneRegex = @"^1[3456789]\d{9}$";
    }
}
