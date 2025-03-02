namespace MudBlazorPdf.Pdf;

public record Zoom
{
    private const int MinZoomLevel = 1;
    private const int MaxZoomLevel = 17;
    private const int DefaultZoomLevel = 8;
    
    private int _zoomLevel;

    public Zoom()
    {
        _zoomLevel = DefaultZoomLevel;
    }

    public double GetScale()
    {
        return 0.01 * GetZoomPercentage();
    }

    public bool ZoomIn()
    {
        if (_zoomLevel == MaxZoomLevel)
            return false;

        _zoomLevel += 1;
        return true;
    }
    
    public bool ZoomOut()
    {
        if (_zoomLevel == MinZoomLevel)
            return false;

        _zoomLevel -= 1;
        return true;
    }

    public bool ResetZoom()
    {
        if (_zoomLevel == DefaultZoomLevel)
            return false;
        
        _zoomLevel = DefaultZoomLevel;
        return true;
    }

    public string GetZoomPercentageString() => $"{GetZoomPercentage()}%";
    private int GetZoomPercentage()
    {
        return _zoomLevel switch
        {
            1 => 25,
            2 => 33,
            3 => 50,
            4 => 67,
            5 => 75,
            6 => 80,
            7 => 90,
            8 => 100,
            9 => 110,
            10 => 125,
            11 => 150,
            12 => 175,
            13 => 200,
            14 => 250,
            15 => 300,
            16 => 400,
            17 => 500,
            _ => 100
        };
    }
}