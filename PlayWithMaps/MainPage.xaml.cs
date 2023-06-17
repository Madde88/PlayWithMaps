using System.Diagnostics;
using GeolocatorPlugin;
using GeolocatorPlugin.Abstractions;
using Microsoft.Maui.Devices.Sensors;
using PlayWithMaps.CustomControls;

namespace PlayWithMaps;

public partial class MainPage : ContentPage
{
    Polyline polyline;
    Location myLocation;
    Location myLocationFilterd;
    List<Location> locations = new();
    List<CustomPin> pins = new();
    bool isGettingLocation;
    bool isReTrace;
    Stopwatch stopWatch = new();
    int PinCount;
    ImageSource customPinImage = ImageSource.FromFile("pin.png");
    public ObservableCollection<PositionRec> LocationListViewItems { get; } = new();

    private MainPageViewModel viewModel => BindingContext as MainPageViewModel;
    private IFilterLocation filterLocation;
    public MainPage(MainPageViewModel viewModel, IFilterLocation filterLocation)
    {
        InitializeComponent();
        this.filterLocation = filterLocation;
        BindingContext = viewModel;
        LocationListView.ItemsSource = LocationListViewItems;

    }
    protected async override void OnAppearing()
    {
        await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        myLocation = await GetMyLocation();
        MoveToMyLocation(myLocation);
    }

    async void RecordLocation_Clicked(System.Object sender, System.EventArgs e)
    {
        isReTrace = false;
        await TrackLocation(TraceType.Trace);
    }

    private async Task TrackLocation(TraceType traceType)
    {
        ClearMap();
        //stopWatch.Start();
        
        switch (traceType)
        {
            case TraceType.Trace:
                {
                    polyline = new Polyline
                    {
                        StrokeColor = Colors.Red,
                        StrokeWidth = 2,
                    };
                    mappy.MapElements.Add(polyline);
                    break;
                }

            default:
                {
                    polyline = new Polyline
                    {
                        StrokeColor = Colors.Blue,
                        StrokeWidth = 2,
                    };
                    mappy.MapElements.Add(polyline);
                    break;
                }
        }

        isGettingLocation = true;

        //while (isGettingLocation)
        //{
        //    await MoveToMyLocation();
        //    myLocationFilterd = await GetMyLocationWithFilter();
        //    if (myLocationFilterd != null)
        //    {
        //        locations.Add(myLocationFilterd);
        //        polyline.Add(myLocationFilterd);
        //    }

        //    await Task.Delay(2000);

        //}
        await StartListening();
    }

    async Task StartListening()
    {
        if (CrossGeolocator.Current.IsListening)
            return;

        CrossGeolocator.Current.DesiredAccuracy = 1;

        await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(5), 1, true, new GeolocatorPlugin.Abstractions.ListenerSettings
        {
            ActivityType = GeolocatorPlugin.Abstractions.ActivityType.Fitness,
            AllowBackgroundUpdates = true,
            DeferLocationUpdates = false,
            ListenForSignificantChanges = false,
            PauseLocationUpdatesAutomatically = false
        });

