using System;
using System.Net.Http.Headers;
using ScriptDrawer.Core.Refs.Mappers;
using ScriptDrawer.Shared;

namespace ScriptDrawer.Core.Refs;

public class UrlRef<T, TMapper> : IRef<T>
    where TMapper : struct, IMapper<Stream, T>
{
    private TMapper mapper = default;

    public UrlRef(Uri url)
    {
        Url = url;
    }

    public Uri Url { get; }

    public async Task<T> ResolveAsync(CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient
        {
            DefaultRequestHeaders =
            {
                UserAgent =
                {
                    new ProductInfoHeaderValue("ScriptDrawer", typeof(ImageUrlRef).Assembly.GetName().Version?.ToString(2)),
                },
            },
        };
        await using var stream = await httpClient.GetStreamAsync(Url, cancellationToken);
        return await mapper.MapAsync(stream, cancellationToken);
    }
}
