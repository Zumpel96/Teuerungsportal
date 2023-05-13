namespace Teuerungsportal.Pages;

public partial class Counter
{
    private int CurrentCount { get; set; }

    private void IncrementCount()
    {
        this.CurrentCount++;
    }
}