        CrossGeolocator.Current.PositionChanged += PositionChanged;
        CrossGeolocator.Current.PositionError += PositionError;
    }

    private void PositionChanged(object sender, PositionEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {

            //If updating the UI, ensure you invoke on main thread
            var position = e.Position;
        var location = MapLocationPosition(position);

        MoveToMyLocation(location);

        myLocationFilterd = filterLocation.FilterAndReturnLocation(location);

        if (myLocationFilterd != null)
        {
            locations.Add(location);
            polyline.Add(location);
        
        }
        });
    }

    private Location MapLocationPosition(GeolocatorPlugin.Abstractions.Position position)
    {
        var location = new Location(position.Latitude, position.Longitude);
        location.Accuracy = position.Accuracy;
        location.Timestamp = position.Timestamp;
        location.Speed = position.Speed;
        location.Accuracy = position.Accuracy;
        location.Altitude = position.Altitude;

        return location;
    }

    private void PositionError(object sender, PositionErrorEventArgs e)
    {
        Debug.WriteLine(e.Error);
        //Handle event here for errors
    }

    async Task StopListening()
    {
        if (!CrossGeolocator.Current.IsListening)
            return;

        await CrossGeolocator.Current.StopListeningAsync();

        CrossGeolocator.Current.PositionChanged -= PositionChanged;
        CrossGeolocator.Current.PositionError -= PositionError;
    }

    async void CancelGetLocation_Clicked(System.Object sender, System.EventArgs e)
    {
        isGettingLocation = false;
        //stopWatch.Stop();
        await StopListening();

        var result = await DisplayAlert("Vill Du spara?", "Vill du spara din data i databasen?", "JA", "Avbryt");
        if (result)
        {
            var name = await DisplayPromptAsync("Namn", "Döp sessionen till något", "OK");
            if (string.IsNullOrWhiteSpace(name)) return;
            try
            {
                var positionRec = await viewModel.SavePositionRec(new PositionRec { Name = name, Time = stopWatch.Elapsed, PinCount = PinCount }, isReTrace);
                foreach (var item in locations)
                {
                    var position = new Models.Position { PositionRecId = positionRec.Id, Lat = item.Latitude, Long = item.Longitude, Type = PositionType.PolyLine };
                    await viewModel.SavePosition(position);
                }

                foreach (var item in pins)
                {
                    var position = new Models.Position { PositionRecId = positionRec.Id, Lat = item.Location.Latitude, Long = item.Location.Longitude, Type = PositionType.Pin };
                    await viewModel.SavePosition(position);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }

    async void GetSavedLocations_Clicked(System.Object sender, System.EventArgs e)
    {

        var positionRecsList = await viewModel.GetAllPositionRecs();
        if (positionRecsList == null)
            return;
        
        LocationListViewItems.Clear();
        positionRecsList.ForEach(a => LocationListViewItems.Add(a));

    }

    private async Task<Location> GetMyLocation()
    {
        var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
#if IOS
        request.RequestFullAccuracy = true;
#endif
        
        myLocation = await Geolocation.GetLocationAsync(request);

        return myLocation;
    }
    
    private async Task<Location> GetMyLocationWithFilter()
    {
        var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
#if IOS
        request.RequestFullAccuracy = true;
#endif
        
        myLocation = filterLocation.FilterAndReturnLocation(await Geolocation.GetLocationAsync(request));

        return myLocation;
    }

    void Clear_Clicked(System.Object sender, System.EventArgs e)
    {
        ClearMap();
    }

    async void GetReTraceLocation_Clicked(System.Object sender, System.EventArgs e)
    {
        isReTrace = true;
        await TrackLocation(TraceType.ReTrace);
    }

    async void AddPin_Clicked(System.Object sender, System.EventArgs e)
    {
        var location = await GetMyLocation();
        var pin = new CustomPin()
        {
            Location = location,
            Label = string.Format("Accuracy: {0}", location.Accuracy),
            ImageSource = customPinImage
        };
        mappy.Pins.Add(pin);
        pins.Add(pin);
        PinCount += 1;
    }

    private void ClearMap()
    {
        mappy.MapElements.Clear();
        locations.Clear();
        pins.Clear();
        mappy.Pins.Clear();
    }

    async void GetLocation_Clicked(System.Object sender, System.EventArgs e)
    {
        var myLocation = await GetMyLocation();
        MoveToMyLocation(myLocation);
    }

    private void MoveToMyLocation(Location myLocation)
    {
        mappy.MoveToRegion(MapSpan.FromCenterAndRadius(myLocation, Distance.FromMeters(10)));
    }

    private void LocationListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        var positionRec = (PositionRec)e.SelectedItem;
        
            var PolyColor = positionRec.TraceType == TraceType.Trace ? Colors.Red : Colors.Blue;

            Polyline polyline = new Polyline
            {
                StrokeColor = PolyColor,
                StrokeWidth = 2
            };
            var lines = positionRec?.Positions?.Where(a => a.Type == PositionType.PolyLine);
            foreach (var line in lines)
            {
                polyline.Add(new Location(line.Lat, line.Long));
            }

            var pinns = positionRec?.Positions?.Where(a => a.Type == PositionType.Pin);

            foreach (var pin in pinns)
            {
                var pinmap = new CustomPin()
                {
                    Location = new Location(pin.Lat, pin.Long),
                    Label = "Apport",
                    ImageSource = customPinImage
                };
                mappy.Pins.Add(pinmap);
            }
            mappy.MapElements.Add(polyline);
            
        var firstPosition = positionRec?.Positions?.FirstOrDefault();
        if (firstPosition == null)
            return;
        
        mappy.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(firstPosition.Lat, firstPosition.Long), Distance.FromMeters(1)));

    }

    async void DeleteTrace_Clicked(System.Object sender, System.EventArgs e)
    {
        if (sender is MenuItem menuItem)
        {
            if (menuItem.BindingContext is PositionRec position)
            {
                await viewModel.DeletePositionRecording(position.Id);
                LocationListViewItems.Remove(position);

            }
        }
    }
}

