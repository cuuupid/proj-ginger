.class public final Landroidx/core/view/DisplayCompat$ModeCompat;
.super Ljava/lang/Object;
.source "DisplayCompat.java"


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Landroidx/core/view/DisplayCompat;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x19
    name = "ModeCompat"
.end annotation


# instance fields
.field private final mIsNative:Z

.field private final mMode:Landroid/view/Display$Mode;

.field private final mPhysicalSize:Landroid/graphics/Point;


# direct methods
.method constructor <init>(Landroid/graphics/Point;)V
    .locals 1
    .annotation system Ldalvik/annotation/MethodParameters;
        accessFlags = {
            0x0
        }
        names = {
            "physicalSize"
        }
    .end annotation

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    const-string v0, "physicalSize == null"

    invoke-static {p1, v0}, Landroidx/core/util/Preconditions;->checkNotNull(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;

    iput-object p1, p0, Landroidx/core/view/DisplayCompat$ModeCompat;->mPhysicalSize:Landroid/graphics/Point;

    const/4 p1, 0x0

    iput-object p1, p0, Landroidx/core/view/DisplayCompat$ModeCompat;->mMode:Landroid/view/Display$Mode;

    const/4 p1, 0x1

    iput-boolean p1, p0, Landroidx/core/view/DisplayCompat$ModeCompat;->mIsNative:Z

    return-void
.end method

.method constructor <init>(Landroid/view/Display$Mode;Landroid/graphics/Point;)V
    .locals 1
    .annotation system Ldalvik/annotation/MethodParameters;
        accessFlags = {
            0x0,
            0x0
        }
        names = {
            "mode",
            "physicalSize"
        }
    .end annotation

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    const-string v0, "mode == null, can\'t wrap a null reference"

    invoke-static {p1, v0}, Landroidx/core/util/Preconditions;->checkNotNull(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;

    const-string v0, "physicalSize == null"

    invoke-static {p2, v0}, Landroidx/core/util/Preconditions;->checkNotNull(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;

    iput-object p2, p0, Landroidx/core/view/DisplayCompat$ModeCompat;->mPhysicalSize:Landroid/graphics/Point;

    iput-object p1, p0, Landroidx/core/view/DisplayCompat$ModeCompat;->mMode:Landroid/view/Display$Mode;

    const/4 p1, 0x1

    iput-boolean p1, p0, Landroidx/core/view/DisplayCompat$ModeCompat;->mIsNative:Z

    return-void
.end method

.method constructor <init>(Landroid/view/Display$Mode;Z)V
    .locals 3
    .annotation system Ldalvik/annotation/MethodParameters;
        accessFlags = {
            0x0,
            0x0
        }
        names = {
            "mode",
            "isNative"
        }
    .end annotation

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    const-string v0, "mode == null, can\'t wrap a null reference"

    invoke-static {p1, v0}, Landroidx/core/util/Preconditions;->checkNotNull(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;

    new-instance v0, Landroid/graphics/Point;

    invoke-virtual {p1}, Landroid/view/Display$Mode;->getPhysicalWidth()I

    move-result v1

    invoke-virtual {p1}, Landroid/view/Display$Mode;->getPhysicalHeight()I

    move-result v2

    invoke-direct {v0, v1, v2}, Landroid/graphics/Point;-><init>(II)V

    iput-object v0, p0, Landroidx/core/view/DisplayCompat$ModeCompat;->mPhysicalSize:Landroid/graphics/Point;

    iput-object p1, p0, Landroidx/core/view/DisplayCompat$ModeCompat;->mMode:Landroid/view/Display$Mode;

    iput-boolean p2, p0, Landroidx/core/view/DisplayCompat$ModeCompat;->mIsNative:Z

    return-void
.end method


# virtual methods
.method public getPhysicalHeight()I
    .locals 1

    iget-object v0, p0, Landroidx/core/view/DisplayCompat$ModeCompat;->mPhysicalSize:Landroid/graphics/Point;

    iget v0, v0, Landroid/graphics/Point;->y:I

    return v0
.end method

.method public getPhysicalWidth()I
    .locals 1

    iget-object v0, p0, Landroidx/core/view/DisplayCompat$ModeCompat;->mPhysicalSize:Landroid/graphics/Point;

    iget v0, v0, Landroid/graphics/Point;->x:I

    return v0
.end method

.method public isNative()Z
    .locals 1
    .annotation runtime Ljava/lang/Deprecated;
    .end annotation

    iget-boolean v0, p0, Landroidx/core/view/DisplayCompat$ModeCompat;->mIsNative:Z

    return v0
.end method

.method public toMode()Landroid/view/Display$Mode;
    .locals 1

    iget-object v0, p0, Landroidx/core/view/DisplayCompat$ModeCompat;->mMode:Landroid/view/Display$Mode;

    return-object v0
.end method
