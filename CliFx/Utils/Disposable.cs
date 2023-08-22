using System;
using System.Collections.Generic;

namespace CliFx.Utils;

internal partial class Disposable : IDisposable
{
    private readonly Action _dispose;

    public Disposable(Action dispose) => _dispose = dispose;

    public void Dispose() => _dispose();
}

internal partial class Disposable
{
    public static IDisposable Create(Action dispose) => new Disposable(dispose);

    public static IDisposable Merge(IEnumerable<IDisposable> disposables) =>
        Create(() =>
        {
            foreach (var disposable in disposables)
                disposable.Dispose();
        });

    public static IDisposable Merge(params IDisposable[] disposables) =>
        Merge((IEnumerable<IDisposable>)disposables);
}
