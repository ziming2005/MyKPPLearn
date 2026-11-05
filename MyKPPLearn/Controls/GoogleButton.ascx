<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="GoogleButton.ascx.cs" Inherits="MyKPPLearn.Controls.GoogleButton" %>

<asp:PlaceHolder ID="phGoogle" runat="server">
    <asp:Label ID="lblGoogleError" runat="server" CssClass="alert alert-error" Visible="false" EnableViewState="false" />

    <div class="auth-divider"><span>or</span></div>

    <%-- Our own full-width button (Google's is at most 400px wide). Google's real button is laid over it,
         invisible and stretched to the same size, so the click still goes to Google. --%>
    <div class="google-button">
        <span class="btn btn-secondary btn-block google-button__face" aria-hidden="true">
            <svg class="google-button__logo" viewBox="0 0 48 48" aria-hidden="true"><path fill="#EA4335" d="M24 9.5c3.5 0 6.6 1.2 9.1 3.6l6.8-6.8C35.8 2.4 30.3 0 24 0 14.6 0 6.6 5.4 2.6 13.2l7.9 6.2C12.4 13.7 17.7 9.5 24 9.5z"/><path fill="#4285F4" d="M46.1 24.5c0-1.6-.1-3.1-.4-4.5H24v9h12.4c-.5 2.9-2.2 5.3-4.6 6.9l7.4 5.7c4.3-4 6.9-9.9 6.9-17.1z"/><path fill="#FBBC05" d="M10.5 28.6c-.5-1.4-.8-3-.8-4.6s.3-3.2.8-4.6l-7.9-6.2C.9 16.6 0 20.2 0 24s.9 7.4 2.6 10.8l7.9-6.2z"/><path fill="#34A853" d="M24 48c6.5 0 11.9-2.1 15.9-5.8l-7.4-5.7c-2.1 1.4-4.8 2.3-8.5 2.3-6.3 0-11.6-4.2-13.5-9.9l-7.9 6.2C6.6 42.6 14.6 48 24 48z"/></svg>
            <%: ButtonLabel %>
        </span>
        <div class="google-button__overlay" id="googleButton" data-client-id="<%: GoogleClientId %>" data-text="<%: ButtonText %>"></div>
    </div>

    <asp:HiddenField ID="hfCredential" runat="server" />
    <asp:Button ID="btnGoogle" runat="server" CssClass="google-button__submit" OnClick="btnGoogle_Click" CausesValidation="false" UseSubmitBehavior="false" TabIndex="-1" aria-hidden="true" />

    <script src="https://accounts.google.com/gsi/client" async defer></script>
    <script>
        // Google's button hands back a signed ID token; post it to the server, which verifies it before signing in.
        window.addEventListener("load", function () {
            var host = document.getElementById("googleButton");

            if (!host || !window.google || !google.accounts || !google.accounts.id) {
                return;
            }

            google.accounts.id.initialize({
                client_id: host.getAttribute("data-client-id"),
                callback: function (response) {
                    document.getElementById("<%= hfCredential.ClientID %>").value = response.credential;
                    document.getElementById("<%= btnGoogle.ClientID %>").click();
                }
            });

            // Google draws its button at up to 400 x 40px; scale it to cover our button exactly.
            google.accounts.id.renderButton(host, {
                theme: "outline",
                size: "large",
                shape: "rectangular",
                text: host.getAttribute("data-text"),
                width: 400
            });

            function fit() {
                var face = host.parentNode;
                host.style.transform = "scale(" + (face.offsetWidth / 400) + "," + (face.offsetHeight / 40) + ")";
            }

            fit();
            window.addEventListener("resize", fit);
        });
    </script>
</asp:PlaceHolder>
