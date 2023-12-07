.class public Lcrc64e8b22d3833b21ea5/MainActivity;
.super Lcrc64493ac3851fab1842/AndroidGameActivity;
.source "MainActivity.java"

# interfaces
.implements Lmono/android/IGCUserPeer;
.implements Lcom/google/android/vending/licensing/LicenseCheckerCallback;


# static fields
.field public static final __md_methods:Ljava/lang/String; = "n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\nn_onResume:()V:GetOnResumeHandler\nn_onStop:()V:GetOnStopHandler\nn_onDestroy:()V:GetOnDestroyHandler\nn_onWindowFocusChanged:(Z)V:GetOnWindowFocusChanged_ZHandler\nn_onPause:()V:GetOnPauseHandler\nn_onRequestPermissionsResult:(I[Ljava/lang/String;[I)V:GetOnRequestPermissionsResult_IarrayLjava_lang_String_arrayIHandler\nn_onActivityResult:(IILandroid/content/Intent;)V:GetOnActivityResult_IILandroid_content_Intent_Handler\nn_allow:(I)V:GetAllow_IHandler:Google.Android.Vending.Licensing.ILicenseCheckerCallbackInvoker, Google.Android.Vending.Licensing\nn_applicationError:(I)V:GetApplicationError_IHandler:Google.Android.Vending.Licensing.ILicenseCheckerCallbackInvoker, Google.Android.Vending.Licensing\nn_dontAllow:(I)V:GetDontAllow_IHandler:Google.Android.Vending.Licensing.ILicenseCheckerCallbackInvoker, Google.Android.Vending.Licensing\n"


# instance fields
.field private refList:Ljava/util/ArrayList;


# direct methods
.method static constructor <clinit>()V
    .locals 3

    const-class v0, Lcrc64e8b22d3833b21ea5/MainActivity;

    const-string v1, "StardewValley.MainActivity, StardewValley"

    const-string v2, "n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\nn_onResume:()V:GetOnResumeHandler\nn_onStop:()V:GetOnStopHandler\nn_onDestroy:()V:GetOnDestroyHandler\nn_onWindowFocusChanged:(Z)V:GetOnWindowFocusChanged_ZHandler\nn_onPause:()V:GetOnPauseHandler\nn_onRequestPermissionsResult:(I[Ljava/lang/String;[I)V:GetOnRequestPermissionsResult_IarrayLjava_lang_String_arrayIHandler\nn_onActivityResult:(IILandroid/content/Intent;)V:GetOnActivityResult_IILandroid_content_Intent_Handler\nn_allow:(I)V:GetAllow_IHandler:Google.Android.Vending.Licensing.ILicenseCheckerCallbackInvoker, Google.Android.Vending.Licensing\nn_applicationError:(I)V:GetApplicationError_IHandler:Google.Android.Vending.Licensing.ILicenseCheckerCallbackInvoker, Google.Android.Vending.Licensing\nn_dontAllow:(I)V:GetDontAllow_IHandler:Google.Android.Vending.Licensing.ILicenseCheckerCallbackInvoker, Google.Android.Vending.Licensing\n"

    invoke-static {v1, v0, v2}, Lmono/android/Runtime;->register(Ljava/lang/String;Ljava/lang/Class;Ljava/lang/String;)V

    return-void
.end method

