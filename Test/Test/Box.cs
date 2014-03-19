using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Test
{
    class Box
    {
        double x, y;
        Rect hitBox;
        String name;
        bool active;
        FrameworkElement _e;

        public Box(FrameworkElement _e)
        {
            this.x = Canvas.GetLeft(_e);
            this.y = Canvas.GetTop(_e);

            if (double.IsNaN(this.x))
                this.x = 0;
            if (double.IsNaN(this.y))
                this.y = 0;

            this.active = true;
            this._e = _e;
            this.hitBox = new Rect(x, y, _e.Width, _e.Height);
            this.name = _e.Name;
        }

        public Rect getHitBox() {
            return hitBox;
        }

        public String getName() {
            return name;
        }

        public bool getActive() {
            return active;
        }

        public void setActive(bool active) {
            this.active = active;
        }

        public FrameworkElement getFrameWorkElement() {
            return _e;
        }

        public void updateHitBox(FrameworkElement _e) {
            hitBox.X = Canvas.GetLeft(_e);
            hitBox.Y = Canvas.GetTop(_e);
        }
    }
}
