.class public Lxamarin/google/android/play/core/assetpacks/AssetPackStateUpdateListenerWrapper;
.super Ljava/lang/Object;
.source "AssetPackStateUpdateListenerWrapper.java"


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lxamarin/google/android/play/core/assetpacks/AssetPackStateUpdateListenerWrapper$AssetPackStateListener;
    }
.end annotation


# instance fields
.field private assetPackStateUpdatedListener:Lcom/google/android/play/core/assetpacks/AssetPackStateUpdateListener;

.field private stateUpdateListener:Lxamarin/google/android/play/core/assetpacks/AssetPackStateUpdateListenerWrapper$AssetPackStateListener;


# direct methods
.method public constructor <init>()V
    .locals 1

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    new-instance v0, Lxamarin/google/android/play/core/assetpacks/AssetPackStateUpdateListenerWrapper$$ExternalSyntheticLambda0;

    invoke-direct {v0, p0}, Lxamarin/google/android/play/core/assetpacks/AssetPackStateUpdateListenerWrapper$$ExternalSyntheticLambda0;-><init>(Lxamarin/google/android/play/core/assetpacks/AssetPackStateUpdateListenerWrapper;)V

    iput-object v0, p0, Lxamarin/google/android/play/core/assetpacks/AssetPackStateUpdateListenerWrapper;->assetPackStateUpdatedListener:Lcom/google/android/play/core/assetpacks/AssetPackStateUpdateListener;

    const/4 v0, 0x0

    iput-object v0, p0, Lxamarin/google/android/play/core/assetpacks/AssetPackStateUpdateListenerWrapper;->stateUpdateListener:Lxamarin/google/android/play/core/assetpacks/AssetPackStateUpdateListenerWrapper$AssetPackStateListener;

    return-void
.end method


# virtual methods
.method public GetListener()Lcom/google/android/play/core/assetpacks/AssetPackStateUpdateListener;
    .locals 1

    iget-object v0, p0, Lxamarin/google/android/play/core/assetpacks/AssetPackStateUpdateListenerWrapper;->assetPackStateUpdatedListener:Lcom/google/android/play/core/assetpacks/AssetPackStateUpdateListener;

    return-object v0
.end method

.method synthetic lambda$new$0$xamarin-google-android-play-core-assetpacks-AssetPackStateUpdateListenerWrapper(Lcom/google/android/play/core/assetpacks/AssetPackState;)V
    .locals 0

    invoke-virtual {p0, p1}, Lxamarin/google/android/play/core/assetpacks/AssetPackStateUpdateListenerWrapper;->onStateUpdate(Lcom/google/android/play/core/assetpacks/AssetPackState;)V

    return-void
.end method

.method onStateUpdate(Lcom/google/android/play/core/assetpacks/AssetPackState;)V
    .locals 1

    iget-object v0, p0, Lxamarin/google/android/play/core/assetpacks/AssetPackStateUpdateListenerWrapper;->stateUpdateListener:Lxamarin/google/android/play/core/assetpacks/AssetPackStateUpdateListenerWrapper$AssetPackStateListener;

    if-eqz v0, :cond_0

    invoke-interface {v0, p1}, Lxamarin/google/android/play/core/assetpacks/AssetPackStateUpdateListenerWrapper$AssetPackStateListener;->OnStateUpdate(Lcom/google/android/play/core/assetpacks/AssetPackState;)V

    :cond_0
    return-void
.end method

.method public setStateUpdateListener(Lxamarin/google/android/play/core/assetpacks/AssetPackStateUpdateListenerWrapper$AssetPackStateListener;)V
    .locals 0

    iput-object p1, p0, Lxamarin/google/android/play/core/assetpacks/AssetPackStateUpdateListenerWrapper;->stateUpdateListener:Lxamarin/google/android/play/core/assetpacks/AssetPackStateUpdateListenerWrapper$AssetPackStateListener;

    return-void
.end method
