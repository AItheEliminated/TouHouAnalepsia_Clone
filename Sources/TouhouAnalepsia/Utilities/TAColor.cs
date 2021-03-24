using TaleWorlds.Library;

namespace TouhouAnalepsia
{
    public static class TAColor
    {
        public static Color Error
        {
            get
            {
                return Color.ConvertStringToColor("#E3242BFF");
            }
        }
        public static Color Warning
        {
            get
            {
                return Color.ConvertStringToColor("#FCE205FF");
            }
        }
        public static Color Info
        {
            get
            {
                return Color.ConvertStringToColor("#0080FFFF");
            }
        }
        public static Color Highlight
        {
            get
            {
                return Color.ConvertStringToColor("#03AC13FF");
            }
        }

        public static Color CustomColor(string colorStr)
        {
            return Color.ConvertStringToColor(colorStr);
        }
    }
}
