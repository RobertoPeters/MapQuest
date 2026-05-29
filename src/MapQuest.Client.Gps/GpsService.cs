using MapQuest.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace MapQuest.Client.Gps;

public class GpsService(): IGpsService
{
    public event EventHandler<GpsCoordinates?>? OnLocationUpdated;

    public bool? IsGpsSupported { get; private set; }
    public GpsCoordinates? CurrentLocation { get; private set; }
    public string? LastErrorMessage { get; private set; }

    private DotNetObjectReference<GpsService>? _dotNetRef;
    private bool _isTracking = false;
    private GpsCoordinates? _lastRealLocation;

    public bool IsMocked { get; private set; }

    public async Task SetupAsync(IServiceProvider serviceProvider)
    {
        var jsRuntime = serviceProvider.GetRequiredService<IJSRuntime>();
        IsGpsSupported = await jsRuntime.InvokeAsync<bool>("gpsHelper.isGeolocationAvailable");
        await StartTracking(jsRuntime);
    }

    public DateTime? ConvertTimestamp(long? timestamp)
    {
        if (timestamp == null) return null;
        try
        {
           return DateTimeOffset.FromUnixTimeMilliseconds(timestamp.Value).LocalDateTime;
        }
        catch
        {
            return null;
        }
    }

    private async Task StartTracking(IJSRuntime jsRuntime)
    {
        if (!_isTracking && IsGpsSupported == true)
        {
            LastErrorMessage = null;
            _dotNetRef = DotNetObjectReference.Create(this);
            var success = await jsRuntime.InvokeAsync<bool>("gpsHelper.startWatchingPosition", _dotNetRef);
            if (success)
            {
                _isTracking = true;
            }
            else
            {
                LastErrorMessage = "Kan GPS locatie niet bepalen.";
            }
        }
    }

    [JSInvokable]
    public async Task OnLocationReceived(GpsCoordinates location)
    {
        _lastRealLocation = location;
        if (IsMocked) return;
        CurrentLocation = location;
        LastErrorMessage = null;
        OnLocationUpdated?.Invoke(this, location);
    }

    [JSInvokable]
    public void OnLocationError(string error)
    {
        LastErrorMessage = error;
        _isTracking = false;
        if (!IsMocked)
        {
            OnLocationUpdated?.Invoke(this, null);
        }
    }

    public void SetMockLocation(double latitude, double longitude, double accuracy = 50.0)
    {
        IsMocked = true;
        CurrentLocation = new GpsCoordinates
        {
            Latitude = latitude,
            Longitude = longitude,
            Accuracy = accuracy,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
        LastErrorMessage = null;
        OnLocationUpdated?.Invoke(this, CurrentLocation);
    }

    public void DisableMock()
    {
        IsMocked = false;
        CurrentLocation = _lastRealLocation;
        OnLocationUpdated?.Invoke(this, CurrentLocation);
    }

}
