.class public Lmono/android/view/accessibility/AccessibilityManager_AccessibilityServicesStateChangeListenerImplementor;
.super Ljava/lang/Object;
.source "AccessibilityManager_AccessibilityServicesStateChangeListenerImplementor.java"

# interfaces
.implements Lmono/android/IGCUserPeer;
.implements Landroid/view/accessibility/AccessibilityManager$AccessibilityServicesStateChangeListener;


# static fields
.field public static final __md_methods:Ljava/lang/String; = "n_onAccessibilityServicesStateChanged:(Landroid/view/accessibility/AccessibilityManager;)V:GetOnAccessibilityServicesStateChanged_Landroid_view_accessibility_AccessibilityManager_Handler:Android.Views.Accessibility.AccessibilityManager/IAccessibilityServicesStateChangeListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n"


# instance fields
.field private refList:Ljava/util/ArrayList;


# direct methods
.method static constructor <clinit>()V
    .locals 3

    const-class v0, Lmono/android/view/accessibility/AccessibilityManager_AccessibilityServicesStateChangeListenerImplementor;

    const-string v1, "Android.Views.Accessibility.AccessibilityManager+IAccessibilityServicesStateChangeListenerImplementor, Mono.Android"

    const-string v2, "n_onAccessibilityServicesStateChanged:(Landroid/view/accessibility/AccessibilityManager;)V:GetOnAccessibilityServicesStateChanged_Landroid_view_accessibility_AccessibilityManager_Handler:Android.Views.Accessibility.AccessibilityManager/IAccessibilityServicesStateChangeListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n"

    invoke-static {v1, v0, v2}, Lmono/android/Runtime;->register(Ljava/lang/String;Ljava/lang/Class;Ljava/lang/String;)V

    return-void
.end method

.method public constructor <init>()V
    .locals 3

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    invoke-virtual {p0}, Ljava/lang/Object;->getClass()Ljava/lang/Class;

    move-result-object v0

    const-class v1, Lmono/android/view/accessibility/AccessibilityManager_AccessibilityServicesStateChangeListenerImplementor;

    if-ne v0, v1, :cond_0

    const/4 v0, 0x0

    new-array v0, v0, [Ljava/lang/Object;

    const-string v1, "Android.Views.Accessibility.AccessibilityManager+IAccessibilityServicesStateChangeListenerImplementor, Mono.Android"

    const-string v2, ""

    invoke-static {v1, v2, p0, v0}, Lmono/android/TypeManager;->Activate(Ljava/lang/String;Ljava/lang/String;Ljava/lang/Object;[Ljava/lang/Object;)V

    :cond_0
    return-void
.end method

.method private native n_onAccessibilityServicesStateChanged(Landroid/view/accessibility/AccessibilityManager;)V
.end method


# virtual methods
.method public monodroidAddReference(Ljava/lang/Object;)V
    .locals 1

    iget-object v0, p0, Lmono/android/view/accessibility/AccessibilityManager_AccessibilityServicesStateChangeListenerImplementor;->refList:Ljava/util/ArrayList;

    if-nez v0, :cond_0

    new-instance v0, Ljava/util/ArrayList;

    invoke-direct {v0}, Ljava/util/ArrayList;-><init>()V

    iput-object v0, p0, Lmono/android/view/accessibility/AccessibilityManager_AccessibilityServicesStateChangeListenerImplementor;->refList:Ljava/util/ArrayList;

    :cond_0
    iget-object v0, p0, Lmono/android/view/accessibility/AccessibilityManager_AccessibilityServicesStateChangeListenerImplementor;->refList:Ljava/util/ArrayList;

    invoke-virtual {v0, p1}, Ljava/util/ArrayList;->add(Ljava/lang/Object;)Z

    return-void
.end method

.method public monodroidClearReferences()V
    .locals 1

    iget-object v0, p0, Lmono/android/view/accessibility/AccessibilityManager_AccessibilityServicesStateChangeListenerImplementor;->refList:Ljava/util/ArrayList;

    if-eqz v0, :cond_0

    invoke-virtual {v0}, Ljava/util/ArrayList;->clear()V

    :cond_0
    return-void
.end method

.method public onAccessibilityServicesStateChanged(Landroid/view/accessibility/AccessibilityManager;)V
    .locals 0

    invoke-direct {p0, p1}, Lmono/android/view/accessibility/AccessibilityManager_AccessibilityServicesStateChangeListenerImplementor;->n_onAccessibilityServicesStateChanged(Landroid/view/accessibility/AccessibilityManager;)V

    return-void
.end method
