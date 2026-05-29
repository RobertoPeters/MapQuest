namespace MapQuest.Interfaces;

public interface IGpsService
{
    event EventHandler<GpsCoordinates?>? OnLocationUpdated;
    bool? IsGpsSupported { get; }
    GpsCoordinates? CurrentLocation { get; }
    string? LastErrorMessage { get; }
    bool IsMocked { get; }
    Task SetupAsync(IServiceProvider serviceProvider);
    DateTime? ConvertTimestamp(long? timestamp);
    void SetMockLocation(double latitude, double longitude, double accuracy = 50.0);
    void DisableMock();
}
