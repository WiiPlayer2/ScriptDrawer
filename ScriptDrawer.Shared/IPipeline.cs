namespace ScriptDrawer.Shared;

public interface IPipeline
{
    Type ConfigurationType { get; }

    Task ExecuteAsync(IPublisher publisher, object? configuration, CancellationToken cancellationToken);
}

public interface IPipeline<in TConfiguration> : IPipeline
{
    Type IPipeline.ConfigurationType => typeof(TConfiguration);

    Task IPipeline.ExecuteAsync(IPublisher publisher, object? configuration, CancellationToken cancellationToken)
        => ExecuteAsync(publisher, (TConfiguration) configuration!, cancellationToken);

    Task ExecuteAsync(IPublisher publisher, TConfiguration configuration, CancellationToken cancellationToken);
}
