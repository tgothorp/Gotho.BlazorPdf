using Gotho.BlazorPdf.Pdf;

var checks = 0;
void Check(bool result, string description)
{
    if (!result) throw new InvalidOperationException(description);
    checks++;
}
var zoom = new Zoom();
Check(zoom.GetScale() == 1 && zoom.GetZoomPercentageString() == "100%", "Default zoom");
Check(!zoom.ResetZoom(), "Reset at default");
Check(zoom.SetScale(1.4375), "Continuous touch scale accepted");
Check(Math.Abs(zoom.GetScale() - 1.4375) < 0.00001, "Continuous scale retained");
Check(zoom.ZoomIn() && zoom.GetScale() == 1.5, "Plus selects the next preset");
zoom.SetScale(1.4375);
Check(zoom.ZoomOut() && zoom.GetScale() == 1.25, "Minus selects the previous preset");
Check(!zoom.SetScale(double.NaN) && !zoom.SetScale(double.PositiveInfinity), "Invalid scales rejected");
zoom.SetScale(100);
Check(zoom.GetScale() == 5 && !zoom.ZoomIn(), "Upper limit");
zoom.SetScale(-10);
Check(zoom.GetScale() == 0.25 && !zoom.ZoomOut(), "Lower limit");
int[] presets = [33, 50, 67, 75, 80, 90, 100, 110, 125, 150, 175, 200, 250, 300, 400, 500];
foreach (var preset in presets)
    Check(zoom.ZoomIn() && Math.Abs(zoom.GetScale() - preset / 100.0) < 0.00001, $"Preset {preset}");
Check(zoom.ResetZoom() && zoom.GetScale() == 1, "Reset after zoom");
Console.WriteLine($"{checks} zoom checks passed.");
