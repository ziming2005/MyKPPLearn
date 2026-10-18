<%@ Page Title="" Language="C#" MasterPageFile="~/Learner.Master" AutoEventWireup="true" CodeBehind="HiddenRoadSignReview.aspx.cs" Inherits="MyKPPLearn.Learner.HiddenRoadSignReview" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Review Answers | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <section class="practice-review mx-review hrs-review">
        <div class="mx-review__top">
            <asp:HyperLink ID="lnkBackTop" runat="server" CssClass="btn btn-secondary btn-small">&larr; Back</asp:HyperLink>
        </div>

        <div class="page-toolbar">
            <h2>Review Answers</h2>
            <label class="practice-review__filter"><input type="checkbox" id="hrsMistakesOnly" /> Show mistakes only</label>
        </div>
        <p class="mx-review__sub"><asp:Literal ID="litSetName" runat="server" /></p>
        <p id="hrsNoMistakes" class="form-hint" hidden>No mistakes to show. Every answer was correct.</p>

        <div id="hrsReviewList" class="practice-review__list">
            <asp:Repeater ID="rptReview" runat="server">
                <ItemTemplate>
                    <article class='<%# "card practice-review__item mx-review-item " + ((bool)Eval("IsCorrect") ? "mx-review-item--correct" : "mx-review-item--incorrect") %>'>
                        <div class="mx-review-item__head">
                            <h3>Question <%# Eval("FoundOrder") %></h3>
                            <span class="badge"><%# Server.HtmlEncode(Convert.ToString(Eval("AssetName"))) %></span>
                            <span class='<%# "mx-outcome " + ((bool)Eval("IsCorrect") ? "mx-outcome--correct" : "mx-outcome--incorrect") %>'><%# (bool)Eval("IsCorrect") ? "&#10003; Correct" : "&#10005; Incorrect" %></span>
                        </div>

                        <img class="lesson-image" src='<%# ResolveUrl("~/" + Eval("AssetImagePath")) %>' alt='<%# Server.HtmlEncode(Convert.ToString(Eval("AssetName"))) %>' />

                        <p class="practice-review__question-text"><%# Server.HtmlEncode(Convert.ToString(Eval("QuestionText"))) %></p>

                        <p class="practice-review__label">Your Answer:</p>
                        <p class="practice-review__answer"><%# AnswerText(Eval("SelectedOrder"), Eval("SelectedText")) %></p>

                        <%# IncorrectDetails(Eval("IsCorrect"), Eval("CorrectOrder"), Eval("CorrectText"), Eval("Explanation")) %>
                    </article>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <div class="form-actions">
            <asp:HyperLink ID="lnkBackResult" runat="server" CssClass="btn btn-secondary">Back to Result</asp:HyperLink>
            <a class="btn btn-primary" href="~/Learner/Game.aspx" runat="server">Back to Game</a>
        </div>
    </section>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script>
        (function () {
            var toggle = document.getElementById("hrsMistakesOnly");
            var list = document.getElementById("hrsReviewList");
            var none = document.getElementById("hrsNoMistakes");

            if (!toggle || !list) { return; }

            toggle.addEventListener("change", function () {
                list.classList.toggle("is-mistakes-only", toggle.checked);
                none.hidden = !(toggle.checked && !list.querySelector(".mx-review-item--incorrect"));
            });
        })();
    </script>
</asp:Content>
