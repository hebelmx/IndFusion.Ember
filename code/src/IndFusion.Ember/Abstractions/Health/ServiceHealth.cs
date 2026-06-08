using Microsoft.Extensions.Logging;

namespace IndFusion.Ember.Abstractions.Health;

/// <summary>
/// Service health monitoring implementation with real-time updates via SignalR.
/// Provides type-safe health status tracking following Railway-Oriented Programming patterns.
/// </summary>
/// <typeparam name="T">The type of health data structure.</typeparam>
public class ServiceHealth<T> : IServiceHealth<T>
{
    private readonly ILogger<ServiceHealth<T>> _logger;
    private HealthStatus _status;
    private T? _data;
    private DateTime _lastUpdated;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceHealth{T}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public ServiceHealth(ILogger<ServiceHealth<T>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _status = HealthStatus.Healthy;
        _lastUpdated = DateTime.UtcNow;
    }

    /// <inheritdoc />
    public HealthStatus Status => _status;

    /// <inheritdoc />
    public T? Data => _data;

    /// <inheritdoc />
    public DateTime LastUpdated => _lastUpdated;

    /// <inheritdoc />
    public event EventHandler<HealthStatusChangedEventArgs<T>>? HealthStatusChanged;

    /// <inheritdoc />
    public Task<Result> UpdateHealthAsync(HealthStatus status, T? data = default, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            _logger.CancelledBeforeUpdateHealth();
            return Task.FromResult(ResultExtensions.Cancelled());
        }

        try
        {
            var previousStatus = _status;
            _status = status;
            _data = data;
            _lastUpdated = DateTime.UtcNow;

            _logger.HealthStatusUpdated(previousStatus, status);

            // Raise event if status changed
            if (previousStatus != status)
            {
                var args = new HealthStatusChangedEventArgs<T>(previousStatus, status, data);
                HealthStatusChanged?.Invoke(this, args);
                _logger.HealthStatusChangedLog(previousStatus, status);
            }

            return Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            _logger.ErrorUpdatingHealth(ex);
            return Task.FromResult(Result.WithFailure($"Failed to update health status: {ex.Message}"));
        }
    }
}

/// <summary>
/// High-performance source-generated log messages for <see cref="ServiceHealth{T}"/>.
/// </summary>
internal static partial class ServiceHealthLog
{
    [LoggerMessage(EventId = 1201, Level = LogLevel.Warning, Message = "Operation cancelled before updating health status")]
    public static partial void CancelledBeforeUpdateHealth(this ILogger logger);

    [LoggerMessage(EventId = 1202, Level = LogLevel.Debug, Message = "Health status updated from {PreviousStatus} to {NewStatus}")]
    public static partial void HealthStatusUpdated(this ILogger logger, HealthStatus previousStatus, HealthStatus newStatus);

    [LoggerMessage(EventId = 1203, Level = LogLevel.Information, Message = "Health status changed from {PreviousStatus} to {NewStatus}")]
    public static partial void HealthStatusChangedLog(this ILogger logger, HealthStatus previousStatus, HealthStatus newStatus);

    [LoggerMessage(EventId = 1204, Level = LogLevel.Error, Message = "Error updating health status")]
    public static partial void ErrorUpdatingHealth(this ILogger logger, Exception exception);
}

