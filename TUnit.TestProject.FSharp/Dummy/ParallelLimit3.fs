namespace TUnit.TestProject.FSharp.Dummy

open TUnit.Core.Interfaces

type ParallelLimit3() =
    interface IParallelLimit with
        member _.Limit = 3

