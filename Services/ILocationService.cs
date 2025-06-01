// ILocationService.cs
namespace EmergencyComm.Api.Services
{
    public interface ILocationService
    {
        double CalculateDistance(double lat1, double lon1, double lat2, double lon2);
        bool IsWithinRadius(double centerLat, double centerLon, double pointLat, double pointLon, double radiusKm);
        string GetLocationName(double latitude, double longitude);
        (double lat, double lon) GetCenterPoint(IEnumerable<(double lat, double lon)> coordinates);
        double GetOptimalRadius(IEnumerable<(double lat, double lon)> coordinates);
        bool IsInDangerZone(double latitude, double longitude, double dangerLat, double dangerLon, double dangerRadius);
        string FormatCoordinates(double latitude, double longitude);
        (double lat, double lon) ParseCoordinates(string coordinateString);
    }
}