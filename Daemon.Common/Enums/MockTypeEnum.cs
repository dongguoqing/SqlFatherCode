namespace Daemon.Common.Enums
{
    using Daemon.Common.Enums.Extension;
    public enum MockTypeEnum
    {
        [EnumDescription("NONE","不模拟")]
        NONE,//不模拟
        [EnumDescription("INCREASE","递增")]
        INCREASE,//递增
        [EnumDescription("FIXED","固定")]
        FIXED,//固定
        [EnumDescription("RANDOM","随机")]
        RANDOM,//随机
        [EnumDescription("RULE","规则")]
        RULE,//规则
        [EnumDescription("DICT","词库")]
        DICT//词库
    }
}