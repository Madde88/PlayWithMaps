using System;
using MapKit;
using UIKit;

namespace PlayWithMaps.Platforms.iOS.Handlers;

public class CustomAnnotation : MKPointAnnotation
{
    public Guid Identifier { get; init; }
    public UIImage? Image { get; init; }
    public required IMapPin Pin { get; init; }
}
