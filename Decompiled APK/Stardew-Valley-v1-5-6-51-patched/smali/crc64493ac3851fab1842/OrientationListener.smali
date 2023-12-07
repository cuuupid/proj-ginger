.class public Lcrc64493ac3851fab1842/OrientationListener;
.super Landroid/view/OrientationEventListener;
.source "OrientationListener.java"

# interfaces
.implements Lmono/android/IGCUserPeer;


# static fields
.field public static final __md_methods:Ljava/lang/String; = "n_onOrientationChanged:(I)V:GetOnOrientationChanged_IHandler\n"


# instance fields
.field private refList:Ljava/util/ArrayList;


# direct methods
.method static constructor <clinit>()V
    .locals 3

    const-class v0, Lcrc64493ac3851fab1842/OrientationListener;

    const-string v1, "Microsoft.Xna.Framework.OrientationListener, MonoGame.Framework"

    const-string v2, "n_onOrientationChanged:(I)V:GetOnOrientationChanged_IHandler\n"

    invoke-static {v1, v0, v2}, Lmono/android/Runtime;->register(Ljava/lang/String;Ljava/lang/Class;Ljava/lang/String;)V

    return-void
.end method

.method public constructor <init>(Landroid/content/Context;I)V
    .locals 2

    invoke-direct {p0, p1, p2}, Landroid/view/OrientationEventListener;-><init>(Landroid/content/Context;I)V

    invoke-virtual {p0}, Ljava/lang/Object;->getClass()Ljava/lang/Class;

    move-result-object v0

    const-class v1, Lcrc64493ac3851fab1842/OrientationListener;

    if-ne v0, v1, :cond_0

    const/4 v0, 0x2

    new-array v0, v0, [Ljava/lang/Object;

    const/4 v1, 0x0

    aput-object p1, v0, v1

    const/4 p1, 0x1

    invoke-static {p2}, Ljava/lang/Integer;->valueOf(I)Ljava/lang/Integer;

    move-result-object p2

    aput-object p2, v0, p1

    const-string p1, "Microsoft.Xna.Framework.OrientationListener, MonoGame.Framework"

    const-string p2, "Android.Content.Context, Mono.Android:Android.Hardware.SensorDelay, Mono.Android"

    invoke-static {p1, p2, p0, v0}, Lmono/android/TypeManager;->Activate(Ljava/lang/String;Ljava/lang/String;Ljava/lang/Object;[Ljava/lang/Object;)V

    :cond_0
    return-void
.end method

.method private native n_onOrientationChanged(I)V
.end method


# virtual methods
.method public monodroidAddReference(Ljava/lang/Object;)V
    .locals 1

    iget-object v0, p0, Lcrc64493ac3851fab1842/OrientationListener;->refList:Ljava/util/ArrayList;

    if-nez v0, :cond_0

    new-instance v0, Ljava/util/ArrayList;

    invoke-direct {v0}, Ljava/util/ArrayList;-><init>()V

    iput-object v0, p0, Lcrc64493ac3851fab1842/OrientationListener;->refList:Ljava/util/ArrayList;

    :cond_0
    iget-object v0, p0, Lcrc64493ac3851fab1842/OrientationListener;->refList:Ljava/util/ArrayList;

    invoke-virtual {v0, p1}, Ljava/util/ArrayList;->add(Ljava/lang/Object;)Z

    return-void
.end method

.method public monodroidClearReferences()V
    .locals 1

    iget-object v0, p0, Lcrc64493ac3851fab1842/OrientationListener;->refList:Ljava/util/ArrayList;

    if-eqz v0, :cond_0

    invoke-virtual {v0}, Ljava/util/ArrayList;->clear()V

    :cond_0
    return-void
.end method

.method public onOrientationChanged(I)V
    .locals 0

    invoke-direct {p0, p1}, Lcrc64493ac3851fab1842/OrientationListener;->n_onOrientationChanged(I)V

    return-void
.end method
