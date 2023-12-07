.class public Lmono/MonoPackageManager_Resources;
.super Ljava/lang/Object;
.source "MonoPackageManager_Resources.java"


# static fields
.field public static Assemblies:[Ljava/lang/String;

.field public static Dependencies:[Ljava/lang/String;


# direct methods
.method static constructor <clinit>()V
    .locals 8

    const-string v0, "StardewValley.dll"

    const-string v1, "MonoGame.Framework.dll"

    const-string v2, "BmFont.dll"

    const-string v3, "Google.Android.Vending.Licensing.dll"

    const-string v4, "SkiaSharp.dll"

    const-string v5, "StardewValley.GameData.dll"

    const-string v6, "Xamarin.Android.Support.DocumentFile.dll"

    const-string v7, "xTile.dll"

    filled-new-array/range {v0 .. v7}, [Ljava/lang/String;

    move-result-object v0

    sput-object v0, Lmono/MonoPackageManager_Resources;->Assemblies:[Ljava/lang/String;

    const/4 v0, 0x0

    new-array v0, v0, [Ljava/lang/String;

    sput-object v0, Lmono/MonoPackageManager_Resources;->Dependencies:[Ljava/lang/String;

    return-void
.end method

.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method
