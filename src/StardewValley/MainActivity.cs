using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.V4.Provider;
using Android.Util;
using Android.Views;
using Android.Widget;
using Google.Android.Vending.Licensing;
using Java.Interop;
using Java.IO;
using Java.Lang;
using Java.Util;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using StardewValley.Mobile;

namespace StardewValley
{
	[Activity(Label = "Stardew Valley", Icon = "@mipmap/ic_launcher", Theme = "@style/Theme.Splash", MainLauncher = true, AlwaysRetainTaskState = true, LaunchMode = LaunchMode.SingleInstance, ScreenOrientation = ScreenOrientation.SensorLandscape, ConfigurationChanges = (ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.UiMode))]
	public class MainActivity : AndroidGameActivity, ILicenseCheckerCallback, IJavaObject, IDisposable, IJavaPeerable
	{
		public static MainActivity instance;

		private static bool isPaused;

		private Game1 _game1;

		private Action _callback;

		public const string Base64PublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAry4fecehDpCohQk4XhiIZX9ylIGUThWZxfN9qwvQyTh53hvnpQl/lCrjfflKoPz6gz5jJn6JI1PTnoBy/iXVx1+kbO99qBgJE2V8PS5pq+Usbeqqmqqzx4lEzhiYQ2um92v4qkldNYZFwbTODYPIMbSbaLm7eK9ZyemaRbg9ssAl4QYs0EVxzDK1DjuXilRk28WxiK3lNJTz4cT38bfs4q6Zvuk1vWUvnMqcxiugox6c/9j4zZS5C4+k+WY6mHjUMuwssjCY3G+aImWDSwnU3w9G41q8EoPvJ1049PIi7GJXErusTYZITmqfonyejmSFLPt8LHtux9AmJgFSrC3UhwIDAQAB";

		private LicenseChecker _licenseChecker;

		private ServerManagedPolicy _licensePolicy;

		public bool IsDoingStorageMigration;

		public static string LastSaveGameID { get; private set; }

		public int AndroidBuildVersion
		{
			get
			{
				Context context = Application.Context;
				PackageManager packageManager = context.PackageManager;
				PackageInfo packageInfo = packageManager.GetPackageInfo(context.PackageName, (PackageInfoFlags)0);
				return packageInfo.VersionCode;
			}
		}

		public bool HasPermissions
		{
			get
			{
				if (PackageManager.CheckPermission("android.permission.ACCESS_NETWORK_STATE", PackageName) == Permission.Granted && PackageManager.CheckPermission("android.permission.ACCESS_WIFI_STATE", PackageName) == Permission.Granted && PackageManager.CheckPermission("android.permission.INTERNET", PackageName) == Permission.Granted && PackageManager.CheckPermission("android.permission.VIBRATE", PackageName) == Permission.Granted)
				{
					return true;
				}
				return false;
			}
		}

		private string[] requiredPermissions => new string[4] { "android.permission.ACCESS_NETWORK_STATE", "android.permission.ACCESS_WIFI_STATE", "android.permission.INTERNET", "android.permission.VIBRATE" };

		private string[] deniedPermissionsArray
		{
			get
			{
				List<string> list = new List<string>();
				string[] array = requiredPermissions;
				for (int i = 0; i < array.Length; i++)
				{
					if (PackageManager.CheckPermission(array[i], PackageName) != 0)
					{
						list.Add(array[i]);
					}
				}
				return list.ToArray();
			}
		}

		protected override void OnCreate(Android.OS.Bundle bundle)
		{
			instance = this;
			Log.It("MainActivity.OnCreate");
			RequestWindowFeature(WindowFeatures.NoTitle);
			if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
			{
				Window.Attributes.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.ShortEdges;
			}
			Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
			Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
			base.OnCreate(bundle);
			CheckAppPermissions();
		}

		private void OnCreatePartTwo()
		{
			Log.It("MainActivity.OnCreatePartTwo");
			MobileDisplay.SetupDisplaySettings();
			SetPaddingForMenus();
			GameRunner gameRunner = new GameRunner();
			SetContentView((View)gameRunner.Services.GetService(typeof(View)));
			GameRunner.instance = gameRunner;
			_game1 = gameRunner.gamePtr;
			gameRunner.Run();
			DoLicenseCheck();
		}

