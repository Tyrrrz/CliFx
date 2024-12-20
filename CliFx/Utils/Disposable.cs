﻿using System;
using System.Collections.Generic;

namespace CliFx.Utils;

internal partial class Disposable(Action dispose) : IDisposable
{
    public void Dispose() => dispose();
}

internal partial class Disposable
{
    public static IDisposable Create(Action dispose) => new Disposable(dispose);

    public static IDisposable Merge(params IEnumerable<IDisposable> disposables) =>
        Create(() =>
        {
            foreach (var disposable in disposables)
                disposable.Dispose();
        });
}
