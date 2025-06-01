// LocationService.cs
using System.Globalization;

namespace EmergencyComm.Api.Services
{
    public class LocationService : ILocationService
    {
        private readonly ILogger<LocationService> _logger;

        public LocationService(ILogger<LocationService> logger)
        {
            _logger = logger;
        }

        public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        public bool IsWithinRadius(double centerLat, double centerLon, double pointLat, double pointLon, double radiusKm)
        {
            var distance = CalculateDistance(centerLat, centerLon, pointLat, pointLon);
            return distance <= radiusKm;
        }

        public string GetLocationName(double latitude, double longitude)
        {
            return $"{latitude:F4}, {longitude:F4}";
        }

        public (double lat, double lon) GetCenterPoint(IEnumerable<(double lat, double lon)> coordinates)
        {
            if (!coordinates.Any())
                return (0, 0);

            var coordinateList = coordinates.ToList();
            var centerLat = coordinateList.Average(c => c.lat);
            var centerLon = coordinateList.Average(c => c.lon);

            return (centerLat, centerLon);
        }

        public double GetOptimalRadius(IEnumerable<(double lat, double lon)> coordinates)
        {
            if (!coordinates.Any())
                return 1.0;

            var coordinateList = coordinates.ToList();
            if (coordinateList.Count == 1)
                return 1.0;

            var center = GetCenterPoint(coordinateList);
            var maxDistance = coordinateList
                .Select(c => CalculateDistance(center.lat, center.lon, c.lat, c.lon))
                .Max();

            return maxDistance * 1.2;
        }

        public bool IsInDangerZone(double latitude, double longitude, double dangerLat, double dangerLon, double dangerRadiusMeters)
        {
            var dangerRadiusKm = dangerRadiusMeters / 1000.0;
            return IsWithinRadius(dangerLat, dangerLon, latitude, longitude, dangerRadiusKm);
        }

        public string FormatCoordinates(double latitude, double longitude)
        {
            var latDirection = latitude >= 0 ? "N" : "S";
            var lonDirection = longitude >= 0 ? "E" : "W";

            return $"{Math.Abs(latitude):F6}°{latDirection}, {Math.Abs(longitude):F6}°{lonDirection}";
        }

        public (double lat, double lon) ParseCoordinates(string coordinateString)
        {
            try
            {
                var parts = coordinateString.Split(',')
                    .Select(p => p.Trim())
                    .ToArray();

                if (parts.Length != 2)
                    throw new ArgumentException("Invalid coordinate format. Expected: 'lat,lon'");

                var lat = double.Parse(parts[0], CultureInfo.InvariantCulture);
                var lon = double.Parse(parts[1], CultureInfo.InvariantCulture);

                if (lat < -90 || lat > 90)
                    throw new ArgumentException("Latitude must be between -90 and 90 degrees");

                if (lon < -180 || lon > 180)
                    throw new ArgumentException("Longitude must be between -180 and 180 degrees");

                return (lat, lon);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing coordinates: {CoordinateString}", coordinateString);
                throw new ArgumentException($"Invalid coordinate format: {coordinateString}", ex);
            }
        }

        private static double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
    }
}