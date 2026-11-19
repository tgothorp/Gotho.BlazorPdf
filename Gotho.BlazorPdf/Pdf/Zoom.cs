namespace Gotho.BlazorPdf.Pdf;

public record Zoom
{
    private static readonly int[] Percentages = [25, 33, 50, 67, 75, 80, 90, 100, 110, 125, 150, 175, 200, 250, 300, 400, 500];
    private double _percentage = 100;

    public Zoom()
    {
        _percentage = 100;
    }

    public double GetScale()
    {
        return 0.01 * GetZoomPercentage();
    }

    public bool ZoomIn()
    {
        var next = Percentages.FirstOrDefault(value => value > _percentage + 0.01);
        if (next == 0)
            return false;

        _percentage = next;
        return true;
    }
    
    public bool ZoomOut()
    {
        var previous = Percentages.LastOrDefault(value => value < _percentage - 0.01);
        if (previous == 0)
            return false;

        _percentage = previous;
        return true;
    }

    public bool ResetZoom()
    {
        if (_percentage == 100)
            return false;
        
        _percentage = 100;
        return true;
    }

    public bool SetScale(double scale)
    {
        if (!double.IsFinite(scale))
            return false;
        var percentage = Math.Clamp(scale * 100, 25, 500);
        if (Math.Abs(percentage - _percentage) < 0.001)
            return false;
        _percentage = percentage;
        return true;
    }

    public string GetZoomPercentageString() => $"{_percentage:0.#}%";
    private double GetZoomPercentage() => _percentage;
}
