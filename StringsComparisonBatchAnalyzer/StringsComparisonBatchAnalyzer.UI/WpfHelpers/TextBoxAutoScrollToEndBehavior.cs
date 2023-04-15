using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

public class TextBoxAutoScrollToEndBehavior : Behavior<TextBox>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.TextChanged += OnTextChanged;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.TextChanged -= OnTextChanged;
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        AssociatedObject.ScrollToEnd();
    }
}