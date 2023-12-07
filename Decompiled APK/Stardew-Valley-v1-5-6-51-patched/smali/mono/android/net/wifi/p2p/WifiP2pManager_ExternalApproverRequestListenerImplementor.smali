.class public Lmono/android/net/wifi/p2p/WifiP2pManager_ExternalApproverRequestListenerImplementor;
.super Ljava/lang/Object;
.source "WifiP2pManager_ExternalApproverRequestListenerImplementor.java"

# interfaces
.implements Lmono/android/IGCUserPeer;
.implements Landroid/net/wifi/p2p/WifiP2pManager$ExternalApproverRequestListener;


# static fields
.field public static final __md_methods:Ljava/lang/String; = "n_onAttached:(Landroid/net/MacAddress;)V:GetOnAttached_Landroid_net_MacAddress_Handler:Android.Net.Wifi.P2p.WifiP2pManager/IExternalApproverRequestListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\nn_onConnectionRequested:(ILandroid/net/wifi/p2p/WifiP2pConfig;Landroid/net/wifi/p2p/WifiP2pDevice;)V:GetOnConnectionRequested_ILandroid_net_wifi_p2p_WifiP2pConfig_Landroid_net_wifi_p2p_WifiP2pDevice_Handler:Android.Net.Wifi.P2p.WifiP2pManager/IExternalApproverRequestListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\nn_onDetached:(Landroid/net/MacAddress;I)V:GetOnDetached_Landroid_net_MacAddress_IHandler:Android.Net.Wifi.P2p.WifiP2pManager/IExternalApproverRequestListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\nn_onPinGenerated:(Landroid/net/MacAddress;Ljava/lang/String;)V:GetOnPinGenerated_Landroid_net_MacAddress_Ljava_lang_String_Handler:Android.Net.Wifi.P2p.WifiP2pManager/IExternalApproverRequestListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n"


# instance fields
.field private refList:Ljava/util/ArrayList;


# direct methods
.method static constructor <clinit>()V
    .locals 3

    const-class v0, Lmono/android/net/wifi/p2p/WifiP2pManager_ExternalApproverRequestListenerImplementor;

    const-string v1, "Android.Net.Wifi.P2p.WifiP2pManager+IExternalApproverRequestListenerImplementor, Mono.Android"

    const-string v2, "n_onAttached:(Landroid/net/MacAddress;)V:GetOnAttached_Landroid_net_MacAddress_Handler:Android.Net.Wifi.P2p.WifiP2pManager/IExternalApproverRequestListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\nn_onConnectionRequested:(ILandroid/net/wifi/p2p/WifiP2pConfig;Landroid/net/wifi/p2p/WifiP2pDevice;)V:GetOnConnectionRequested_ILandroid_net_wifi_p2p_WifiP2pConfig_Landroid_net_wifi_p2p_WifiP2pDevice_Handler:Android.Net.Wifi.P2p.WifiP2pManager/IExternalApproverRequestListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\nn_onDetached:(Landroid/net/MacAddress;I)V:GetOnDetached_Landroid_net_MacAddress_IHandler:Android.Net.Wifi.P2p.WifiP2pManager/IExternalApproverRequestListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\nn_onPinGenerated:(Landroid/net/MacAddress;Ljava/lang/String;)V:GetOnPinGenerated_Landroid_net_MacAddress_Ljava_lang_String_Handler:Android.Net.Wifi.P2p.WifiP2pManager/IExternalApproverRequestListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n"

    invoke-static {v1, v0, v2}, Lmono/android/Runtime;->register(Ljava/lang/String;Ljava/lang/Class;Ljava/lang/String;)V

    return-void
.end method

