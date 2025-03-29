namespace Gotho.BlazorPdf.Pdf;

public class DrawLayer
{
    public bool Enabled { get; private set; } = false;

    public string PenColor { get; private set; } = "#000000";
    public int PenThickness { get; private set; } = 2;

    public void Toggle()
    {
        Enabled = !Enabled;
    }

    public void UpdateColor(string color)
    {
        PenColor = color;
    }

    public void UpdateThickness(int thickness)
    {
        PenThickness = thickness;
    }
}