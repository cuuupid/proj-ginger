using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.Util;
using Android.Views;

namespace StardewValley.Mobile
{
	internal class MobileDisplay
	{
		private struct MobileMetrics
		{
			public readonly MobileDeviceType Type;

			public readonly string Model;

			public readonly int PixelWidth;

			public readonly int PixelHeight;

			public readonly int Ppi;

			public readonly int PixelInset;

			public MobileMetrics(MobileDeviceType type, string model, int pixelWidth, int pixelHeight, int ppi, int pixelInset = 0)
			{
				Type = type;
				Model = model;
				PixelWidth = pixelWidth;
				PixelHeight = pixelHeight;
				Ppi = ppi;
				PixelInset = pixelInset;
			}

			public bool IsEqual(int pixelWidth, int pixelHeight)
			{
				if (PixelWidth == pixelWidth)
				{
					return PixelHeight == pixelHeight;
				}
				return false;
			}

			public bool IsModel(string model)
			{
				if (string.IsNullOrEmpty(model) || string.IsNullOrEmpty(Model))
				{
					return false;
				}
				if (Model.IndexOf(model, StringComparison.OrdinalIgnoreCase) != -1)
				{
					return true;
				}
				return false;
			}
		}

		private static readonly MobileDevice DisplayEmulation;

		private static readonly Dictionary<MobileDevice, MobileMetrics> Metrics;

		private const float MIN_TILE_HEIGHT_IN_INCHES = 0.225f;

		private const float OPTIMAL_TILE_HEIGHT_IN_INCHES = 0.3f;

		private const float MIN_VISIBLE_ROWS = 10f;

		private const float MIN_ZOOM_SCALE = 0.5f;

		private const float MAX_ZOOM_SCALE = 5f;

		private const float OPTIMAL_BUTTON_HEIGHT_IN_INCHES = 0.225f;

		public static float ZoomScale { get; private set; }

		public static float MenuButtonScale { get; private set; }

		public static int ScreenWidthPixels { get; private set; }

		public static int ScreenHeightPixels { get; private set; }

		public static bool IsiPhoneX { get; private set; }

		public static float DesktopScale { get; private set; }