		protected override void OnResume()
		{
			Log.It("MainActivity.OnResume");
			base.OnResume();
			if (_game1 != null)
			{
				_game1.OnAppResume();
			}
			RequestedOrientation = ScreenOrientation.SensorLandscape;
			SetImmersive();
			isPaused = false;
		}

		protected override void OnStop()
		{
			Log.It("MainActivity.OnStop");
			base.OnStop();
		}

		protected override void OnDestroy()
		{
			Log.It("MainActivity.OnDestroy");
			base.OnDestroy();
			Process.KillProcess(Process.MyPid());
		}

		public override void OnWindowFocusChanged(bool hasFocus)
		{
			base.OnWindowFocusChanged(hasFocus);
			if (hasFocus)
			{
				RequestedOrientation = ScreenOrientation.SensorLandscape;
				SetImmersive();
			}
		}

		protected override void OnPause()
		{
			Log.It("MainActivity.OnPause");
			isPaused = true;
			if (_game1 != null)
			{
				_game1.OnAppPause();
			}
			Game1.emergencyBackup();
			base.OnPause();
		}

		protected void SetImmersive()
		{
			if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
			{
				Window.DecorView.SystemUiVisibility = (StatusBarVisibility)5894;
			}
		}

		public int GetBuild()
		{
			Context context = Application.Context;
			PackageManager packageManager = context.PackageManager;
			PackageInfo packageInfo = packageManager.GetPackageInfo(context.PackageName, (PackageInfoFlags)0);
			return packageInfo.VersionCode;
		}

		public void SetPaddingForMenus()
		{
			Log.It("MainActivity.SetPaddingForMenus build:" + GetBuild());
			if (Build.VERSION.SdkInt >= BuildVersionCodes.P && Window != null && Window.DecorView != null && Window.DecorView.RootWindowInsets != null && Window.DecorView.RootWindowInsets.DisplayCutout != null)
			{
				DisplayCutout displayCutout = Window.DecorView.RootWindowInsets.DisplayCutout;
				Log.It("MainActivity.SetPaddingForMenus DisplayCutout:" + displayCutout);
				if (displayCutout.SafeInsetLeft > 0 || displayCutout.SafeInsetRight > 0)
				{
					int num = Math.Max(displayCutout.SafeInsetLeft, displayCutout.SafeInsetRight);
					Game1.xEdge = Math.Min(90, num);
					Game1.toolbarPaddingX = num;
					Log.It("MainActivity.SetPaddingForMenus CUT OUT toolbarPaddingX:" + Game1.toolbarPaddingX + ", xEdge:" + Game1.xEdge);
					return;
				}
			}
			string manufacturer = Build.Manufacturer;
			string model = Build.Model;
			DisplayMetrics displayMetrics = new DisplayMetrics();
			WindowManager.DefaultDisplay.GetRealMetrics(displayMetrics);
			if (displayMetrics.HeightPixels >= 1920 || displayMetrics.WidthPixels >= 1920)
			{
				Game1.xEdge = 20;
				Game1.toolbarPaddingX = 20;
			}
		}

		private void CheckForLastSavedGame()
		{
		}

		public void CheckToCopySaveGames()
		{
		}

		private void CopySaveGame(string saveGameID)
		{
			Log.It("MainActivity.CopySaveGame... saveGameID:" + saveGameID);
			MemoryStream memoryStream = new MemoryStream(131072);
			Stream stream = TitleContainer.OpenStream("Content/SaveGames/" + saveGameID + "/SaveGameInfo");
			stream.CopyTo(memoryStream);
			memoryStream.Seek(0L, SeekOrigin.Begin);
			byte[] buffer = memoryStream.GetBuffer();
			int num = (int)memoryStream.Length;
			stream.Close();
			memoryStream = new MemoryStream(2097152);
			stream = TitleContainer.OpenStream("Content/SaveGames/" + saveGameID + "/" + saveGameID);
			stream.CopyTo(memoryStream);
			memoryStream.Seek(0L, SeekOrigin.Begin);
			byte[] buffer2 = memoryStream.GetBuffer();
			int num2 = (int)memoryStream.Length;
			stream.Close();
		}

