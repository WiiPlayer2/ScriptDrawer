namespace ScriptDrawer.Shared;

public interface IPipeline
{
    Type ConfigurationType { get; }

    Task ExecuteAsync(IPublisher publisher, object? configuration);
}

public interface IPipeline<in TConfiguration> : IPipeline
{
    Type IPipeline.ConfigurationType => typeof(TConfiguration);

    Task IPipeline.ExecuteAsync(IPublisher publisher, object? configuration)
        => ExecuteAsync(publisher, (TConfiguration) configuration!);

    Task ExecuteAsync(IPublisher publisher, TConfiguration configuration);
}
