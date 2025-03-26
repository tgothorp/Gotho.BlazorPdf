namespace Gotho.BlazorPdf.Pdf;

internal record Annotation
{
    public bool Enabled { get; private set; }

    public string PenColor { get; private set; } = "#FF0000";
    public int PenThickness { get; private set; } = 1;
    public int PenOpacity { get; private set; } = 255;
    
    public void ToggleAnnotationLayer()
    {
        Enabled = !Enabled;
    }

    public void SetPenColor(string color)
    {
        PenColor = color;
    }
}