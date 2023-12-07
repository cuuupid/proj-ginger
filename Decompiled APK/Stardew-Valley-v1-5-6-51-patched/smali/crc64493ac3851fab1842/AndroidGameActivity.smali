.class public Lcrc64493ac3851fab1842/AndroidGameActivity;
.super Landroid/app/Activity;
.source "AndroidGameActivity.java"

# interfaces
.implements Lmono/android/IGCUserPeer;


# static fields
.field public static final __md_methods:Ljava/lang/String; = "n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\nn_onConfigurationChanged:(Landroid/content/res/Configuration;)V:GetOnConfigurationChanged_Landroid_content_res_Configuration_Handler\nn_onPause:()V:GetOnPauseHandler\nn_onResume:()V:GetOnResumeHandler\nn_onDestroy:()V:GetOnDestroyHandler\n"


# instance fields
.field private refList:Ljava/util/ArrayList;


# direct methods
.method static constructor <clinit>()V
    .locals 3

    const-class v0, Lcrc64493ac3851fab1842/AndroidGameActivity;

    const-string v1, "Microsoft.Xna.Framework.AndroidGameActivity, MonoGame.Framework"

    const-string v2, "n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\nn_onConfigurationChanged:(Landroid/content/res/Configuration;)V:GetOnConfigurationChanged_Landroid_content_res_Configuration_Handler\nn_onPause:()V:GetOnPauseHandler\nn_onResume:()V:GetOnResumeHandler\nn_onDestroy:()V:GetOnDestroyHandler\n"

    invoke-static {v1, v0, v2}, Lmono/android/Runtime;->register(Ljava/lang/String;Ljava/lang/Class;Ljava/lang/String;)V

    return-void
.end method

.method public constructor <init>()V
    .locals 3

    invoke-direct {p0}, Landroid/app/Activity;-><init>()V

    invoke-virtual {p0}, Ljava/lang/Object;->getClass()Ljava/lang/Class;

    move-result-object v0

    const-class v1, Lcrc64493ac3851fab1842/AndroidGameActivity;

    if-ne v0, v1, :cond_0

    const/4 v0, 0x0

    new-array v0, v0, [Ljava/lang/Object;

    const-string v1, "Microsoft.Xna.Framework.AndroidGameActivity, MonoGame.Framework"

    const-string v2, ""

    invoke-static {v1, v2, p0, v0}, Lmono/android/TypeManager;->Activate(Ljava/lang/String;Ljava/lang/String;Ljava/lang/Object;[Ljava/lang/Object;)V

    :cond_0
    return-void
.end method

.method private native n_onConfigurationChanged(Landroid/content/res/Configuration;)V
.end method

.method private native n_onCreate(Landroid/os/Bundle;)V
.end method

.method private native n_onDestroy()V
.end method

.method private native n_onPause()V
.end method

.method private native n_onResume()V
.end method


# virtual methods
.method public monodroidAddReference(Ljava/lang/Object;)V
    .locals 1

    iget-object v0, p0, Lcrc64493ac3851fab1842/AndroidGameActivity;->refList:Ljava/util/ArrayList;

    if-nez v0, :cond_0

    new-instance v0, Ljava/util/ArrayList;

    invoke-direct {v0}, Ljava/util/ArrayList;-><init>()V

    iput-object v0, p0, Lcrc64493ac3851fab1842/AndroidGameActivity;->refList:Ljava/util/ArrayList;

    :cond_0
    iget-object v0, p0, Lcrc64493ac3851fab1842/AndroidGameActivity;->refList:Ljava/util/ArrayList;

    invoke-virtual {v0, p1}, Ljava/util/ArrayList;->add(Ljava/lang/Object;)Z

    return-void
.end method

.method public monodroidClearReferences()V
    .locals 1

    iget-object v0, p0, Lcrc64493ac3851fab1842/AndroidGameActivity;->refList:Ljava/util/ArrayList;

    if-eqz v0, :cond_0

    invoke-virtual {v0}, Ljava/util/ArrayList;->clear()V

    :cond_0
    return-void
.end method

.method public onConfigurationChanged(Landroid/content/res/Configuration;)V
    .locals 0

    invoke-direct {p0, p1}, Lcrc64493ac3851fab1842/AndroidGameActivity;->n_onConfigurationChanged(Landroid/content/res/Configuration;)V

    return-void
.end method

.method public onCreate(Landroid/os/Bundle;)V
    .locals 1

    invoke-direct {p0, p1}, Lcrc64493ac3851fab1842/AndroidGameActivity;->n_onCreate(Landroid/os/Bundle;)V

    return-void
.end method

.method public onDestroy()V
    .locals 0

    invoke-direct {p0}, Lcrc64493ac3851fab1842/AndroidGameActivity;->n_onDestroy()V

    return-void
.end method

.method public onPause()V
    .locals 0

    invoke-direct {p0}, Lcrc64493ac3851fab1842/AndroidGameActivity;->n_onPause()V

    return-void
.end method

.method public onResume()V
    .locals 0

    invoke-direct {p0}, Lcrc64493ac3851fab1842/AndroidGameActivity;->n_onResume()V

    return-void
.end method
