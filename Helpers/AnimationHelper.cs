namespace ForkFeedMobile.Helpers;

public static class AnimationHelper
{
    public static async Task PopAsync(VisualElement element)
    {
        await element.ScaleTo(0.85, 80, Easing.CubicIn);
        await element.ScaleTo(1.0, 80, Easing.CubicOut);
    }
}
