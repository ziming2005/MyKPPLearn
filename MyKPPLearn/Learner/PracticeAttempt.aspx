<%@ Page Title="" Language="C#" MasterPageFile="~/Learner.Master" AutoEventWireup="true" CodeBehind="PracticeAttempt.aspx.cs" Inherits="MyKPPLearn.Learner.PracticeAttempt" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Practice | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div id="practiceAttemptRoot" data-category='<%= CategoryId %>' data-set='<%= SetNumber %>' data-app-root='<%= ResolveUrl("~/") %>'>

        <nav id="practiceCrumb" class="breadcrumb" aria-label="Breadcrumb"></nav>

        <!-- ================= INSTRUCTIONS ================= -->
        <section id="viewInstructions" class="card practice-instructions">
            <h2 id="instrTitle"></h2>
            <div class="practice-chips">
                <span id="instrSubtitle" class="practice-chip"></span>
                <span id="instrCount" class="practice-chip"></span>
                <span class="practice-chip">No time limit</span>
            </div>

            <div class="practice-instructions__block">
                <h3>Instructions</h3>
                <ul class="mx-intro__tips">
                    <li><svg class="mx-intro__check" viewBox="0 0 20 20" aria-hidden="true" focusable="false"><path d="M4 10.5l4 4 8-9" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"/></svg><span>Select one answer for each question.</span></li>
                    <li><svg class="mx-intro__check" viewBox="0 0 20 20" aria-hidden="true" focusable="false"><path d="M4 10.5l4 4 8-9" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"/></svg><span>Your answer is checked straight away and cannot be changed.</span></li>
                    <li><svg class="mx-intro__check" viewBox="0 0 20 20" aria-hidden="true" focusable="false"><path d="M4 10.5l4 4 8-9" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"/></svg><span>You can move between questions before you submit.</span></li>
                </ul>
            </div>

            <button type="button" id="btnStartPractice" class="btn btn-primary">Start Practice</button>
        </section>

        <!-- ================= ATTEMPT ================= -->
        <section id="viewAttempt" class="practice-attempt" hidden>
            <div class="practice-attempt__meta">
                <span id="attemptCategoryTitle" class="practice-attempt__category"></span>
                <span id="attemptAnsweredText" class="practice-attempt__answered"></span>
            </div>

            <div id="attemptProgressBar" class="progress-track" role="progressbar" aria-valuemin="0" aria-valuemax="0" aria-valuenow="0" aria-label="Practice progress">
                <div id="attemptProgressFill" class="progress-track__fill" style="width: 0%;"></div>
            </div>

            <div class="card question-card">
                <div id="questionImageWrap" class="question-card__image" hidden>
                    <img id="questionImage" alt="" class="lesson-image" />
                </div>

                <h3 id="questionText" class="question-card__text"></h3>

                <div id="questionOptions" class="question-options" role="radiogroup" aria-labelledby="questionText"></div>

                <div id="questionFeedback" class="question-feedback" hidden></div>
            </div>

            <div class="lesson-nav">
                <div class="lesson-nav__side">
                    <button type="button" id="btnPrevQuestion" class="btn btn-secondary">Previous</button>
                </div>
                <div class="lesson-nav__center"></div>
                <div class="lesson-nav__side lesson-nav__side--end">
                    <button type="button" id="btnNextQuestion" class="btn btn-primary">Next</button>
                    <button type="button" id="btnSubmitPractice" class="btn btn-primary" hidden>Submit Practice</button>
                </div>
            </div>
        </section>

        <!-- ================= SUBMIT CONFIRMATION ================= -->
        <div id="submitConfirmOverlay" class="modal-overlay" hidden>
            <div class="modal-panel card" role="alertdialog" aria-modal="true" aria-labelledby="submitConfirmTitle">
                <h3 id="submitConfirmTitle">Submit Practice?</h3>
                <p id="submitConfirmText"></p>
                <div class="form-actions">
                    <button type="button" id="btnContinuePractice" class="btn btn-secondary">Go to Unanswered</button>
                    <button type="button" id="btnSubmitAnyway" class="btn btn-primary">Submit Anyway</button>
                </div>
            </div>
        </div>

        <!-- ================= RESULT ================= -->
        <section id="viewResult" class="card practice-result" hidden>
            <h2 class="practice-result__heading">Practice Result</h2>
            <p id="resultMeta" class="practice-result__meta"></p>

            <div class="practice-result__score">
                <div class="practice-result__score-value" id="resultScore"></div>
                <div class="practice-result__score-pct" id="resultPercentage"></div>
            </div>

            <p id="resultMessage" class="practice-result__message"></p>

            <div class="practice-result__breakdown">
                <div class="practice-result__stat practice-result__stat--correct"><strong id="resCorrect">0</strong><span>Correct</span></div>
                <div class="practice-result__stat practice-result__stat--incorrect"><strong id="resIncorrect">0</strong><span>Incorrect</span></div>
                <div class="practice-result__stat practice-result__stat--unanswered"><strong id="resUnanswered">0</strong><span>Unanswered</span></div>
            </div>

            <div class="form-actions">
                <a id="lnkNextSet" class="btn btn-primary" href="#" hidden>Next Set</a>
                <button type="button" id="btnReviewAnswers" class="btn btn-secondary">Review Answers</button>
                <button type="button" id="btnTryAgain" class="btn btn-secondary">Try Again</button>
                <a class="btn btn-secondary" href="~/Learner/Practice.aspx" runat="server">Back to Practice</a>
            </div>
        </section>

        <!-- ================= REVIEW ================= -->
        <section id="viewReview" class="practice-review mx-review" hidden>
            <div class="mx-review__top">
                <button type="button" id="btnBackToResultTop" class="btn btn-secondary btn-small">&larr; Back</button>
            </div>
            <div class="page-toolbar">
                <h2>Review Answers</h2>
                <label class="practice-review__filter"><input type="checkbox" id="chkMistakesOnly" /> Show mistakes only</label>
            </div>
            <p id="reviewSub" class="mx-review__sub"></p>

            <div id="reviewList" class="practice-review__list"></div>

            <div class="form-actions">
                <button type="button" id="btnBackToResult" class="btn btn-secondary">Back to Result</button>
                <a class="btn btn-primary" href="~/Learner/Practice.aspx" runat="server">Back to Practice</a>
            </div>
        </section>

    </div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script src="<%= ResolveUrl("~/Scripts/practice.js") + "?v=" + PracticeDemoScriptVersion %>"></script>
</asp:Content>
