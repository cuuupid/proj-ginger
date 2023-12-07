.class public Lmono/MonoPackageManager;
.super Ljava/lang/Object;
.source "MonoPackageManager.java"


# static fields
.field static Context:Landroid/content/Context;

.field static initialized:Z

.field static lock:Ljava/lang/Object;


# direct methods
.method static constructor <clinit>()V
    .locals 1

    new-instance v0, Ljava/lang/Object;

    invoke-direct {v0}, Ljava/lang/Object;-><init>()V

    sput-object v0, Lmono/MonoPackageManager;->lock:Ljava/lang/Object;

    return-void
.end method

.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method

.method public static LoadApplication(Landroid/content/Context;Landroid/content/pm/ApplicationInfo;[Ljava/lang/String;)V
    .locals 13

    sget-object v0, Lmono/MonoPackageManager;->lock:Ljava/lang/Object;

    monitor-enter v0

    :try_start_0
    instance-of v1, p0, Landroid/app/Application;

    if-eqz v1, :cond_0

    sput-object p0, Lmono/MonoPackageManager;->Context:Landroid/content/Context;

    :cond_0
    sget-boolean v1, Lmono/MonoPackageManager;->initialized:Z

    if-nez v1, :cond_6

    new-instance v1, Landroid/content/IntentFilter;

    const-string v2, "android.intent.action.TIMEZONE_CHANGED"

    invoke-direct {v1, v2}, Landroid/content/IntentFilter;-><init>(Ljava/lang/String;)V

    new-instance v2, Lmono/android/app/NotifyTimeZoneChanges;

    invoke-direct {v2}, Lmono/android/app/NotifyTimeZoneChanges;-><init>()V

    invoke-virtual {p0, v2, v1}, Landroid/content/Context;->registerReceiver(Landroid/content/BroadcastReceiver;Landroid/content/IntentFilter;)Landroid/content/Intent;

    invoke-static {}, Ljava/util/Locale;->getDefault()Ljava/util/Locale;

    move-result-object v1

    new-instance v2, Ljava/lang/StringBuilder;

    invoke-direct {v2}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v1}, Ljava/util/Locale;->getLanguage()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    const-string v3, "-"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    invoke-virtual {v1}, Ljava/util/Locale;->getCountry()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v2, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {p0}, Landroid/content/Context;->getFilesDir()Ljava/io/File;

    move-result-object v1

    invoke-virtual {v1}, Ljava/io/File;->getAbsolutePath()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {p0}, Landroid/content/Context;->getCacheDir()Ljava/io/File;

    move-result-object v2

    invoke-virtual {v2}, Ljava/io/File;->getAbsolutePath()Ljava/lang/String;

    move-result-object v2

    invoke-static {p0}, Lmono/MonoPackageManager;->getNativeLibraryPath(Landroid/content/Context;)Ljava/lang/String;

    move-result-object v4

    invoke-virtual {p0}, Landroid/content/Context;->getClassLoader()Ljava/lang/ClassLoader;

    move-result-object v8

    invoke-static {p1}, Lmono/MonoPackageManager;->getNativeLibraryPath(Landroid/content/pm/ApplicationInfo;)Ljava/lang/String;

    move-result-object v5

    sget p0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v6, 0x1a

    if-lt p0, v6, :cond_1

    invoke-static {}, Ljava/time/OffsetDateTime;->now()Ljava/time/OffsetDateTime;

    move-result-object p0

    invoke-virtual {p0}, Ljava/time/OffsetDateTime;->getOffset()Ljava/time/ZoneOffset;

    move-result-object p0

    invoke-virtual {p0}, Ljava/time/ZoneOffset;->getTotalSeconds()I

    move-result p0

    :goto_0
    move v7, p0

    goto :goto_1

    :cond_1
    invoke-static {}, Ljava/util/Calendar;->getInstance()Ljava/util/Calendar;

    move-result-object p0

    const/16 v6, 0xf

    invoke-virtual {p0, v6}, Ljava/util/Calendar;->get(I)I

    move-result p0

    invoke-static {}, Ljava/util/Calendar;->getInstance()Ljava/util/Calendar;

    move-result-object v6

    const/16 v7, 0x10

    invoke-virtual {v6, v7}, Ljava/util/Calendar;->get(I)I

    move-result v6

    add-int/2addr p0, v6

    div-int/lit16 p0, p0, 0x3e8

    goto :goto_0

    :goto_1
    const/4 p0, 0x3

    new-array v6, p0, [Ljava/lang/String;

    const/4 p0, 0x0

    aput-object v1, v6, p0

    const/4 v1, 0x1

    aput-object v2, v6, v1

    const/4 v2, 0x2

    aput-object v4, v6, v2

    sget v2, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v4, 0x15

    if-lt v2, v4, :cond_3

    iget-object v2, p1, Landroid/content/pm/ApplicationInfo;->splitSourceDirs:[Ljava/lang/String;

    if-eqz v2, :cond_3

    iget-object p1, p1, Landroid/content/pm/ApplicationInfo;->splitSourceDirs:[Ljava/lang/String;

    array-length p1, p1

    if-le p1, v1, :cond_2

    const/4 p0, 0x1

    :cond_2
    move v12, p0

    goto :goto_2

    :cond_3
    const/4 v12, 0x0

    :goto_2
    sget-boolean p0, Lmono/android/BuildConfig;->Debug:Z

    if-eqz p0, :cond_4

    const-string p0, "xamarin-debug-app-helper"

    invoke-static {p0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    invoke-static {p2, v5, v6, v12}, Lmono/android/DebugRuntime;->init([Ljava/lang/String;Ljava/lang/String;[Ljava/lang/String;Z)V

    goto :goto_3

    :cond_4
    const-string p0, "monosgen-2.0"

    invoke-static {p0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    :goto_3
    const-string p0, "xamarin-app"

    invoke-static {p0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    sget-boolean p0, Lmono/android/BuildConfig;->DotNetRuntime:Z

    if-nez p0, :cond_5

    const-string p0, "mono-native"

    invoke-static {p0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    goto :goto_4

    :cond_5
    const-string p0, "System.Security.Cryptography.Native.Android"

    invoke-static {p0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    :goto_4
    const-string p0, "monodroid"

    invoke-static {p0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    sget-object v9, Lmono/MonoPackageManager_Resources;->Assemblies:[Ljava/lang/String;

    sget v10, Landroid/os/Build$VERSION;->SDK_INT:I

    invoke-static {}, Lmono/MonoPackageManager;->isEmulator()Z

    move-result v11

    move-object v4, p2

    invoke-static/range {v3 .. v12}, Lmono/android/Runtime;->initInternal(Ljava/lang/String;[Ljava/lang/String;Ljava/lang/String;[Ljava/lang/String;ILjava/lang/ClassLoader;[Ljava/lang/String;IZZ)V

    invoke-static {}, Lmono/android/app/ApplicationRegistration;->registerApplications()V

    sput-boolean v1, Lmono/MonoPackageManager;->initialized:Z

    :cond_6
    monitor-exit v0

    return-void

    :catchall_0
    move-exception p0

    monitor-exit v0
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    goto :goto_6

    :goto_5
    throw p0

    :goto_6
    goto :goto_5
.end method

.method public static getAssemblies()[Ljava/lang/String;
    .locals 1

    sget-object v0, Lmono/MonoPackageManager_Resources;->Assemblies:[Ljava/lang/String;

    return-object v0
.end method

.method public static getDependencies()[Ljava/lang/String;
    .locals 1

    sget-object v0, Lmono/MonoPackageManager_Resources;->Dependencies:[Ljava/lang/String;

    return-object v0
.end method

.method static getNativeLibraryPath(Landroid/content/Context;)Ljava/lang/String;
    .locals 0

    invoke-virtual {p0}, Landroid/content/Context;->getApplicationInfo()Landroid/content/pm/ApplicationInfo;

    move-result-object p0

    invoke-static {p0}, Lmono/MonoPackageManager;->getNativeLibraryPath(Landroid/content/pm/ApplicationInfo;)Ljava/lang/String;

    move-result-object p0

    return-object p0
.end method

.method static getNativeLibraryPath(Landroid/content/pm/ApplicationInfo;)Ljava/lang/String;
    .locals 2

    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v1, 0x9

    if-lt v0, v1, :cond_0

    iget-object p0, p0, Landroid/content/pm/ApplicationInfo;->nativeLibraryDir:Ljava/lang/String;

    return-object p0

    :cond_0
    new-instance v0, Ljava/lang/StringBuilder;

    invoke-direct {v0}, Ljava/lang/StringBuilder;-><init>()V

    iget-object p0, p0, Landroid/content/pm/ApplicationInfo;->dataDir:Ljava/lang/String;

    invoke-virtual {v0, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    const-string p0, "/lib"

    invoke-virtual {v0, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object p0

    return-object p0
.end method

.method static isEmulator()Z
    .locals 2

    sget-object v0, Landroid/os/Build;->HARDWARE:Ljava/lang/String;

    const-string v1, "ranchu"

    invoke-virtual {v0, v1}, Ljava/lang/String;->contains(Ljava/lang/CharSequence;)Z

    move-result v1

    if-nez v1, :cond_1

    const-string v1, "goldfish"

    invoke-virtual {v0, v1}, Ljava/lang/String;->contains(Ljava/lang/CharSequence;)Z

    move-result v0

    if-eqz v0, :cond_0

    goto :goto_0

    :cond_0
    const/4 v0, 0x0

    return v0

    :cond_1
    :goto_0
    const/4 v0, 0x1

    return v0
.end method

.method public static setContext(Landroid/content/Context;)V
    .locals 0

    return-void
.end method
