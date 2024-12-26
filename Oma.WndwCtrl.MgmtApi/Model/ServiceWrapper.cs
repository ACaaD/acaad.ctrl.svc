using System.Diagnostics.CodeAnalysis;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Model;

namespace Oma.WndwCtrl.MgmtApi.Model;


public sealed record ServiceWrapper<TService> : IDisposable, IServiceWrapper<TService>
    where TService : class, IService
{
    private readonly ILogger<ServiceWrapper<TService>> _logger;

    public ServiceWrapper(TService service, ILogger<ServiceWrapper<TService>> logger)
    {
        _logger = logger;
        Service = service;
        ServiceGuid = Guid.NewGuid();
    }

    public TService Service { get; }

    public Guid ServiceGuid { get; }
    public DateTime? StartedAt { get; private set; }
    public ServiceStatus Status { get; private set; }

    private CancellationTokenSource? CancellationTokenSource { get; set; }
    
    private Task? StartTask { get; set; }
    private Task? CompletionTask { get; set; }
    
    public Task StartAsync(CancellationToken cancelToken = default, params string[] args)
    {
        if (IsRunning())
        {
            return StartTask;
        }
        
        CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
        
        Status = ServiceStatus.Starting;
        
        StartTask = Service.StartAsync(CancellationTokenSource.Token, args: args);

        StartTask
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    _logger.LogError(t.Exception, "Could not start service.");
                    
                    Status = ServiceStatus.Crashed;
                    return;
                }

                StartedAt = DateTime.UtcNow;
                Status = ServiceStatus.Running;

                CompletionTask = Service.WaitForShutdownAsync(CancellationTokenSource.Token);
            },
            cancelToken)
            .ConfigureAwait(false);
        
        return StartTask;
    }
    
    public async Task StopAsync(CancellationToken cancelToken = default)
    {
        if (!IsRunning())
        {
            return;
        }
        
        TimeSpan stopTimeout = TimeSpan.FromSeconds(5);
        
        CancellationTokenSource ctsForceCancelAfter = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
        ctsForceCancelAfter.CancelAfter(stopTimeout);

        try
        {
            Status = ServiceStatus.Stopping;
            await CancellationTokenSource.CancelAsync();

            await Task.WhenAny(
                CompletionTask ?? Task.CompletedTask,
                Task.Delay(stopTimeout, ctsForceCancelAfter.Token)
            );
            
            ctsForceCancelAfter.Token.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
            await ForceStopAsync(cancelToken);
        }
        finally
        {
            CancellationTokenSource.Dispose();
            Status = ServiceStatus.Stopped;

            CancellationTokenSource = null;
            CompletionTask = null;
        }
    }
    
    public Task WaitForShutdownAsync(CancellationToken cancelToken = default)
        => Service?.WaitForShutdownAsync(cancelToken) ?? Task.CompletedTask;

    public Task ForceStopAsync(CancellationToken cancelToken = default) => Service.ForceStopAsync(cancelToken);

    [MemberNotNullWhen(true, nameof(CancellationTokenSource))]
    [MemberNotNullWhen(true, nameof(StartTask))]
    [MemberNotNullWhen(true, nameof(CompletionTask))]
    private bool IsRunning()
        => Status == ServiceStatus.Running && CancellationTokenSource is not null && StartTask is not null && CompletionTask is not null;
    
    public void Dispose()
    {
        CancellationTokenSource?.Dispose();
        CancellationTokenSource = null;
    }
}