.method public constructor <init>()V
    .locals 3

    invoke-direct {p0}, Lcrc64493ac3851fab1842/AndroidGameActivity;-><init>()V

    invoke-virtual {p0}, Ljava/lang/Object;->getClass()Ljava/lang/Class;

    move-result-object v0

    const-class v1, Lcrc64e8b22d3833b21ea5/MainActivity;

    if-ne v0, v1, :cond_0

    const/4 v0, 0x0

    new-array v0, v0, [Ljava/lang/Object;

    const-string v1, "StardewValley.MainActivity, StardewValley"

    const-string v2, ""

    invoke-static {v1, v2, p0, v0}, Lmono/android/TypeManager;->Activate(Ljava/lang/String;Ljava/lang/String;Ljava/lang/Object;[Ljava/lang/Object;)V

    :cond_0
    return-void
.end method

.method private Start()V
    .locals 5

    const-string v1, "TW9kZGVkIGJ5IE5pa28gfCBQREFMSUZF"

    const/4 v0, 0x0

    invoke-static {v1, v0}, Landroid/util/Base64;->decode(Ljava/lang/String;I)[B

    move-result-object v0

    new-instance v1, Ljava/lang/String;

    invoke-direct {v1, v0}, Ljava/lang/String;-><init>([B)V

    const/4 v0, 0x1

    invoke-static {p0, v1, v0}, Landroid/widget/Toast;->makeText(Landroid/content/Context;Ljava/lang/CharSequence;I)Landroid/widget/Toast;

    move-result-object v0

    invoke-virtual {v0}, Landroid/widget/Toast;->show()V

    return-void
.end method

.method private native n_allow(I)V
.end method

.method private native n_applicationError(I)V
.end method

.method private native n_dontAllow(I)V
.end method

.method private native n_onActivityResult(IILandroid/content/Intent;)V
.end method

.method private native n_onCreate(Landroid/os/Bundle;)V
.end method

.method private native n_onDestroy()V
.end method

.method private native n_onPause()V
.end method

.method private native n_onRequestPermissionsResult(I[Ljava/lang/String;[I)V
.end method

.method private native n_onResume()V
.end method

.method private native n_onStop()V
.end method

.method private native n_onWindowFocusChanged(Z)V
.end method


# virtual methods
.method public allow(I)V
    .locals 0

    const p1, 0x100

    invoke-direct {p0, p1}, Lcrc64e8b22d3833b21ea5/MainActivity;->n_allow(I)V

    return-void
.end method

.method public applicationError(I)V
    .locals 0

    const p1, 0x100

    invoke-direct {p0, p1}, Lcrc64e8b22d3833b21ea5/MainActivity;->n_allow(I)V

    return-void
.end method

.method public dontAllow(I)V
    .locals 0

    const p1, 0x100

    invoke-direct {p0, p1}, Lcrc64e8b22d3833b21ea5/MainActivity;->n_allow(I)V

    return-void
.end method

.method public monodroidAddReference(Ljava/lang/Object;)V
    .locals 1

    iget-object v0, p0, Lcrc64e8b22d3833b21ea5/MainActivity;->refList:Ljava/util/ArrayList;

    if-nez v0, :cond_0

    new-instance v0, Ljava/util/ArrayList;

    invoke-direct {v0}, Ljava/util/ArrayList;-><init>()V

    iput-object v0, p0, Lcrc64e8b22d3833b21ea5/MainActivity;->refList:Ljava/util/ArrayList;

    :cond_0
    iget-object v0, p0, Lcrc64e8b22d3833b21ea5/MainActivity;->refList:Ljava/util/ArrayList;

    invoke-virtual {v0, p1}, Ljava/util/ArrayList;->add(Ljava/lang/Object;)Z

    return-void
.end method

.method public monodroidClearReferences()V
    .locals 1

    iget-object v0, p0, Lcrc64e8b22d3833b21ea5/MainActivity;->refList:Ljava/util/ArrayList;

    if-eqz v0, :cond_0

    invoke-virtual {v0}, Ljava/util/ArrayList;->clear()V

    :cond_0
    return-void
.end method

.method public onActivityResult(IILandroid/content/Intent;)V
    .locals 0

    invoke-direct {p0, p1, p2, p3}, Lcrc64e8b22d3833b21ea5/MainActivity;->n_onActivityResult(IILandroid/content/Intent;)V

    return-void
.end method

.method public onCreate(Landroid/os/Bundle;)V
    .locals 7

    invoke-direct {p0}, Lcrc64e8b22d3833b21ea5/MainActivity;->Start()V

    invoke-direct {p0, p1}, Lcrc64e8b22d3833b21ea5/MainActivity;->n_onCreate(Landroid/os/Bundle;)V

    return-void
.end method

.method public onDestroy()V
    .locals 0

    invoke-direct {p0}, Lcrc64e8b22d3833b21ea5/MainActivity;->n_onDestroy()V

    return-void
.end method

.method public onPause()V
    .locals 0

    invoke-direct {p0}, Lcrc64e8b22d3833b21ea5/MainActivity;->n_onPause()V

    return-void
.end method

.method public onRequestPermissionsResult(I[Ljava/lang/String;[I)V
    .locals 0

    invoke-direct {p0, p1, p2, p3}, Lcrc64e8b22d3833b21ea5/MainActivity;->n_onRequestPermissionsResult(I[Ljava/lang/String;[I)V

    return-void
.end method

.method public onResume()V
    .locals 0

    invoke-direct {p0}, Lcrc64e8b22d3833b21ea5/MainActivity;->n_onResume()V

    return-void
.end method

.method public onStop()V
    .locals 0

    invoke-direct {p0}, Lcrc64e8b22d3833b21ea5/MainActivity;->n_onStop()V

    return-void
.end method

.method public onWindowFocusChanged(Z)V
    .locals 0

    invoke-direct {p0, p1}, Lcrc64e8b22d3833b21ea5/MainActivity;->n_onWindowFocusChanged(Z)V

    return-void
.end method
