# Google Sign In Migration Sample

Sample Xamarin.iOS showing how to use the new Google SignIn API's with the old Plus API's.


Google has  a newer/better Sign In SDK for iOS which is now available on [NuGet](https://www.nuget.org/packages/Xamarin.Google.iOS.SignIn/), however the existing [Plus](https://www.nuget.org/packages/Xamarin.Google.iOS.Plus/) and [Play Games](https://www.nuget.org/packages/Xamarin.Google.iOS.PlayGames/) SDKs still use the old Sign In API's (contained within the existing Plus SDK).  

This is mostly interesting because while the old Sign In API's still work, as a fallback method of authenticating (if no Google apps are installed to handle auth), the old Sign In SDK launches Safari to perform the authentication.

This is a problem as Apple has started to reject some apps that use Safari to perform authentication.  It's ok to use a webview inside of your own app for authentication, just not to use Safari.  The new Sign In SDK does this and aheres to Apple's new policy.

The problem is, it's not clear how to integrate the new Sign In SDK with the existing Plus and Play Games SDKs which still bundle the old Sign In SDK.  Until Google releases an update, this sample exists to show a simple work around.

When you implement the new Sign In SDK, you will implement the `ISignInDelegate` with at least the `DidSignIn` method.  With the given parameters of that method it is possible to create an instance of `OAuth2Authentication`:

```csharp
class MySignInDelegate : ISignInDelegate 
{
	void DidSignIn (SignIn signIn, GoogleUser user, NSError error) 
	{
		// Convert the new SignIn info into old format
		var myOAuth2AuthenticationInstance = new OAuth2Authentication {
			ClientId = signIn.ClientID,
			AccessToken = user.Authentication.AccessToken,
			ExpirationDate = user.Authentication.AccessTokenExpirationDate,
			RefreshToken = user.Authentication.RefreshToken,
			TokenURL = new NSUrl ("https://accounts.google.com/o/oauth2/token")
		}
	}
}
```

Once you have an instance of `OAuth2Authentication` which is what the Plus and Play Games APIs expect, you can assign it to your API calls:

```csharp
var plusService = new Google.OpenSource.ServicePlus {
	RetryEnabled = true,
	Authorizer = myOAuth2AuthenticationInstance
};
```

This should allow you to continue using the current versions of the Plus and Play Games SDKs with the new Sign In SDK.

Enjoy!