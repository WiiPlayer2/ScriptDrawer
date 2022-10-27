using System;

namespace ScriptDrawer.Core.Refs.Mappers;

public interface IMapper<in TFrom, TTo>
{
    Task<TTo> MapAsync(TFrom from, CancellationToken cancellationToken);
}
