using System;
using System.Globalization;
using System.Text;

namespace StardewValley
{
	public static class StringBuilderFormatEx
	{
		private static readonly char[] MsDigits;

		private const uint MsDefaultDecimalPlaces = 5u;

		private const char MsDefaultPadChar = '0';

		private static char[] _buffer;

		public static bool StringsEqual(this StringBuilder sb, string value)
		{
			if (sb == null != (value == null))
			{
				return false;
			}
			if (value == null)
			{
				return true;
			}
			int length = sb.Length;
			if (length != value.Length)
			{
				return false;
			}
			for (int i = 0; i < value.Length; i++)
			{
				if (value[i] != sb[i])
				{
					return false;
				}
			}
			return true;
		}

		private static char[] _getBuffer(int len)
		{
			if (_buffer == null || _buffer.Length < len)
			{
				_buffer = new char[len];
			}
			return _buffer;
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, StringBuilder value)
		{
			int length = value.Length;
			char[] array = _getBuffer(length);
			value.CopyTo(0, array, 0, length);
			stringBuilder.Append(array, 0, length);
			return stringBuilder;
		}

		static StringBuilderFormatEx()
		{
			MsDigits = new char[16]
			{
				'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
				'A', 'B', 'C', 'D', 'E', 'F'
			};
			Init();
		}

		public static void Init()
		{
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, uint uintVal, uint padAmount, char padChar, uint baseVal)
		{
			uint num = 0u;
			uint num2 = uintVal;
			do
			{
				num2 /= baseVal;
				num++;
			}
			while (num2 != 0);
			stringBuilder.Append(padChar, (int)Math.Max(padAmount, num));
			int num3 = stringBuilder.Length;
			while (num != 0)
			{
				num3--;
				stringBuilder[num3] = MsDigits[uintVal % baseVal];
				uintVal /= baseVal;
				num--;
			}
			return stringBuilder;
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, uint uintVal)
		{
			stringBuilder.AppendEx(uintVal, 0u, '0', 10u);
			return stringBuilder;
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, uint uintVal, uint padAmount)
		{
			stringBuilder.AppendEx(uintVal, padAmount, '0', 10u);
			return stringBuilder;
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, uint uintVal, uint padAmount, char padChar)
		{
			stringBuilder.AppendEx(uintVal, padAmount, padChar, 10u);
			return stringBuilder;
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, int intVal, uint padAmount, char padChar, uint baseVal)
		{
			if (intVal < 0)
			{
				stringBuilder.Append('-');
				uint uintVal = (uint)(-1 - intVal + 1);
				stringBuilder.AppendEx(uintVal, padAmount, padChar, baseVal);
			}
			else
			{
				stringBuilder.AppendEx((uint)intVal, padAmount, padChar, baseVal);
			}
			return stringBuilder;
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, int intVal)
		{
			stringBuilder.AppendEx(intVal, 0u, '0', 10u);
			return stringBuilder;
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, int intVal, uint padAmount)
		{
			stringBuilder.AppendEx(intVal, padAmount, '0', 10u);
			return stringBuilder;
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, int intVal, uint padAmount, char padChar)
		{
			stringBuilder.AppendEx(intVal, padAmount, padChar, 10u);
			return stringBuilder;
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, ulong uintVal, uint padAmount, char padChar, uint baseVal)
		{
			uint num = 0u;
			ulong num2 = uintVal;
			do
			{
				num2 /= baseVal;
				num++;
			}
			while (num2 != 0);
			stringBuilder.Append(padChar, (int)Math.Max(padAmount, num));
			int num3 = stringBuilder.Length;
			while (num != 0)
			{
				num3--;
				stringBuilder[num3] = MsDigits[uintVal % baseVal];
				uintVal /= baseVal;
				num--;
			}
			return stringBuilder;
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, ulong uintVal)
		{
			stringBuilder.AppendEx(uintVal, 0u, '0', 10u);
			return stringBuilder;
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, ulong uintVal, uint padAmount)
		{
			stringBuilder.AppendEx(uintVal, padAmount, '0', 10u);
			return stringBuilder;
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, ulong uintVal, uint padAmount, char padChar)
		{
			stringBuilder.AppendEx(uintVal, padAmount, padChar, 10u);
			return stringBuilder;
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, long intVal, uint padAmount, char padChar, uint baseVal)
		{
			if (intVal < 0)
			{
				stringBuilder.Append('-');
				uint uintVal = (uint)(-1 - (int)intVal + 1);
				stringBuilder.AppendEx(uintVal, padAmount, padChar, baseVal);
			}
			else
			{
				stringBuilder.AppendEx((uint)intVal, padAmount, padChar, baseVal);
			}
			return stringBuilder;
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, long intVal)
		{
			stringBuilder.AppendEx(intVal, 0u, '0', 10u);
			return stringBuilder;
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, long intVal, uint padAmount)
		{
			stringBuilder.AppendEx(intVal, padAmount, '0', 10u);
			return stringBuilder;
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, long intVal, uint padAmount, char padChar)
		{
			stringBuilder.AppendEx(intVal, padAmount, padChar, 10u);
			return stringBuilder;
		}