		public void ShowDiskFullDialogue()
		{
			Log.It("MainActivity.ShowDiskFullDialogue");
			string message = "Disk full. You need to free up some space to continue.";
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.de)
			{
				message = "Festplatte voll. Sie müssen etwas Platz schaffen, um fortzufahren.";
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es)
			{
				message = "Disco lleno. Necesitas liberar algo de espacio para continuar.";
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr)
			{
				message = "Disque plein. Vous devez libérer de l'espace pour continuer.";
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.hu)
			{
				message = "Megtelt a lemez. Szüksége van egy kis hely felszabadítására a folytatáshoz.";
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.it)
			{
				message = "Disco pieno. È necessario liberare spazio per continuare.";
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja)
			{
				message = "ディスクがいっぱいです。続行するにはスペースをいくらか解放する必要があります。";
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko)
			{
				message = "디스크 꽉 참. 계속하려면 여유 공간을 확보해야합니다.";
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt)
			{
				message = "Disco cheio. Você precisa liberar algum espaço para continuar.";
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru)
			{
				message = "Диск полон. Вам нужно освободить место, чтобы продолжить.";
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr)
			{
				message = "Disk dolu. Devam etmek için biraz alan boşaltmanız gerekiyor.";
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh)
			{
				message = "存储空间已满，请释放一些存储空间再继续。";
			}
			AlertDialog.Builder builder = new AlertDialog.Builder(this);
			builder.SetMessage(message);
			builder.SetCancelable(cancelable: false);
			builder.SetPositiveButton("OK", delegate
			{
			});
			Dialog dialog = builder.Create();
			if (!IsFinishing)
			{
				dialog.Show();
			}
		}

		private void OpenInPlayStore()
		{
			try
			{
				OpenPlayStore("market://details?id=" + PackageName);
			}
			catch (ActivityNotFoundException)
			{
				OpenPlayStore("https://play.google.com/store/apps/details?id=" + PackageName);
			}
			catch (System.Exception)
			{
			}
		}

		private void OpenPlayStore(string url)
		{
			if (_licensePolicy != null)
			{
				url = _licensePolicy.LicensingUrl;
				Log.It("MainActivity.OpenPlayStore _licensePolicy.licensingUrl:" + url);
			}
			else
			{
				Log.It("MainActivity.OpenPlayStore default url:" + url);
			}
			Intent intent = new Intent("android.intent.action.VIEW", Android.Net.Uri.Parse(url));
			intent.SetPackage("com.android.vending");
			intent.AddFlags(ActivityFlags.NewTask);
			Application.Context.StartActivity(intent);
		}

		public void PromptForPermissionsIfNecessary(Action callback = null)
		{
			Log.It("MainActivity.PromptForPermissionsIfNecessary...");
			if (HasPermissions)
			{
				if (callback != null)
				{
					Log.It("MainActivity.PromptForPermissionsIfNecessary has permissions, calling callback");
					callback();
				}
			}
			else
			{
				Log.It("MainActivity.PromptForPermissionsIfNecessary doesn't have permissions, prompt for them");
				_callback = callback;
				PromptForPermissionsWithReasonFirst();
			}
		}

		private void PromptForPermissionsWithReasonFirst()
		{
			Log.It("MainActivity.PromptForPermissionsWithReasonFirst...");
			if (!HasPermissions)
			{
				AlertDialog.Builder builder = new AlertDialog.Builder(this);
				string languageCode = Java.Util.Locale.Default.Language.Substring(0, 2);
				builder.SetMessage(PermissionMessageA(languageCode));
				builder.SetCancelable(cancelable: false);
				builder.SetPositiveButton(GetOKString(languageCode), delegate
				{
					Log.It("MainActivity.PromptForPermissionsWithReasonFirst PromptForPermissions A");
					PromptForPermissions();
				});
				Dialog dialog = builder.Create();
				if (!IsFinishing)
				{
					dialog.Show();
				}
			}
			else
			{
				Log.It("MainActivity.PromptForPermissionsWithReasonFirst PromptForPermissions B");
				PromptForPermissions();
			}
		}