		static MobileDisplay()
		{
			DisplayEmulation = MobileDevice.iPadPro129inGen6;
			Metrics = new Dictionary<MobileDevice, MobileMetrics>
			{
				{
					MobileDevice.iPhone6Plus,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone7,1", 1920, 1080, 401)
				},
				{
					MobileDevice.iPhone6,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone7,2", 1334, 750, 326)
				},
				{
					MobileDevice.iPhone6s,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone8,1", 1334, 750, 326)
				},
				{
					MobileDevice.iPhone6sPlus,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone8,2", 1920, 1080, 401)
				},
				{
					MobileDevice.iPhoneSEGen1,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone8,4", 1136, 640, 326)
				},
				{
					MobileDevice.iPhone7,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone9,1 iPhone9,3", 1334, 750, 326)
				},
				{
					MobileDevice.iPhone7Plus,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone9,2 iPhone9,4", 1920, 1080, 401)
				},
				{
					MobileDevice.iPhone8,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone10,1 iPhone10,4", 1334, 750, 326)
				},
				{
					MobileDevice.iPhone8Plus,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone10,2 iPhone10,5", 1920, 1080, 401)
				},
				{
					MobileDevice.iPhoneX,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone10,3 iPhone10,6", 2436, 1125, 458)
				},
				{
					MobileDevice.iPhoneXS,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone11,2", 2436, 1125, 458)
				},
				{
					MobileDevice.iPhoneXSMax,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone11,6", 2688, 1242, 458)
				},
				{
					MobileDevice.iPhoneXR,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone11,8", 1792, 828, 326)
				},
				{
					MobileDevice.iPhoneSEGen2,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone12,8", 1334, 750, 326)
				},
				{
					MobileDevice.iPhone11,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone12,1", 1792, 828, 326)
				},
				{
					MobileDevice.iPhone11Pro,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone12,3", 2436, 1125, 458)
				},
				{
					MobileDevice.iPhone11ProMax,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone12,5", 2688, 1242, 458)
				},
				{
					MobileDevice.iPhone12Mini,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone13,1", 2340, 1080, 476)
				},
				{
					MobileDevice.iPhone12,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone13,2", 2532, 1170, 460)
				},
				{
					MobileDevice.iPhone12Pro,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone13,3", 2532, 1170, 460)
				},
				{
					MobileDevice.iPhone12ProMax,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone13,4", 2778, 1284, 458)
				},
				{
					MobileDevice.iPhone13Pro,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone14,2", 2532, 1170, 460)
				},
				{
					MobileDevice.iPhone13ProMax,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone14,3", 2778, 1284, 458)
				},
				{
					MobileDevice.iPhone13Mini,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone14,4", 2340, 1080, 476)
				},
				{
					MobileDevice.iPhone13,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone14,5", 2532, 1170, 460)
				},
				{
					MobileDevice.iPhoneSEGen3,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone14,6", 1334, 750, 326)
				},
				{
					MobileDevice.iPhone14,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone14,7", 2532, 1170, 460)
				},
				{
					MobileDevice.iPhone14Plus,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone14,8", 2778, 1284, 458)
				},
				{
					MobileDevice.iPhone14Pro,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone15,2", 2556, 1179, 460)
				},
				{
					MobileDevice.iPhone14ProMax,
					new MobileMetrics(MobileDeviceType.iOS, "iPhone15,3", 2796, 1290, 460)
				},
				{
					MobileDevice.iPadMini1,
					new MobileMetrics(MobileDeviceType.iOS, "iPad2,5 iPad2,6 iPad2,7", 1024, 768, 163)
				},
				{
					MobileDevice.iPadMini2,
					new MobileMetrics(MobileDeviceType.iOS, "iPad4,4 iPad4,5 iPad4,6", 2048, 1536, 326)
				},
				{
					MobileDevice.iPadMini3,
					new MobileMetrics(MobileDeviceType.iOS, "iPad4,7 iPad4,8 iPad4,9", 2048, 1536, 326)
				},
				{
					MobileDevice.iPadMini4,
					new MobileMetrics(MobileDeviceType.iOS, "iPad5,1 iPad5,2", 2048, 1536, 326)
				},
				{
					MobileDevice.iPadMiniGen5,
					new MobileMetrics(MobileDeviceType.iOS, "iPad11,1 iPad11,2", 2048, 1536, 326)
				},
				{
					MobileDevice.iPadMiniGen6,
					new MobileMetrics(MobileDeviceType.iOS, "iPad14,1 iPad14,2", 2266, 1488, 326)
				},
				{
					MobileDevice.iPadGen1,
					new MobileMetrics(MobileDeviceType.iOS, "iPad1,1", 1024, 768, 132)
				},
				{
					MobileDevice.iPadGen2,
					new MobileMetrics(MobileDeviceType.iOS, "iPad2,1 iPad2,2 iPad2,3 iPad2,4", 1024, 768, 132)
				},
				{
					MobileDevice.iPadGen3,
					new MobileMetrics(MobileDeviceType.iOS, "iPad3,1 iPad3,2 iPad3,3", 2048, 1536, 264)
				},
				{
					MobileDevice.iPadGen4,
					new MobileMetrics(MobileDeviceType.iOS, "iPad3,4 iPad3,5 iPad3,6", 2048, 1536, 264)
				},
				{
					MobileDevice.iPadGen5,
					new MobileMetrics(MobileDeviceType.iOS, "iPad6,11 iPad6,12", 2048, 1536, 264)
				},
				{
					MobileDevice.iPadGen6,
					new MobileMetrics(MobileDeviceType.iOS, "iPad7,5 iPad7,6", 2048, 1536, 264)
				},
				{
					MobileDevice.iPadGen7,
					new MobileMetrics(MobileDeviceType.iOS, "iPad7,11 iPad7,12", 2160, 1620, 264)
				},
				{
					MobileDevice.iPadGen8,
					new MobileMetrics(MobileDeviceType.iOS, "iPad11,6 iPad11,7 ", 2160, 1620, 264)
				},
				{
					MobileDevice.iPadGen9,
					new MobileMetrics(MobileDeviceType.iOS, "iPad12,1 iPad12,2", 2160, 1620, 264)
				},
				{
					MobileDevice.iPadGen10,
					new MobileMetrics(MobileDeviceType.iOS, "iPad13,18 iPad13,19", 2360, 1640, 264)
				},
				{
					MobileDevice.iPadAirGen1,
					new MobileMetrics(MobileDeviceType.iOS, "iPad4,1 iPad4,2 iPad4,3", 2048, 1536, 264)
				},
				{
					MobileDevice.iPadAir2,
					new MobileMetrics(MobileDeviceType.iOS, "iPad5,3 iPad5,4", 2048, 1536, 264)
				},
				{
					MobileDevice.iPadAirGen3,
					new MobileMetrics(MobileDeviceType.iOS, "iPad11,3 iPad11,4", 2224, 1668, 264)
				},
				{
					MobileDevice.iPadAirGen4,
					new MobileMetrics(MobileDeviceType.iOS, "iPad13,1 iPad13,2", 2360, 1640, 264)
				},
				{
					MobileDevice.iPadAirGen5,
					new MobileMetrics(MobileDeviceType.iOS, "iPad13,16 iPad13,17", 2360, 1640, 264)
				},
				{
					MobileDevice.iPadPro97in,
					new MobileMetrics(MobileDeviceType.iOS, "iPad6,3 Pad6,4", 2048, 1536, 264)
				},
				{
					MobileDevice.iPadPro105in,
					new MobileMetrics(MobileDeviceType.iOS, "iPad7,3 iPad7,4", 2224, 1668, 264)
				},
				{
					MobileDevice.iPadPro11inGen1,
					new MobileMetrics(MobileDeviceType.iOS, "iPad8,1 iPad8,2 iPad8,3 iPad8,4", 2388, 1668, 264)
				},
				{
					MobileDevice.iPadPro11inGen2,
					new MobileMetrics(MobileDeviceType.iOS, "iPad8,9 iPad8,10", 2388, 1668, 264)
				},
				{
					MobileDevice.iPadPro11inGen3,
					new MobileMetrics(MobileDeviceType.iOS, "iPad13,4 iPad13,5 iPad13,6 iPad13,7", 2224, 1668, 264)
				},
				{
					MobileDevice.iPadPro11inGen4,
					new MobileMetrics(MobileDeviceType.iOS, "iPad14,3 iPad14,4", 2388, 1668, 264)
				},
				{
					MobileDevice.iPadPro129inGen1,
					new MobileMetrics(MobileDeviceType.iOS, "iPad6,7 iPad6,8", 2732, 2048, 264)
				},
				{
					MobileDevice.iPadPro129inGen2,
					new MobileMetrics(MobileDeviceType.iOS, "iPad7,1 iPad7,2", 2732, 2048, 264)
				},
				{
					MobileDevice.iPadPro129inGen3,
					new MobileMetrics(MobileDeviceType.iOS, "iPad8,5 iPad8,6 iPad8,7 iPad8,8", 2732, 2048, 264)
				},
				{
					MobileDevice.iPadPro129inGen4,
					new MobileMetrics(MobileDeviceType.iOS, "iPad8,11 iPad8,12", 2732, 2048, 264)
				},
				{
					MobileDevice.iPadPro129inGen5,
					new MobileMetrics(MobileDeviceType.iOS, "iPad13,8 iPad13,9 iPad13,10 iPad13,11", 2732, 2048, 264)
				},
				{
					MobileDevice.iPadPro129inGen6,
					new MobileMetrics(MobileDeviceType.iOS, "iPad14,5 iPad14,6", 2732, 2048, 264)
				},
				{
					MobileDevice.GooglePixel4a,
					new MobileMetrics(MobileDeviceType.Android, "", 1080, 2340, 440)
				},
				{
					MobileDevice.SamsungGalaxyTabA2016,
					new MobileMetrics(MobileDeviceType.Android, "", 1200, 1920, 224)
				},
				{
					MobileDevice.SamsungGalaxyTabS6,
					new MobileMetrics(MobileDeviceType.Android, "", 1600, 2560, 287)
				}
			};
		}