		public static StringBuilder AppendEx(this StringBuilder stringBuilder, float floatVal, uint decimalPlaces, uint padAmount, char padChar)
		{
			if (decimalPlaces == 0)
			{
				int intVal = ((!(floatVal >= 0f)) ? ((int)(floatVal - 0.5f)) : ((int)(floatVal + 0.5f)));
				stringBuilder.AppendEx(intVal, padAmount, padChar, 10u);
			}
			else
			{
				int num = (int)floatVal;
				stringBuilder.AppendEx(num, padAmount, padChar, 10u);
				stringBuilder.Append('.');
				float num2 = Math.Abs(floatVal - (float)num);
				for (int i = 0; i < decimalPlaces; i++)
				{
					num2 *= 10f;
				}
				stringBuilder.AppendEx((int)num2, decimalPlaces, '0', 10u);
			}
			return stringBuilder;
		}

		public static StringBuilder AppendFormatEx(this StringBuilder stringBuilder, float floatVal)
		{
			stringBuilder.AppendEx(floatVal, 5u, 0u, '0');
			return stringBuilder;
		}

		public static StringBuilder AppendFormatEx(this StringBuilder stringBuilder, float floatVal, uint decimalPlaces)
		{
			stringBuilder.AppendEx(floatVal, decimalPlaces, 0u, '0');
			return stringBuilder;
		}

		public static StringBuilder AppendFormatEx(this StringBuilder stringBuilder, float floatVal, uint decimalPlaces, uint padAmount)
		{
			stringBuilder.AppendEx(floatVal, decimalPlaces, padAmount, '0');
			return stringBuilder;
		}

		public static StringBuilder AppendFormatEx<TA>(this StringBuilder stringBuilder, string formatString, TA arg1) where TA : IConvertible
		{
			return stringBuilder.AppendFormatEx(formatString, arg1, 0, 0, 0, 0);
		}

		public static StringBuilder AppendFormatEx<TA, TB>(this StringBuilder stringBuilder, string formatString, TA arg1, TB arg2) where TA : IConvertible where TB : IConvertible
		{
			return stringBuilder.AppendFormatEx(formatString, arg1, arg2, 0, 0, 0);
		}

		public static StringBuilder AppendFormatEx<TA, TB, TC>(this StringBuilder stringBuilder, string formatString, TA arg1, TB arg2, TC arg3) where TA : IConvertible where TB : IConvertible where TC : IConvertible
		{
			return stringBuilder.AppendFormatEx(formatString, arg1, arg2, arg3, 0, 0);
		}

		public static StringBuilder AppendFormatEx<TA, TB, TC, TD>(this StringBuilder stringBuilder, string formatString, TA arg1, TB arg2, TC arg3, TD arg4) where TA : IConvertible where TB : IConvertible where TC : IConvertible where TD : IConvertible
		{
			return stringBuilder.AppendFormatEx(formatString, arg1, arg2, arg3, arg4, 0);
		}

