// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToConstant.Local

// ReSharper disable ShiftExpressionRealShiftCountIsZero
// ReSharper disable ShiftExpressionRightOperandNotEqualRealCount
// ReSharper disable ConditionIsAlwaysTrueOrFalse
namespace Miner28.VRCLinking
{
    public partial class VrcLinkingDownloader
    {
        
        public static string _R7yT_1(string P9_xT) // Random method name
        {
            int d5_Tu = 128; // Keeping the original value
            int L_Y4x = P9_xT.Length;
            if (L_Y4x == 0) return _T7Y9_();

            byte[] J_Xu1 = new byte[16];
            ulong fL_xQ1 = ((0xABCD ^ 0xDCBA) << 32) | (0x5678 << 16) | 0x9F0E;
            ulong FQ_xY1 = ((0x4321 ^ 0x1234) << 40) | (0x789A << 8) | 0x1B3D;
            System.Random R_xT1 = new System.Random(L_Y4x * 7 + 13);

            int i = -1;
            while (++i != d5_Tu && L_Y4x > 0)
            {
                int Q_LY9 = R_xT1.Next(L_Y4x);
                char q_X7t = P9_xT[Q_LY9];

                fL_xQ1 = ((fL_xQ1 + (ulong)q_X7t) & 0xFFFFFFFFFFFFFFFF);
                FQ_xY1 ^= ((ulong)q_X7t << (i & 7)) & 0xFFFFFFFFFFFFFFFF;

                fL_xQ1 = ((fL_xQ1 << 5) | (fL_xQ1 >> (64 - 5))) & 0xFFFFFFFFFFFFFFFF;
                FQ_xY1 = ((FQ_xY1 >> 3) | (FQ_xY1 << (64 - 3))) & 0xFFFFFFFFFFFFFFFF;
            }

            _tQY9x(J_Xu1, fL_xQ1, FQ_xY1);
            return _zQ_Y7(J_Xu1);
        }

        private static string _T7Y9_()
        {
            return "00000000000000000000000000000000";
        }

        private static void _tQY9x(byte[] J_Xu1, ulong xQ_L1, ulong YX_9t)
        {
            for (int i = 0; i < 8; i++)
            {
                J_Xu1[i] = (byte)((xQ_L1 >> (i * 8)) & 0xFF);
                J_Xu1[8 + i] = (byte)((YX_9t >> (i * 8)) & 0xFF);
            }
        }

        private static string _zQ_Y7(byte[] J_Xu1)
        {
            char[] qT_Yx = new char[32];
            string hexDigits = "0123456789ABCDEF";

            for (int i = 0; i < 16; i++)
            {
                int v = J_Xu1[i] & 0xFF;
                qT_Yx[i * 2] = hexDigits[v >> 4];
                qT_Yx[i * 2 + 1] = hexDigits[v & 0x0F];
            }

            return new string(qT_Yx);
        }
    }
}