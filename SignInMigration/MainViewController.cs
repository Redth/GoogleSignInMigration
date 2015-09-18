
using System;
using System.Linq;
using System.Collections.Generic;

using MonoTouch.Dialog;

using Foundation;
using UIKit;
using Google.SignIn;

namespace SignInMigration
{
    public partial class MainViewController : DialogViewController, ISignInDelegate, ISignInUIDelegate
    {
        SignInButtonElement signinButton;
        StringElement status;

        public MainViewController () : base (UITableViewStyle.Grouped, null)
        {
            SignIn.SharedInstance.Delegate = this;
            SignIn.SharedInstance.UIDelegate = this;

            // Automatically sign in the user.
            SignIn.SharedInstance.SignInUserSilently ();

            status = new StringElement ("Not Signed In", () => {
                // Sign out
                SignIn.SharedInstance.SignOutUser ();

                // Clear signed in app state
                currentAuth = null;
                Root[1].Clear ();
                ToggleAuthUI ();
            });
            signinButton = new SignInButtonElement ();

            Root = new RootElement ("Sign In Migration") {
                new Section {
                    signinButton,
                    status,
                },
                new Section {
                },
            };

            ToggleAuthUI ();

        }

        Google.OpenSource.OAuth2Authentication currentAuth = null;

        public void DidSignIn (SignIn signIn, GoogleUser user, NSError error)
        {
            // Perform any operations on signed in user here.
            if (user != null && error == null) {

                // VERY IMPORTANT: We create an OAuth2Authentication instance here
                // to use later with the google plus API since it expects this old 
                // type of object
                currentAuth = new Google.OpenSource.OAuth2Authentication {
                    ClientId = signIn.ClientID,
                    AccessToken = user.Authentication.AccessToken,
                    ExpirationDate = user.Authentication.AccessTokenExpirationDate,
                    RefreshToken = user.Authentication.RefreshToken,
                    TokenURL = new NSUrl ("https://accounts.google.com/o/oauth2/token")
                };

                // Start fetching the signed in user's info
                GetUserInfo ();

                status.Caption = string.Format ("{0} (Tap to Sign Out)", user.Profile.Name);
                
                ToggleAuthUI ();
            }
        }

        [Export ("signIn:didDisconnectWithUser:withError:")]
        public virtual void DidDisconnect (SignIn signIn, GoogleUser user, NSError error)
        {
            currentAuth = null;
            Root [1].Clear ();
            ToggleAuthUI ();
        }

        void ToggleAuthUI ()
        {
            if (SignIn.SharedInstance.CurrentUser == null || SignIn.SharedInstance.CurrentUser.Authentication == null) {
                // Not signed in
                status.Caption = "Not Signed In";
                signinButton.Enabled = true;
            } else {
                // Signed in
                signinButton.Enabled = false;
            }
        }


        void GetUserInfo ()
        {
            // VERY IMPORTANT: We set the Authorizer to the OAuth2Authentication instance
            // we constructed with the new Google Sign In data above
            var plusService = new Google.OpenSource.ServicePlus () {
                RetryEnabled = true,
                Authorizer = currentAuth // Assign our authorizer as the converted authorization we made
            };

            // Create a QueryPlus object to get the details of the user with the given user ID. 
            // The special value "me" indicates the currently signed in user, but you could use 
            // any other valid user ID. Returns a PlusPerson.
            var query = Google.OpenSource.QueryPlus.QueryForPeopleGetWithUserId ("me");
            plusService.ExecuteQuery (query, (ticket, obj, error) => {
                // obj contains the query results, we must cast it to PlusPerson in order to get its information
                var person = obj as Google.OpenSource.PlusPerson;
                if (error != null)
                    InvokeOnMainThread (() => new UIAlertView ("Error", error.Description, null, "Ok", null).Show ());
                else {
                    InvokeOnMainThread (() => { 
                        var section = Root[1];
                        section.Add (new StyledStringElement ("Display Name", person.DisplayName, UITableViewCellStyle.Subtitle));
                        section.Add (new StyledStringElement ("Nick Name", person.Nickname, UITableViewCellStyle.Subtitle));
                        section.Add (new StyledMultilineElement ("About Me", person.AboutMe, UITableViewCellStyle.Subtitle));
                        section.Add (new StyledStringElement ("Birthday", person.Birthday, UITableViewCellStyle.Subtitle));
                        ReloadData ();
                    }); 
                }
            });
        }

        public override UIStatusBarStyle PreferredStatusBarStyle ()
        {
            return UIStatusBarStyle.LightContent;
        }
    }
}