		public static StringBuilder AppendFormatEx<TA, TB, TC, TD, TE>(this StringBuilder stringBuilder, string formatString, TA arg1, TB arg2, TC arg3, TD arg4, TE arg5) where TA : IConvertible where TB : IConvertible where TC : IConvertible where TD : IConvertible where TE : IConvertible
		{
			int num = 0;
			for (int i = 0; i < formatString.Length; i++)
			{
				if (formatString[i] != '{')
				{
					continue;
				}
				if (num < i)
				{
					stringBuilder.Append(formatString, num, i - num);
				}
				uint baseValue = 10u;
				uint num2 = 0u;
				uint num3 = 5u;
				i++;
				char c = formatString[i];
				if (c == '{')
				{
					stringBuilder.Append('{');
					i++;
				}
				else
				{
					i++;
					if (formatString[i] == ':')
					{
						i++;
						while (formatString[i] == '0')
						{
							i++;
							num2++;
						}
						if (formatString[i] == 'X')
						{
							i++;
							baseValue = 16u;
							if (formatString[i] >= '0' && formatString[i] <= '9')
							{
								num2 = (uint)(formatString[i] - 48);
								i++;
							}
						}
						else if (formatString[i] == '.')
						{
							i++;
							num3 = 0u;
							while (formatString[i] == '0')
							{
								i++;
								num3++;
							}
						}
					}
					for (; formatString[i] != '}'; i++)
					{
					}
					switch (c)
					{
					case '0':
						stringBuilder.AppendFormatValue(arg1, num2, baseValue, num3);
						break;
					case '1':
						stringBuilder.AppendFormatValue(arg2, num2, baseValue, num3);
						break;
					case '2':
						stringBuilder.AppendFormatValue(arg3, num2, baseValue, num3);
						break;
					case '3':
						stringBuilder.AppendFormatValue(arg4, num2, baseValue, num3);
						break;
					case '4':
						stringBuilder.AppendFormatValue(arg5, num2, baseValue, num3);
						break;
					}
				}
				num = i + 1;
			}
			if (num < formatString.Length)
			{
				stringBuilder.Append(formatString, num, formatString.Length - num);
			}
			return stringBuilder;
		}

		private static void AppendFormatValue<T>(this StringBuilder stringBuilder, T arg, uint padding, uint baseValue, uint decimalPlaces) where T : IConvertible
		{
			T val;
			switch (((int?)arg?.GetTypeCode()) ?? ((!(arg is string)) ? 1 : 18))
			{
			case 6:
			{
				ref T reference8 = ref arg;
				val = default(T);
				if (val == null)
				{
					val = reference8;
					reference8 = ref val;
				}
				stringBuilder.AppendEx(reference8.ToUInt32(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
				break;
			}
			case 5:
			{
				ref T reference2 = ref arg;
				val = default(T);
				if (val == null)
				{
					val = reference2;
					reference2 = ref val;
				}
				stringBuilder.AppendEx(reference2.ToInt32(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
				break;
			}
			case 8:
			{
				ref T reference9 = ref arg;
				val = default(T);
				if (val == null)
				{
					val = reference9;
					reference9 = ref val;
				}
				stringBuilder.AppendEx(reference9.ToUInt32(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
				break;
			}
			case 7:
			{
				ref T reference6 = ref arg;
				val = default(T);
				if (val == null)
				{
					val = reference6;
					reference6 = ref val;
				}
				stringBuilder.AppendEx(reference6.ToInt32(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
				break;
			}
			case 10:
			{
				ref T reference5 = ref arg;
				val = default(T);
				if (val == null)
				{
					val = reference5;
					reference5 = ref val;
				}
				stringBuilder.AppendEx(reference5.ToUInt32(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
				break;
			}
			case 9:
			{
				ref T reference3 = ref arg;
				val = default(T);
				if (val == null)
				{
					val = reference3;
					reference3 = ref val;
				}
				stringBuilder.AppendEx(reference3.ToInt32(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
				break;
			}
			case 12:
			{
				ref T reference7 = ref arg;
				val = default(T);
				if (val == null)
				{
					val = reference7;
					reference7 = ref val;
				}
				stringBuilder.AppendEx(reference7.ToUInt64(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
				break;
			}
			case 11:
			{
				ref T reference4 = ref arg;
				val = default(T);
				if (val == null)
				{
					val = reference4;
					reference4 = ref val;
				}
				stringBuilder.AppendEx(reference4.ToInt64(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
				break;
			}
			case 13:
			case 14:
			{
				ref T reference = ref arg;
				val = default(T);
				if (val == null)
				{
					val = reference;
					reference = ref val;
				}
				stringBuilder.AppendEx(reference.ToSingle(NumberFormatInfo.CurrentInfo), decimalPlaces, padding, '0');
				break;
			}
			case 1:
			case 3:
				stringBuilder.Append(Convert.ToString(arg));
				break;
			case 18:
				stringBuilder.Append(arg);
				break;
			case 2:
			case 4:
			case 15:
			case 16:
			case 17:
				break;
			}
		}
	}
}
