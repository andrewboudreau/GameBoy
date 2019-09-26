using System;

namespace GameBoy
{
    /// <summary>
    /// The Dot Matrix GameBoy Bootup Sequence.
    /// </summary>
    public class BootRom
    {
        public static string HexString = "31FEFFAF21FF9F32CB7C20FB2126FF0E113E8032E20C3EF3E2323E77773EFCE0471104012110801ACD9500CD9600137BFE3420F311D80006081A1322230520F93E19EA1099212F990E0C3D2808320D20F92E0F18F3673E6457E0423E91E040041E020E0CF044FE9020FA0D20F71D20F20E13247C1E83FE6228061EC1FE6420067BE20C3E87E2F04290E0421520D205204F162018CB4F0604C5CB1117C1CB11170520F522232223C9CEED6666CC0D000B03730083000C000D0008111F8889000EDCCC6EE6DDDDD999BBBB67636E0EECCCDDDC999FBBB9333E3C42B9A5B9A5423C21040111A8001A13BE20FE237DFE3420F506197886230520FB8620FE3E01E050";

        public static string LogoString = @"CEED6666CC0D000B03730083000C000D0008111F8889000EDCCC6EE6DDDDD999BBBB67636E0EECCCDDDC999FBBB9333E";

        public static byte[] Bytes = StringToByteArray(HexString);

        public static byte[] Logo = StringToByteArray(LogoString);

        public static int SizeInBytes = HexString.Length / 2;

        public static byte[] StringToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }
    }
}