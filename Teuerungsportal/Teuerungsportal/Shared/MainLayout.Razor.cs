namespace Teuerungsportal.Shared;

public partial class MainLayout
{
    private bool DrawerOpen { get; set; } = true;

    private void DrawerToggle()
    {
        this.DrawerOpen = !this.DrawerOpen;
    }
}