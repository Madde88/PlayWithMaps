using System.Diagnostics;
using PlayWithMaps.CustomControls;

namespace PlayWithMaps;

public partial class MainPage : ContentPage
{
    Location myLocation;
    Location myLocationFilterd;
    List<Location> locations = new();
    List<CustomPin> pins = new();
    bool isGettingLocation;
    bool isReTrace;
    Stopwatch stopWatch = new();
    int PinCount;
    ImageSource customPinImage = ImageSource.FromFile("pin.png");

    private MainPageViewModel viewModel => BindingContext as MainPageViewModel;
    private IFilterLocation filterLocation;
    public MainPage(MainPageViewModel viewModel, IFilterLocation filterLocation)
    {
        InitializeComponent();
        this.filterLocation = filterLocation;
        BindingContext = viewModel;

    }
    protected async override void OnAppearing()
    {
        await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        myLocation = await GetMyLocation();
        mappy.MoveToRegion(MapSpan.FromCenterAndRadius(myLocation, Distance.FromMeters(10)));
    }

    async void RecordLocation_Clicked(System.Object sender, System.EventArgs e)
    {
        isReTrace = false;
        await TrackLocation(TraceType.Trace);
    }

    private async Task TrackLocation(TraceType traceType)
    {
        ClearMap();
        stopWatch.Start();
        Polyline polyline;
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

        while (isGettingLocation)
        {
            await MoveToMyLocation();
            myLocationFilterd = await GetMyLocationWithFilter();
            if (myLocationFilterd != null)
            {
                locations.Add(myLocationFilterd);
                polyline.Add(myLocationFilterd);
            }

            await Task.Delay(2000);

        }
    }

    async void CancelGetLocation_Clicked(System.Object sender, System.EventArgs e)
    {
        isGettingLocation = false;
        stopWatch.Stop();
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

        foreach (var positionRec in positionRecsList)
        {
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

        }
        var firstPosition = positionRecsList?.FirstOrDefault()?.Positions?.FirstOrDefault();
        if (firstPosition == null)
            return;

        mappy.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(firstPosition.Lat, firstPosition.Long), Distance.FromMeters(1)));


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
        await MoveToMyLocation();
    }

    private async Task MoveToMyLocation()
    {
        var myLocation = await GetMyLocation();
        mappy.MoveToRegion(MapSpan.FromCenterAndRadius(myLocation, Distance.FromMeters(10)));
    }
}

