namespace TUnit.TestProject.FSharp

open System.Threading.Tasks
open TUnit.Assertions
open TUnit.Core.Interfaces
open TUnit.Core
open TUnit.TestProject.FSharp.Dummy
open TUnit.Assertions.Extensions
open TUnit.Assertions.AssertionBuilders

type InitializableClass() =
    let mutable isInitialized = false
    interface IAsyncInitializer with
        member _.InitializeAsync() =
            isInitialized <- true
            Task.CompletedTask
    member _.IsInitialized = isInitialized

type ClassDataSourceDrivenTests() =
    [<Test>]
    [<ClassDataSource(typeof<SomeAsyncDisposableClass>)>]
    member this.DataSource_Class (value: SomeAsyncDisposableClass) =
        // Dummy method
        ()

    [<Test>]
    [<ClassDataSource(typeof<SomeAsyncDisposableClass>)>]
    member this.DataSource_Class_Generic (value: SomeAsyncDisposableClass) =
        // Dummy method
        ()

    [<Test>]
    [<ClassDataSource(typeof<InitializableClass>)>]
    member this.IsInitialized_With_1_ClassDataSource (class1: InitializableClass) =
        task {
            let assertion = Assert.That(class1.IsInitialized).IsTrue()
            let awaiter = assertion.GetAwaiter()
            awaiter.GetResult() |> ignore
        }

    //[<Test>]
    //[<ClassDataSource(typeof<InitializableClass>, typeof<InitializableClass>)>]
    //let IsInitialized_With_2_ClassDataSources (class1: InitializableClass, class2: InitializableClass) =
    //    task {
    //        do! Assert.That(class1.IsInitialized).IsTrue()
    //        do! Assert.That(class2.IsInitialized).IsTrue()
    //    }

    //[<Test>]
    //[<ClassDataSource(typeof<InitializableClass>, typeof<InitializableClass>, typeof<InitializableClass>)>]
    //let IsInitialized_With_3_ClassDataSources (class1: InitializableClass, class2: InitializableClass, class3: InitializableClass) =
    //    task {
    //        do! Assert.That(class1.IsInitialized).IsTrue()
    //        do! Assert.That(class2.IsInitialized).IsTrue()
    //        do! Assert.That(class3.IsInitialized).IsTrue()
    //    }

    //[<Test>]
    //[<ClassDataSource(typeof<InitializableClass>, typeof<InitializableClass>, typeof<InitializableClass>, typeof<InitializableClass>)>]
    //let IsInitialized_With_4_ClassDataSources (class1: InitializableClass, class2: InitializableClass, class3: InitializableClass, class4: InitializableClass) =
    //    task {
    //        do! Assert.That(class1.IsInitialized).IsTrue()
    //        do! Assert.That(class2.IsInitialized).IsTrue()
    //        do! Assert.That(class3.IsInitialized).IsTrue()
    //        do! Assert.That(class4.IsInitialized).IsTrue()
    //    }

    //[<Test>]
    //[<ClassDataSource(typeof<InitializableClass>, typeof<InitializableClass>, typeof<InitializableClass>, typeof<InitializableClass>, typeof<InitializableClass>)>]
    //let IsInitialized_With_5_ClassDataSources (class1: InitializableClass, class2: InitializableClass, class3: InitializableClass, class4: InitializableClass, class5: InitializableClass) =
    //    task {
    //        do! Assert.That(class1.IsInitialized).IsTrue()
    //        do! Assert.That(class2.IsInitialized).IsTrue()
    //        do! Assert.That(class3.IsInitialized).IsTrue()
    //        do! Assert.That(class4.IsInitialized).IsTrue()
    //        do! Assert.That(class5.IsInitialized).IsTrue()
    //    }