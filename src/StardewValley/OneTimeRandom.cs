namespace StardewValley
{
	internal class OneTimeRandom
	{
		private const double shift3 = 0.125;

		private const double shift9 = 1.0 / 512.0;

		private const double shift27 = 7.450580596923828E-09;

		private const double shift53 = 1.1102230246251565E-16;

		public static ulong GetLong(ulong a, ulong b, ulong c, ulong d)
		{
			ulong num = ((a ^ ((b >> 14) | (b << 50))) + (((c >> 31) | (c << 33)) ^ ((d >> 18) | (d << 46)))) * 1911413418482053185L;
			ulong num2 = ((((a >> 30) | (a << 34)) ^ c) + (((b >> 32) | (b << 32)) ^ ((d >> 50) | (d << 14)))) * 1139072524405308145L;
			ulong num3 = ((((a >> 49) | (a << 15)) ^ ((d >> 33) | (d << 31))) + (b ^ ((c >> 48) | (c << 16)))) * 8792993707439626365L;
			ulong num4 = ((((a >> 17) | (a << 47)) ^ ((b >> 47) | (b << 17))) + (((c >> 15) | (c << 49)) ^ d)) * 1089642907432013597L;
			return (num ^ num2 ^ ((num3 >> 21) | (num3 << 43)) ^ ((num4 >> 44) | (num4 << 20))) * 2550117894111961111L + (((num >> 20) | (num << 44)) ^ ((num2 >> 41) | (num2 << 23)) ^ ((num3 >> 42) | (num3 << 22)) ^ num4) * 8786584852613159497L + (((num >> 43) | (num << 21)) ^ ((num2 >> 22) | (num2 << 42)) ^ num3 ^ ((num4 >> 23) | (num4 << 41))) * 3971056679291618767L;
		}

		public static double GetDouble(ulong a, ulong b, ulong c, ulong d)
		{
			return (double)(GetLong(a, b, c, d) >> 11) * 1.1102230246251565E-16;
		}
	}
}