		public static void SetupDisplaySettings()
		{
			MainActivity instance = MainActivity.instance;
			Display defaultDisplay = instance.WindowManager.DefaultDisplay;
			Point point = new Point();
			defaultDisplay.GetRealSize(point);
			int x = point.X;
			int y = point.Y;
			DisplayMetrics displayMetrics = instance.Resources.DisplayMetrics;
			int ppi = Math.Max((int)displayMetrics.DensityDpi, Math.Max((int)displayMetrics.Xdpi, (int)displayMetrics.Ydpi));
			Android_SetDisplaySettings(x, y, ppi, 0);
			PrintInfo(null, x, y, ppi);
		}

		private static void SetDisplaySettings(MobileDevice device)
		{
			MobileMetrics mobileMetrics = Metrics[device];
			if (mobileMetrics.Type == MobileDeviceType.Android)
			{
				Android_SetDisplaySettings(mobileMetrics.PixelWidth, mobileMetrics.PixelHeight, mobileMetrics.Ppi, mobileMetrics.PixelInset);
			}
			else
			{
				iOS_SetDisplaySettings(mobileMetrics.Model, mobileMetrics.PixelWidth, mobileMetrics.PixelHeight, mobileMetrics.Ppi);
			}
			PrintInfo(device, mobileMetrics.PixelWidth, mobileMetrics.PixelHeight, mobileMetrics.Ppi);
		}