		public void ShowErrorAlertDialogue(string message)
		{
			Log.It("MainActivity.ShowErrorAlertDialogue " + message);
			AlertDialog.Builder builder = new AlertDialog.Builder(this);
			string languageCode = Java.Util.Locale.Default.Language.Substring(0, 2);
			builder.SetMessage(message);
			builder.SetCancelable(cancelable: false);
			builder.SetPositiveButton(GetOKString(languageCode), delegate
			{
				Finish();
			});
			Dialog dialog = builder.Create();
			if (!IsFinishing)
			{
				dialog.Show();
			}
		}

		private void OpenAppSettingsOnPhone()
		{
			Intent intent = new Intent();
			intent.SetAction("android.settings.APPLICATION_DETAILS_SETTINGS");
			Android.Net.Uri data = Android.Net.Uri.FromParts("package", PackageName, null);
			intent.SetData(data);
			StartActivity(intent);
		}

		public void LogPermissions()
		{
			Log.It("MainActivity.LogPermissions method PackageManager: , AccessNetworkState:" + PackageManager.CheckPermission("android.permission.ACCESS_NETWORK_STATE", PackageName).ToString() + ", AccessWifiState:" + PackageManager.CheckPermission("android.permission.ACCESS_WIFI_STATE", PackageName).ToString() + ", Internet:" + PackageManager.CheckPermission("android.permission.INTERNET", PackageName).ToString() + ", Vibrate:" + PackageManager.CheckPermission("android.permission.VIBRATE", PackageName));
			if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
			{
				Log.It("MainActivity.LogPermissions: , AccessNetworkState:" + CheckSelfPermission("android.permission.ACCESS_NETWORK_STATE").ToString() + ", AccessWifiState:" + CheckSelfPermission("android.permission.ACCESS_WIFI_STATE").ToString() + ", Internet:" + CheckSelfPermission("android.permission.INTERNET").ToString() + ", Vibrate:" + CheckSelfPermission("android.permission.VIBRATE"));
			}
		}

		public void CheckAppPermissions()
		{
			LogPermissions();
			if (HasPermissions)
			{
				Log.It("MainActivity.CheckAppPermissions permissions already granted.");
				OnCreatePartTwo();
			}
			else
			{
				Log.It("MainActivity.CheckAppPermissions PromptForPermissions C");
				PromptForPermissionsWithReasonFirst();
			}
		}

		public void PromptForPermissions()
		{
			Log.It("MainActivity.PromptForPermissions requesting permissions...deniedPermissionsArray:" + deniedPermissionsArray.Length);
			string[] array = deniedPermissionsArray;
			if (array.Length != 0)
			{
				Log.It("PromptForPermissions permissionsArray:" + array.Length);
				for (int i = 0; i < array.Length; i++)
				{
					Log.It("PromptForPermissions permissionsArray[" + i + "]: " + array[i]);
				}
				RequestPermissions(array, 0);
			}
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
			Log.It("MainActivity.OnRequestPermissionsResult requestCode:" + requestCode + " len:" + permissions.Length);
			if (permissions.Length == 0)
			{
				Log.It("MainActivity.OnRequestPermissionsResult no permissions returned, RETURNING");
				return;
			}
			string languageCode = Java.Util.Locale.Default.Language.Substring(0, 2);
			int num = 0;
			if (requestCode == 0)
			{
				for (int i = 0; i < grantResults.Length; i++)
				{
					Log.It("MainActivity.OnRequestPermissionsResult permission:" + permissions[i] + ", granted:" + grantResults[i]);
					if (grantResults[i] == Permission.Granted)
					{
						num++;
					}
					else
					{
						if (grantResults[i] != Permission.Denied)
						{
							continue;
						}
						Log.It("MainActivity.OnRequestPermissionsResult PERMISSION " + permissions[i] + " DENIED!");
						try
						{
							AlertDialog.Builder builder = new AlertDialog.Builder(this);
							builder.SetCancelable(cancelable: false);
							if (ShouldShowRequestPermissionRationale(permissions[i]))
							{
								builder.SetMessage(PermissionMessageA(languageCode));
								builder.SetPositiveButton(GetOKString(languageCode), delegate
								{
									Log.It("MainActivity.OnRequestPermissionsResult PromptForPermissions D");
									PromptForPermissions();
								});
							}
							else
							{
								builder.SetMessage(PermissionMessageB(languageCode));
								builder.SetPositiveButton(GetOKString(languageCode), delegate
								{
									OpenAppSettingsOnPhone();
								});
							}
							Dialog dialog = builder.Create();
							if (!IsFinishing)
							{
								dialog.Show();
							}
							return;
						}
						catch (IllegalArgumentException)
						{
							OpenInPlayStore();
							return;
						}
					}
				}
			}
			if (num == permissions.Length)
			{
				if (_callback != null)
				{
					Log.It("MainActivity.OnRequestPermissionsResult permissions granted, calling callback");
					_callback();
					_callback = null;
					return;
				}
				Log.It("MainActivity.OnRequestPermissionsResult " + num + "/" + permissions.Length + " granted, check for licence...");
				OnCreatePartTwo();
			}
		}

