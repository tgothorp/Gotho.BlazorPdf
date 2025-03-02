namespace MudBlazorPdf.Pdf;

public record Orientation(PdfOrientation PdfOrientation)
{
    public PdfOrientation PdfOrientation { get; private set; } = PdfOrientation;

    private int _orientation = PdfOrientation == PdfOrientation.Portrait ? 0 : -90;
    private PdfOrientation _oldOrientation = PdfOrientation;

    public int GetOrientation()
    {
        return _orientation;
    }
    
    public void RotateClockwise()
    {
        _orientation += 90;
        _orientation = _orientation.Equals(360) ? 0 : _orientation;
        UpdateOldOrientation();
    }

    public void RotateCounterClockwise()
    {
        _orientation -= 90;
        _orientation = _orientation.Equals(360) ? 0 : _orientation;
        UpdateOldOrientation();
    }

    public void Flip()
    {
        _oldOrientation = PdfOrientation;
        PdfOrientation = PdfOrientation == PdfOrientation.Portrait ? PdfOrientation.Landscape : PdfOrientation.Portrait;
        _orientation = PdfOrientation == PdfOrientation.Portrait ? 0 : -90;
    }

    private void UpdateOldOrientation()
    {
        _oldOrientation = _orientation switch
        {
            0 => PdfOrientation = PdfOrientation.Portrait,
            -90 => PdfOrientation = PdfOrientation.Landscape,
            _ => _oldOrientation
        };
    }
}