namespace PlayWithMaps;

public partial class MainPage : ContentPage
{
    Location myLocation;
    List<Location> locations = new();
    bool isGettingLocation;
    bool isReTrace;
    private MainPageViewModel viewModel => BindingContext as MainPageViewModel;

    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

    }
    protected async override void OnAppearing()
    {
        await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        myLocation = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Default, TimeSpan.FromSeconds(10)));
        mappy.MoveToRegion(MapSpan.FromCenterAndRadius(myLocation, Distance.FromMeters(1)));
    }

    async void GetLocation_Clicked(System.Object sender, System.EventArgs e)
    {
        isReTrace = false;
        await TrackLocation(TraceType.Trace);
    }

    private async Task TrackLocation(TraceType traceType)
    {
        Polyline polyline;
        var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
#if IOS
        request.RequestFullAccuracy = true;
#endif
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
            myLocation = await Geolocation.GetLocationAsync(request);
            mappy.MoveToRegion(MapSpan.FromCenterAndRadius(myLocation, Distance.FromMeters(1)));

            locations.Add(myLocation);
            polyline.Add(myLocation);
            await Task.Delay(1000);

        }
    }

    async void CancelGetLocation_Clicked(System.Object sender, System.EventArgs e)
    {
        isGettingLocation = false;
        var result = await DisplayAlert("Vill Du spara?", "Vill du spara din data i databasen?", "JA", "Avbryt");
        if (result)
        {
            var name = await DisplayPromptAsync("Namn", "Döp sessionen till något", "OK");
            if (string.IsNullOrWhiteSpace(name)) return;
            var positionRec = await viewModel.SavePositionRec(name, isReTrace);
            foreach (var item in locations)
            {
                var position = new Models.Position { PositionRecId = positionRec.Id, Lat = item.Latitude, Long = item.Longitude };
                await viewModel.SavePosition(position);
            }
        }
        mappy.MapElements.Clear();
        locations.Clear();
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

            foreach (var item in positionRec.Positions)
            {
                polyline.Add(new Location(item.Lat, item.Long));
            }
            mappy.MapElements.Add(polyline);
        }
        var firstPosition = positionRecsList?.FirstOrDefault()?.Positions?.FirstOrDefault();
        if (firstPosition == null)
            return;

        mappy.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(firstPosition.Lat, firstPosition.Long), Distance.FromMeters(1)));


    }

    void Clear_Clicked(System.Object sender, System.EventArgs e)
    {
        mappy.MapElements.Clear();
    }

    async void GetReTraceLocation_Clicked(System.Object sender, System.EventArgs e)
    {
        isReTrace = true;   
        await TrackLocation(TraceType.ReTrace);
    }
}


