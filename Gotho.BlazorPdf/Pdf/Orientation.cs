namespace Gotho.BlazorPdf.Pdf;

public record Orientation(PdfOrientation PdfOrientation)
{
    public PdfOrientation PdfOrientation { get; private set; } = PdfOrientation;
    private int _orientation = PdfOrientation == PdfOrientation.Portrait ? 0 : -90;

    public int GetOrientation()
    {
        return _orientation;
    }
    
    public void RotateClockwise()
    {
        _orientation += 90;
        _orientation = _orientation.Equals(360) ? 0 : _orientation;
        
        if (_orientation.Equals(90) || _orientation.Equals(270))
            PdfOrientation = PdfOrientation.Landscape;
        else
            PdfOrientation = PdfOrientation.Portrait;
    }

    public void RotateCounterClockwise()
    {
        _orientation -= 90;
        _orientation = _orientation.Equals(-90) ? 270 : _orientation;

        if (_orientation.Equals(90) || _orientation.Equals(270))
            PdfOrientation = PdfOrientation.Landscape;
        else
            PdfOrientation = PdfOrientation.Portrait;
    }

    public void Flip()
    {
        PdfOrientation = PdfOrientation == PdfOrientation.Portrait ? PdfOrientation.Landscape : PdfOrientation.Portrait;
        _orientation = _orientation switch
        {
            0 => 90,
            90 => 0,
            180 => 270,
            270 => 180,
            _ => 0
        };
    }
}