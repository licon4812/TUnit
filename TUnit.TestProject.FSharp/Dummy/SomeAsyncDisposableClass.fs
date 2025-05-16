namespace TUnit.TestProject.FSharp.Dummy

open System
open System.Threading.Tasks

type SomeAsyncDisposableClass() =
    let mutable isDisposed = false

    member _.IsDisposed = isDisposed
    member _.Value = 1

    interface IAsyncDisposable with
        member _.DisposeAsync() =
            isDisposed <- true
            ValueTask()

