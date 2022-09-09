using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyApp.Namespace
{
    public class SignoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // The cookie auth handler will clear the local cookie when you sign out from its authentication scheme.
            // The OpenId Connect handler will perform the protocol steps for the roundtrip to IdentityServer when you sign out of its scheme.
            return SignOut("Cookies", "oidc");
            // This will clear the local cookie and then redirect to the IdentityServer.
            // The IdentityServer will clear its cookies and then give the user a link to return back to the web application.
        }
    }
}
