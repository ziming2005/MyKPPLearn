<%@ Page Title="" Language="C#" MasterPageFile="~/Learner.Master" AutoEventWireup="true" CodeBehind="HiddenRoadSignGame.aspx.cs" Inherits="MyKPPLearn.Learner.HiddenRoadSignGame" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Road Sign Game | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div id="hrsGame" runat="server" class="hrs-game">

        <div class="hrs-game__bar">
            <h2 class="hrs-game__title"><span class="dash-icon"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M6 12h4M8 10v4"/><path d="M15 13h.01M18 11h.01"/><path d="M17.3 5H6.7a4 4 0 0 0-3.9 3.2l-1.5 7A3 3 0 0 0 4.2 19a3 3 0 0 0 2.4-1.2l1.4-2h8l1.4 2a3 3 0 0 0 2.4 1.2 3 3 0 0 0 2.9-3.8l-1.5-7A4 4 0 0 0 17.3 5z"/></svg></span><asp:Literal ID="litSetName" runat="server" /></h2>
            <div class="hrs-progress" aria-live="polite">
                <span class="hrs-progress__count"><span id="hrsFoundCount">0</span> / <span id="hrsTotalCount"><asp:Literal ID="litTotal" runat="server" /></span></span>
                <span class="hrs-progress__label">Found</span>
            </div>
        </div>

        <p class="hrs-game__hint">Find and tap the hidden road signs in the scene.</p>

        <p class="hrs-rotate-hint" role="note">
            <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><rect x="2" y="7" width="20" height="10" rx="2"/><path d="M12 17h.01"/></svg>
            Rotate your phone to landscape for a bigger scene.
        </p>

        <div id="hrsScene" class="hrs-scene">
            <img id="imgScene" runat="server" alt="A road scene containing hidden road signs" draggable="false" />
        </div>

        <noscript><p class="alert alert-error">JavaScript is required to play this game.</p></noscript>
    </div>

    <!-- Attempt modal: opens when a hidden road sign is found. It cannot be dismissed without answering. -->
    <div id="hrsModal" class="modal-overlay" hidden>
        <div class="modal-panel card hrs-modal" role="dialog" aria-modal="true" aria-labelledby="hrsModalTitle" aria-describedby="hrsQuestion" tabindex="-1">
            <p id="hrsModalTitle" class="hrs-modal__tag">Road Sign Found</p>
            <img id="hrsSignImg" class="hrs-modal__sign" alt="The road sign you found" hidden />
            <h3 id="hrsQuestion" class="hrs-modal__question"></h3>

            <div id="hrsOptions" class="question-options hrs-options" role="radiogroup" aria-labelledby="hrsQuestion"></div>

            <div id="hrsFeedback" class="hrs-feedback" hidden>
                <div id="hrsExplanationBlock">
                    <p class="hrs-feedback__label">Explanation</p>
                    <p id="hrsExplanation" class="hrs-feedback__text"></p>
                </div>
            </div>

            <p id="hrsError" class="field-error hrs-modal__error" role="alert" hidden></p>

            <div class="hrs-modal__actions">
                <button type="button" id="hrsNextBtn" class="btn btn-primary" disabled>Next</button>
            </div>
        </div>
    </div>

    <div id="hrsAnnounce" class="mx-sr-only" aria-live="polite" role="status"></div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script src="<%= ResolveUrl("~/Scripts/road-sign-fit.js") %>?v=20261010"></script>
    <script src="<%= ResolveUrl("~/Scripts/hidden-road-sign-game.js") + "?v=" + GameScriptVersion %>"></script>
</asp:Content>
