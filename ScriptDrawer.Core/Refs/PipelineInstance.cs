using ScriptDrawer.Shared;

namespace ScriptDrawer.Core.Refs;

public record PipelineInstance(IPipeline Pipeline, object? Configuration)
{
    public Task ExecuteAsync(IPublisher publisher, CancellationToken cancellationToken)
        => Pipeline.ExecuteAsync(publisher, Configuration, cancellationToken);
}
