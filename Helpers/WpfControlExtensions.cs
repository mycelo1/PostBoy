using System.Windows.Controls;
using System.Windows.Media;

namespace PostBoy.Helpers
{
    public static class WpfControlExtensions
    {
        public static void SetAlert(this Control control)
        {
            void _SetAlert()
            {
                control.Background = Brushes.Yellow;
            }
            if ((control is TabItem) && (!((TabItem)control).IsSelected))
            {
                _SetAlert();
            }
            else if (!control.Focus())
            {
                _SetAlert();
            }
        }

        public static void ClearAlert(this Control control)
        {
            control.ClearValue(Control.BackgroundProperty);
        }
    }
}