		private static void PrintInfo(MobileDevice? device, int pixelWidth, int pixelHeight, int ppi)
		{
			Console.WriteLine($"MobileDisplay.SetDisplaySettings {(device.HasValue ? device.Value.ToString() : string.Empty)} {pixelWidth}x{pixelHeight} {ppi} PPI\n" + $"\t\tZoomScale: {ZoomScale}\n" + $"\t\tMenuButtonScale: {MenuButtonScale}\n" + $"\t\txEdge: {Game1.xEdge}\n" + $"\t\ttoolbarPaddingX: {Game1.toolbarPaddingX}\n");
		}

		private static void CalculateZoomAndMenuScale(int width, int height, int dpi)
		{
			float num = (float)height / (float)dpi;
			float num2 = 1f;
			if (num > 5f)
			{
				num2 = 1.5f;
			}
			else if (num > 4f)
			{
				num2 = 1.25f;
			}
			float num3 = (float)dpi * 0.3f * num2;
			float num4 = (float)height / num3;
			float val = num3 / 64f;
			if (num4 < 10f)
			{
				num3 = (float)dpi * 0.225f * num2;
				val = num3 / 64f;
			}
			ZoomScale = Math.Max(0.5f, Math.Min(val, 5f));
			float num5 = (float)dpi * 0.225f * num2;
			MenuButtonScale = num5 / 64f;
			MenuButtonScale = Math.Max(0.5f, Math.Min(MenuButtonScale, 5f));
		}

		private static void EnsureLandscapeMode(ref int width, ref int height)
		{
			if (height > width)
			{
				int num = width;
				width = height;
				height = num;
			}
		}

		public static void Android_SetDisplaySettings(int width, int height, int ppi, int inset)
		{
			EnsureLandscapeMode(ref width, ref height);
			ScreenWidthPixels = width;
			ScreenHeightPixels = height;
			CalculateZoomAndMenuScale(width, height, ppi);
			if (inset >= 0)
			{
				Game1.xEdge = Math.Min(90, inset);
				Game1.toolbarPaddingX = inset;
			}
			else if (height >= 1920 || width >= 1920)
			{
				Game1.xEdge = 20;
				Game1.toolbarPaddingX = 20;
			}
		}

		private static int iOS_LookupPpi(string model)
		{
			foreach (MobileMetrics value in Metrics.Values)
			{
				if (value.Type != MobileDeviceType.Android && value.IsModel(model))
				{
					return value.Ppi;
				}
			}
			return 300;
		}

		private static bool IsDevice(string model, params MobileDevice[] devices)
		{
			foreach (MobileDevice key in devices)
			{
				if (Metrics[key].IsModel(model))
				{
					return true;
				}
			}
			return false;
		}

		public static void iOS_SetDisplaySettings(string model, int pixelWidth, int pixelHeight, int? ppi)
		{
			EnsureLandscapeMode(ref pixelWidth, ref pixelHeight);
			ScreenWidthPixels = pixelWidth;
			ScreenHeightPixels = pixelHeight;
			if (!ppi.HasValue)
			{
				ppi = iOS_LookupPpi(model);
			}
			CalculateZoomAndMenuScale(pixelWidth, pixelHeight, ppi.Value);
			if (IsDevice(model, MobileDevice.iPhoneX, MobileDevice.iPhoneXSMax))
			{
				Game1.xEdge = 64;
				Game1.toolbarPaddingX = 82;
			}
			else if (IsDevice(model, MobileDevice.iPhoneX, MobileDevice.iPhoneXR, MobileDevice.iPhoneXS, MobileDevice.iPhoneXSMax, MobileDevice.iPhone11, MobileDevice.iPhone11Pro, MobileDevice.iPhone11ProMax, MobileDevice.iPhone12, MobileDevice.iPhone12Mini, MobileDevice.iPhone12Pro, MobileDevice.iPhone12ProMax, MobileDevice.iPhone13, MobileDevice.iPhone13Mini, MobileDevice.iPhone13Pro, MobileDevice.iPhone13ProMax, MobileDevice.iPhone14, MobileDevice.iPhone14Plus))
			{
				Game1.xEdge = 64;
				Game1.toolbarPaddingX = 72;
			}
			else if (IsDevice(model, MobileDevice.iPhone14Pro, MobileDevice.iPhone14ProMax))
			{
				Game1.xEdge = 64;
				Game1.toolbarPaddingX = 82;
			}
			IsiPhoneX = IsDevice(model, MobileDevice.iPhoneX, MobileDevice.iPhoneXSMax);
		}
	}
}