		private string PermissionMessageA(string languageCode)
		{
			return languageCode switch
			{
				"de" => "Du musst die Erlaubnis zum Lesen/Schreiben auf dem externen Speicher geben, um das Spiel zu speichern und Speicherstände auf andere Plattformen übertragen zu können. Bitte gib diese Genehmigung, um spielen zu können.", 
				"es" => "Para guardar la partida y transferir partidas guardadas a y desde otras plataformas, se necesita permiso para leer/escribir en almacenamiento externo. Concede este permiso para poder jugar.", 
				"ja" => "外部機器への読み込み/書き出しの許可が、ゲームのセーブデータの保存や他プラットフォームとの双方向のデータ移行実行に必要です。プレイを続けるには許可をしてください。", 
				"pt" => "Para salvar o jogo e transferir jogos salvos entre plataformas é necessário permissão para ler/gravar em armazenamento externo. Forneça essa permissão para jogar.", 
				"ru" => "Для сохранения игры и переноса сохранений с/на другие платформы нужно разрешение на чтение-запись на внешнюю память. Дайте разрешение, чтобы начать играть.", 
				"ko" => "게임을 저장하려면 외부 저장공간에 대한 읽기/쓰기 권한이 필요합니다. 또한 저장 데이터 이전 기능을 허용해 다른 플랫폼에서 게임 진행상황을 가져올 때에도 권한이 필요합니다. 게임을 플레이하려면 권한을 허용해 주십시오.", 
				"tr" => "Oyunu kaydetmek ve kayıtları platformlardan platformlara taşımak için harici depolamada okuma/yazma izni gereklidir. Lütfen oynayabilmek için izin verin.", 
				"fr" => "Une autorisation de lecture / écriture sur un stockage externe est requise pour sauvegarder le jeu et vous permettre de transférer des sauvegardes vers et depuis d'autres plateformes. Veuillez donner l'autorisation afin de jouer.", 
				"hu" => "A játék mentéséhez, és ahhoz, hogy a különböző platformok között hordozhasd a játékmentést, engedélyezned kell a külső tárhely olvasását/írását, Kérjük, a játékhoz engedélyezd ezeket.", 
				"it" => "È necessaria l'autorizzazione a leggere/scrivere su un dispositivo di memorizzazione esterno per salvare la partita e per consentire di trasferire i salvataggi da e su altre piattaforme. Concedi l'autorizzazione per giocare.", 
				"zh" => "《星露谷物语》请求获得授权用来保存游戏数据以及访问线上功能。", 
				_ => "Read/write to external storage permission is required to save the game, and to allow to you transfer saves to and from other platforms. Please give permission in order to play.", 
			};
		}

