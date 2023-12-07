.class public Lxamarin/google/android/play/core/splitInstall/SplitInstallStateUpdatedListenerWrapper;
.super Ljava/lang/Object;
.source "SplitInstallStateUpdatedListenerWrapper.java"


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lxamarin/google/android/play/core/splitInstall/SplitInstallStateUpdatedListenerWrapper$StateUpdateListener;
    }
.end annotation


# instance fields
.field private splitInstallStateUpdatedListener:Lcom/google/android/play/core/splitinstall/SplitInstallStateUpdatedListener;

.field private stateUpdateListener:Lxamarin/google/android/play/core/splitInstall/SplitInstallStateUpdatedListenerWrapper$StateUpdateListener;


# direct methods
.method public constructor <init>()V
    .locals 1

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    new-instance v0, Lxamarin/google/android/play/core/splitInstall/SplitInstallStateUpdatedListenerWrapper$$ExternalSyntheticLambda0;

    invoke-direct {v0, p0}, Lxamarin/google/android/play/core/splitInstall/SplitInstallStateUpdatedListenerWrapper$$ExternalSyntheticLambda0;-><init>(Lxamarin/google/android/play/core/splitInstall/SplitInstallStateUpdatedListenerWrapper;)V

    iput-object v0, p0, Lxamarin/google/android/play/core/splitInstall/SplitInstallStateUpdatedListenerWrapper;->splitInstallStateUpdatedListener:Lcom/google/android/play/core/splitinstall/SplitInstallStateUpdatedListener;

    const/4 v0, 0x0

    iput-object v0, p0, Lxamarin/google/android/play/core/splitInstall/SplitInstallStateUpdatedListenerWrapper;->stateUpdateListener:Lxamarin/google/android/play/core/splitInstall/SplitInstallStateUpdatedListenerWrapper$StateUpdateListener;

    return-void
.end method


# virtual methods
.method public GetListener()Lcom/google/android/play/core/splitinstall/SplitInstallStateUpdatedListener;
    .locals 1

    iget-object v0, p0, Lxamarin/google/android/play/core/splitInstall/SplitInstallStateUpdatedListenerWrapper;->splitInstallStateUpdatedListener:Lcom/google/android/play/core/splitinstall/SplitInstallStateUpdatedListener;

    return-object v0
.end method

.method synthetic lambda$new$0$xamarin-google-android-play-core-splitInstall-SplitInstallStateUpdatedListenerWrapper(Lcom/google/android/play/core/splitinstall/SplitInstallSessionState;)V
    .locals 0

    invoke-virtual {p0, p1}, Lxamarin/google/android/play/core/splitInstall/SplitInstallStateUpdatedListenerWrapper;->onStateUpdate(Lcom/google/android/play/core/splitinstall/SplitInstallSessionState;)V

    return-void
.end method

.method onStateUpdate(Lcom/google/android/play/core/splitinstall/SplitInstallSessionState;)V
    .locals 1

    iget-object v0, p0, Lxamarin/google/android/play/core/splitInstall/SplitInstallStateUpdatedListenerWrapper;->stateUpdateListener:Lxamarin/google/android/play/core/splitInstall/SplitInstallStateUpdatedListenerWrapper$StateUpdateListener;

    if-eqz v0, :cond_0

    invoke-interface {v0, p1}, Lxamarin/google/android/play/core/splitInstall/SplitInstallStateUpdatedListenerWrapper$StateUpdateListener;->OnStateUpdate(Lcom/google/android/play/core/splitinstall/SplitInstallSessionState;)V

    :cond_0
    return-void
.end method

.method public setStateUpdateListener(Lxamarin/google/android/play/core/splitInstall/SplitInstallStateUpdatedListenerWrapper$StateUpdateListener;)V
    .locals 0

    iput-object p1, p0, Lxamarin/google/android/play/core/splitInstall/SplitInstallStateUpdatedListenerWrapper;->stateUpdateListener:Lxamarin/google/android/play/core/splitInstall/SplitInstallStateUpdatedListenerWrapper$StateUpdateListener;

    return-void
.end method
