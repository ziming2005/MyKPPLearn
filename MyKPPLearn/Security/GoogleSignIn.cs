using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Web.Script.Serialization;

namespace MyKPPLearn.Security
{
    /// <summary>The verified identity from a Google ID token.</summary>
    public class GoogleIdentity
    {
        public string Subject { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// "Sign in with Google": checks the ID token the Google button hands to the page.
    /// The token is sent to Google's tokeninfo endpoint, which validates its signature and expiry;
    /// this class then checks it was issued by Google, for this site's client ID, for a verified email.
    /// </summary>
    public static class GoogleSignIn
    {
        private const string TokenInfoUrl = "https://oauth2.googleapis.com/tokeninfo?id_token=";

        /// <summary>The OAuth client ID from Web.config, or null when Google sign-in is not configured.</summary>
        public static string ClientId
        {
            get
            {
                string value = ConfigurationManager.AppSettings["GoogleClientId"];
                return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            }
        }

        /// <summary>Returns the identity, or null if the token is missing, invalid, expired or not for this site.</summary>
        public static GoogleIdentity Verify(string idToken)
        {
            string clientId = ClientId;

            if (clientId == null || string.IsNullOrWhiteSpace(idToken) || idToken.Length > 4096)
            {
                return null;
            }

            string json;

            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                using (WebClient client = new WebClient())
                {
                    json = client.DownloadString(TokenInfoUrl + Uri.EscapeDataString(idToken));
                }
            }
            catch (WebException)
            {
                // Google answers 400 for an invalid or expired token.
                return null;
            }

            Dictionary<string, object> claims = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(json);

            string issuer = Claim(claims, "iss");
            string subject = Claim(claims, "sub");
            string email = Claim(claims, "email");

            bool issuedByGoogle = issuer == "accounts.google.com" || issuer == "https://accounts.google.com";
            bool forThisSite = string.Equals(Claim(claims, "aud"), clientId, StringComparison.Ordinal);
            bool emailVerified = string.Equals(Claim(claims, "email_verified"), "true", StringComparison.OrdinalIgnoreCase);

            if (!issuedByGoogle || !forThisSite || !emailVerified || string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(email))
            {
                return null;
            }

            return new GoogleIdentity
            {
                Subject = subject,
                Email = email.Trim().ToLowerInvariant(),
                Name = Claim(claims, "name")
            };
        }

        private static string Claim(Dictionary<string, object> claims, string name)
        {
            object value;
            return claims != null && claims.TryGetValue(name, out value) && value != null ? Convert.ToString(value) : null;
        }
    }
}