		private string PermissionMessageB(string languageCode)
		{
			return languageCode switch
			{
				"de" => "Bitte geh in die Handy-Einstellungen > Apps > Stardew Valley > Berechtigungen und aktiviere den Speicher, um das Spiel zu spielen.", 
				"es" => "En el teléfono, ve a Ajustes > Aplicaciones > Stardew Valley > Permisos y activa Almacenamiento para jugar al juego.", 
				"ja" => "設定 > アプリ > スターデューバレー > 許可の順に開いていき、ストレージを有効にしてからゲームをプレイしましょう。", 
				"pt" => "Acesse Configurar > Aplicativos > Stardew Valley > Permissões e ative Armazenamento para jogar.", 
				"ru" => "Перейдите в меню Настройки > Приложения > Stardew Valley > Разрешения и дайте доступ к памяти, чтобы начать играть.", 
				"ko" => "휴대전화의 설정 > 어플리케이션 > 스타듀 밸리 > 권한 에서 저장공간을 활성화한 뒤 게임을 플레이해 주십시오.", 
				"tr" => "Lütfen oyunu oynayabilmek için telefonda Ayarlar > Uygulamalar > Stardew Valley > İzinler ve Depolamayı etkinleştir yapın.", 
				"fr" => "Veuillez aller dans les Paramètres du téléphone> Applications> Stardew Valley> Autorisations, puis activez Stockage pour jouer.", 
				"hu" => "Lépje be a telefonodon a Beállítások > Alkalmazások > Stardew Valley > Engedélyek menübe, majd engedélyezd a Tárhelyet a játékhoz.", 
				"it" => "Nel telefono, vai su Impostazioni > Applicazioni > Stardew Valley > Autorizzazioni e attiva Memoria archiviazione per giocare.", 
				"zh" => "可在“设置-权限隐私-按应用管理权限-星露谷物语”进行设置，并打开“电话”、“读取位置信息”、“存储”权限。", 
				_ => "Please go into phone Settings > Apps > Stardew Valley > Permissions, and enable Storage to play the game.", 
			};
		}

		private string GetOKString(string languageCode)
		{
			return languageCode switch
			{
				"de" => "OK", 
				"es" => "DE ACUERDO", 
				"ja" => "OK", 
				"pt" => "Está bem", 
				"ru" => "Хорошо", 
				"ko" => "승인", 
				"tr" => "tamam", 
				"fr" => "D'accord", 
				"hu" => "rendben", 
				"it" => "ok", 
				"zh" => "好", 
				_ => "OK", 
			};
		}

		private void DoLicenseCheck()
		{
			Log.It("MainActivity.CheckUsingServerManagedPolicy");
			byte[] array = new byte[20]
			{
				46, 106, 30, 87, 95, 51, 77, 117, 128, 103,
				57, 22, 69, 27, 36, 111, 13, 44, 88, 92
			};
			string packageName = PackageName;
			string @string = Settings.Secure.GetString(ContentResolver, "android_id");
			Log.It("MainActivity.CheckUsingServerManagedPolicy\nsalt:" + array?.ToString() + "\nappId:" + packageName + "\ndeviceId:" + @string);
			AESObfuscator obfuscator = new AESObfuscator(array, packageName, @string);
			_licensePolicy = new ServerManagedPolicy(this, obfuscator);
			_licenseChecker = new LicenseChecker(this, _licensePolicy, "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAry4fecehDpCohQk4XhiIZX9ylIGUThWZxfN9qwvQyTh53hvnpQl/lCrjfflKoPz6gz5jJn6JI1PTnoBy/iXVx1+kbO99qBgJE2V8PS5pq+Usbeqqmqqzx4lEzhiYQ2um92v4qkldNYZFwbTODYPIMbSbaLm7eK9ZyemaRbg9ssAl4QYs0EVxzDK1DjuXilRk28WxiK3lNJTz4cT38bfs4q6Zvuk1vWUvnMqcxiugox6c/9j4zZS5C4+k+WY6mHjUMuwssjCY3G+aImWDSwnU3w9G41q8EoPvJ1049PIi7GJXErusTYZITmqfonyejmSFLPt8LHtux9AmJgFSrC3UhwIDAQAB");
			_licenseChecker.CheckAccess(this);
		}

		public void Allow([GeneratedEnum] PolicyResponse response)
		{
			Log.It("MainActivity.Allow response:" + response);
		}

		public void DontAllow([GeneratedEnum] PolicyResponse response)
		{
			Log.It("MainActivity.DontAllow response:" + response);
			switch (response)
			{
			case PolicyResponse.Retry:
				WaitThenCheckForValidLicence();
				break;
			default:
				OpenInPlayStore();
				Finish();
				break;
			case PolicyResponse.Licensed:
				break;
			}
		}

		private async void WaitThenCheckForValidLicence()
		{
			await Task.Delay(TimeSpan.FromSeconds(30.0));
			DoLicenseCheck();
		}

		public void ApplicationError([GeneratedEnum] LicenseCheckerErrorCode errorCode)
		{
			Log.It("MainActivity.ApplicationError errorCode:" + errorCode);
		}

