namespace PlayWithMaps;

public class FilterLocation : IFilterLocation
{
    List<Location> oldLocationList = new();
    List<Location> noAccuracyLocationList = new();
    List<Location> inaccurateLocationList = new();
    List<Location> kalmanNGLocationList = new();
    
    float currentSpeed = 0.0f; // meters/second

    private KalmanLatLong kalmanFilter = new KalmanLatLong(3);
    long runStartTimeInMillis;
    
    public Location FilterAndReturnLocation(Location location)
    {
        try
        {
            
            var age = GetLocationAge(location);
        
            if(age > 10 * 1000) //more than 10 seconds
            {
                Console.WriteLine("Location is old");
                oldLocationList.Add(location);
                return null;
            }

            if(location.Accuracy <= 0)
            {
                Console.WriteLine( "Latitidue and longitude values are invalid.");
                noAccuracyLocationList.Add(location);
                return null;
            }

//setAccuracy(newLocation.getAccuracy());
            var horizontalAccuracy = location.Accuracy;
            if(horizontalAccuracy > 100)
            {
                Console.WriteLine("Accuracy is too low.");
                inaccurateLocationList.Add(location);
                return null;
            }

/* Kalman Filter */
            float Qvalue;

            long locationTimeInMillis = (long)(location.Timestamp.ToUnixTimeMilliseconds());
            long elapsedTimeInMillis = locationTimeInMillis - runStartTimeInMillis;

            if(currentSpeed == 0.0f)
            {
                Qvalue = 3.0f; //3 meters per second
            }
            else
            {
                Qvalue = currentSpeed; // meters per second
            }

            kalmanFilter.Process(location.Latitude, location.Longitude, (float)location.Accuracy, elapsedTimeInMillis, Qvalue);
            double predictedLat = kalmanFilter.Lat;
            double predictedLng = kalmanFilter.Lng;

            Location predictedLocation = new Location();
            predictedLocation.Latitude = predictedLat;
            predictedLocation.Longitude = predictedLng;
            double predictedDeltaInMeters = location.CalculateDistance(predictedLocation, DistanceUnits.Kilometers) * 1000;

            if(predictedDeltaInMeters > 60)
            {
                Console.WriteLine("Kalman Filter detects mal GPS, we should probably remove this from track");
                kalmanFilter.consecutiveRejectCount += 1;

                if(kalmanFilter.consecutiveRejectCount > 3)
                {
                    kalmanFilter = new KalmanLatLong(3); //reset Kalman Filter if it rejects more than 3 times in raw.
                }

                kalmanNGLocationList.Add(location);
                return null;
            }
            else
            {
                kalmanFilter.consecutiveRejectCount = 0;
            }

            Console.WriteLine("Location quality is good enough.");
            currentSpeed = (float)location.Speed;

            return location;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }  
    
    private long GetLocationAge(Location location)
    {
        DateTimeOffset currentTime = DateTimeOffset.UtcNow;
        DateTimeOffset locationTime = location.Timestamp;
        TimeSpan age = currentTime - locationTime;
        return (long) age.TotalMilliseconds;
    }

}

public interface IFilterLocation
{
    Location FilterAndReturnLocation(Location location);
}