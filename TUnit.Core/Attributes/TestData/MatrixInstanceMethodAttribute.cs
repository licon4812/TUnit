﻿using System.Diagnostics.CodeAnalysis;

namespace TUnit.Core;

[AttributeUsage(AttributeTargets.Parameter)]
public class MatrixInstanceMethodAttribute
    <[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods)] TClass>
    (string methodName) : MatrixMethodAttribute<TClass>(methodName), IAccessesInstanceData where TClass : class;