		public bool CheckStorageMigration()
		{
			Java.IO.File externalStoragePublicDirectory = Android.OS.Environment.GetExternalStoragePublicDirectory("StardewValley");
			if (externalStoragePublicDirectory == null || !Directory.Exists(externalStoragePublicDirectory.AbsolutePath))
			{
				return true;
			}
			IsDoingStorageMigration = true;
			AlertDialog.Builder builder = new AlertDialog.Builder(this);
			builder.SetTitle("Farm Migration");
			string message = Game1.content.LoadString("Strings\\UI:android_storage_migration");
			string text = Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.10992");
			string text2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11741");
			builder.SetMessage(message);
			builder.SetPositiveButton(text, delegate
			{
				ShowMigrationPicker();
			});
			builder.SetNegativeButton(text2, delegate
			{
				IsDoingStorageMigration = false;
			});
			builder.SetCancelable(cancelable: false);
			AlertDialog alertDialog = builder.Create();
			alertDialog.Show();
			return false;
		}

		private void ShowMigrationPicker()
		{
			Context context = Application.Context;
			Intent intent = new Intent("android.intent.action.OPEN_DOCUMENT_TREE");
			intent.AddFlags(ActivityFlags.GrantReadUriPermission);
			StartActivityForResult(intent, 1234);
		}

		protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
			if (requestCode != 1234)
			{
				return;
			}
			RunOnUiThread(delegate
			{
				if (resultCode == Result.Ok)
				{
					Android.Net.Uri data2 = data.Data;
					CopyGameData(data2);
				}
				else
				{
					CheckStorageMigration();
				}
			});
		}

		private void CopyGameData(Android.Net.Uri folderUri)
		{
			if (!folderUri.LastPathSegment.EndsWith(":StardewValley"))
			{
				CheckStorageMigration();
				return;
			}
			Context context = Application.Context;
			string storagePath = context.GetExternalFilesDir(null).AbsolutePath + "/Saves";
			View decorView = Window.DecorView;
			FrameLayout layout = decorView.FindViewById<FrameLayout>(16908290);
			ProgressBar progressBar = new ProgressBar(this, null, 16842874);
			FrameLayout.LayoutParams layoutParams = new FrameLayout.LayoutParams(-2, -2);
			layoutParams.Gravity = GravityFlags.Center;
			layout.AddView(progressBar, layoutParams);
			progressBar.Visibility = ViewStates.Visible;
			Window.SetFlags(WindowManagerFlags.NotTouchable, WindowManagerFlags.NotTouchable);
			Action ContinueGame = delegate
			{
				try
				{
					TitleMenu titleMenu = Game1.activeClickableMenu as TitleMenu;
					StartupPreferences startupPreferences = titleMenu.startupPreferences;
					startupPreferences.androidDoneStrorageMigration = true;
					startupPreferences.savePreferences(async: false);
				}
				catch (System.Exception)
				{
				}
				Window.ClearFlags(WindowManagerFlags.NotTouchable);
				layout.RemoveView(progressBar);
				IsDoingStorageMigration = false;
			};
			Task.Run(delegate
			{
				try
				{
					DirectoryCopy(DocumentFile.FromTreeUri(context, folderUri), storagePath);
				}
				catch (System.Exception)
				{
				}
				RunOnUiThread(ContinueGame);
			});
		}

		private static void DirectoryCopy(DocumentFile source, string dest)
		{
			if (!source.Exists())
			{
				return;
			}
			if (source.IsDirectory)
			{
				if (!Directory.Exists(dest))
				{
					Directory.CreateDirectory(dest);
				}
				DocumentFile[] array = source.ListFiles();
				DocumentFile[] array2 = array;
				foreach (DocumentFile documentFile in array2)
				{
					string text = Path.Combine(dest, documentFile.Name);
					if (!System.IO.File.Exists(text))
					{
						DirectoryCopy(documentFile, text);
					}
				}
			}
			else
			{
				Stream stream = instance.ContentResolver.OpenInputStream(source.Uri);
				FileOutputStream fileOutputStream = new FileOutputStream(dest);
				byte[] array3 = new byte[1024];
				int len;
				while ((len = stream.Read(array3)) > 0)
				{
					fileOutputStream.Write(array3, 0, len);
				}
				stream.Close();
				fileOutputStream.Close();
			}
		}
	}
}
