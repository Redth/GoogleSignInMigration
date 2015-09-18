using System;
using Foundation;

namespace SignInMigration
{
    public class SignInButtonElement : MonoTouch.Dialog.Element
    {
        public SignInButtonElement () : base (string.Empty)
        {
        }

        protected override Foundation.NSString CellKey {
            get { return new NSString ("SignInButtonElement"); }
        }

        public Google.SignIn.SignInButton SignInButton { get; private set; }

        bool enabled = true;
        public bool Enabled {
            get {
                return enabled;
            }
            set {
                enabled = value;
                if (SignInButton != null)
                    SignInButton.Enabled = value;
            }
        }

        public override UIKit.UITableViewCell GetCell (UIKit.UITableView tv)
        {
            var cell = tv.DequeueReusableCell (CellKey);

            if (SignInButton == null) {
                SignInButton = new Google.SignIn.SignInButton {
                    Frame = new CoreGraphics.CGRect (20, 0, tv.Frame.Width - 40, 44),
                    Enabled = this.Enabled,                        
                };
            }

            if (cell == null) {
                cell = new UIKit.UITableViewCell (UIKit.UITableViewCellStyle.Default, CellKey);
                cell.Add (SignInButton);
            }

            return cell;
        }
    }
}