.method public constructor <init>()V
    .locals 3

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    invoke-virtual {p0}, Ljava/lang/Object;->getClass()Ljava/lang/Class;

    move-result-object v0

    const-class v1, Lmono/android/net/wifi/p2p/WifiP2pManager_ExternalApproverRequestListenerImplementor;

    if-ne v0, v1, :cond_0

    const/4 v0, 0x0

    new-array v0, v0, [Ljava/lang/Object;

    const-string v1, "Android.Net.Wifi.P2p.WifiP2pManager+IExternalApproverRequestListenerImplementor, Mono.Android"

    const-string v2, ""

    invoke-static {v1, v2, p0, v0}, Lmono/android/TypeManager;->Activate(Ljava/lang/String;Ljava/lang/String;Ljava/lang/Object;[Ljava/lang/Object;)V

    :cond_0
    return-void
.end method

.method private native n_onAttached(Landroid/net/MacAddress;)V
.end method

.method private native n_onConnectionRequested(ILandroid/net/wifi/p2p/WifiP2pConfig;Landroid/net/wifi/p2p/WifiP2pDevice;)V
.end method

.method private native n_onDetached(Landroid/net/MacAddress;I)V
.end method

.method private native n_onPinGenerated(Landroid/net/MacAddress;Ljava/lang/String;)V
.end method


# virtual methods
.method public monodroidAddReference(Ljava/lang/Object;)V
    .locals 1

    iget-object v0, p0, Lmono/android/net/wifi/p2p/WifiP2pManager_ExternalApproverRequestListenerImplementor;->refList:Ljava/util/ArrayList;

    if-nez v0, :cond_0

    new-instance v0, Ljava/util/ArrayList;

    invoke-direct {v0}, Ljava/util/ArrayList;-><init>()V

    iput-object v0, p0, Lmono/android/net/wifi/p2p/WifiP2pManager_ExternalApproverRequestListenerImplementor;->refList:Ljava/util/ArrayList;

    :cond_0
    iget-object v0, p0, Lmono/android/net/wifi/p2p/WifiP2pManager_ExternalApproverRequestListenerImplementor;->refList:Ljava/util/ArrayList;

    invoke-virtual {v0, p1}, Ljava/util/ArrayList;->add(Ljava/lang/Object;)Z

    return-void
.end method

.method public monodroidClearReferences()V
    .locals 1

    iget-object v0, p0, Lmono/android/net/wifi/p2p/WifiP2pManager_ExternalApproverRequestListenerImplementor;->refList:Ljava/util/ArrayList;

    if-eqz v0, :cond_0

    invoke-virtual {v0}, Ljava/util/ArrayList;->clear()V

    :cond_0
    return-void
.end method

.method public onAttached(Landroid/net/MacAddress;)V
    .locals 0

    invoke-direct {p0, p1}, Lmono/android/net/wifi/p2p/WifiP2pManager_ExternalApproverRequestListenerImplementor;->n_onAttached(Landroid/net/MacAddress;)V

    return-void
.end method

.method public onConnectionRequested(ILandroid/net/wifi/p2p/WifiP2pConfig;Landroid/net/wifi/p2p/WifiP2pDevice;)V
    .locals 0

    invoke-direct {p0, p1, p2, p3}, Lmono/android/net/wifi/p2p/WifiP2pManager_ExternalApproverRequestListenerImplementor;->n_onConnectionRequested(ILandroid/net/wifi/p2p/WifiP2pConfig;Landroid/net/wifi/p2p/WifiP2pDevice;)V

    return-void
.end method

.method public onDetached(Landroid/net/MacAddress;I)V
    .locals 0

    invoke-direct {p0, p1, p2}, Lmono/android/net/wifi/p2p/WifiP2pManager_ExternalApproverRequestListenerImplementor;->n_onDetached(Landroid/net/MacAddress;I)V

    return-void
.end method

.method public onPinGenerated(Landroid/net/MacAddress;Ljava/lang/String;)V
    .locals 0

    invoke-direct {p0, p1, p2}, Lmono/android/net/wifi/p2p/WifiP2pManager_ExternalApproverRequestListenerImplementor;->n_onPinGenerated(Landroid/net/MacAddress;Ljava/lang/String;)V

    return-void
.end method
