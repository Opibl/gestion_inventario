using System.Drawing.Drawing2D;

namespace MiApp.UI;

public class ModernPanel : Panel
{
    public ModernPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.White;
        Resize += ModernPanel_Resize;
    }

    private void ModernPanel_Resize(object? sender, EventArgs e)
    {
        using var path = GetRoundedRect(new Rectangle(0, 0, Width, Height), 18);
        Region = new Region(path);
    }

    private GraphicsPath GetRoundedRect(Rectangle r, int radius)
    {
        int d = radius * 2;
        var path = new GraphicsPath();

        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();

        return path;
    }
}