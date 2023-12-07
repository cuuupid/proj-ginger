.class public final synthetic Lxamarin/google/android/play/core/splitInstall/SplitInstallStateUpdatedListenerWrapper$$ExternalSyntheticLambda0;
.super Ljava/lang/Object;
.source "D8$$SyntheticClass"

# interfaces
.implements Lcom/google/android/play/core/splitinstall/SplitInstallStateUpdatedListener;


# instance fields
.field public final synthetic f$0:Lxamarin/google/android/play/core/splitInstall/SplitInstallStateUpdatedListenerWrapper;


# direct methods
.method public synthetic constructor <init>(Lxamarin/google/android/play/core/splitInstall/SplitInstallStateUpdatedListenerWrapper;)V
    .locals 0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    iput-object p1, p0, Lxamarin/google/android/play/core/splitInstall/SplitInstallStateUpdatedListenerWrapper$$ExternalSyntheticLambda0;->f$0:Lxamarin/google/android/play/core/splitInstall/SplitInstallStateUpdatedListenerWrapper;

    return-void
.end method


# virtual methods
.method public final onStateUpdate(Ljava/lang/Object;)V
    .locals 1

    iget-object v0, p0, Lxamarin/google/android/play/core/splitInstall/SplitInstallStateUpdatedListenerWrapper$$ExternalSyntheticLambda0;->f$0:Lxamarin/google/android/play/core/splitInstall/SplitInstallStateUpdatedListenerWrapper;

    check-cast p1, Lcom/google/android/play/core/splitinstall/SplitInstallSessionState;

    invoke-virtual {v0, p1}, Lxamarin/google/android/play/core/splitInstall/SplitInstallStateUpdatedListenerWrapper;->lambda$new$0$xamarin-google-android-play-core-splitInstall-SplitInstallStateUpdatedListenerWrapper(Lcom/google/android/play/core/splitinstall/SplitInstallSessionState;)V

    return-void
.end method
