<%@ Page Title="" Language="C#" MasterPageFile="~/Learner.Master" AutoEventWireup="true" CodeBehind="MockExamAttempt.aspx.cs" Inherits="MyKPPLearn.Learner.MockExamAttempt" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Mock Exam | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div id="mockExamAttemptRoot" data-type='<%= TypeId %>' data-set='<%= SetNumber %>' data-app-root='<%= ResolveUrl("~/") %>'>

        <!-- ================= INSTRUCTIONS ================= -->
        <section id="mxViewInstructions" class="card mx-intro">

            <header class="mx-intro__block mx-intro__header">
                <h2 id="mxInstrTitle"></h2>
                <p id="mxInstrSubtitle" class="mx-intro__set" hidden></p>
                <div class="mx-intro__stats">
                    <div class="mx-intro__stat mx-intro__stat--questions">
                        <strong id="mxInstrQuestions" class="mx-intro__stat-value"></strong>
                        <span class="mx-intro__stat-label">Questions</span>
                    </div>
                    <div class="mx-intro__stat mx-intro__stat--minutes">
                        <strong id="mxInstrMinutes" class="mx-intro__stat-value"></strong>
                        <span class="mx-intro__stat-label">Minutes</span>
                    </div>
                    <div class="mx-intro__stat mx-intro__stat--pass">
                        <h3 class="mx-intro__stat-title">Passing Requirements</h3>
                        <dl class="mx-intro__reqs">
                            <div class="mx-intro__req">
                                <dt>Colour Blind</dt>
                                <dd id="mxInstrPassCb"></dd>
                            </div>
                            <div class="mx-intro__req">
                                <dt>Theory</dt>
                                <dd id="mxInstrPassTh"></dd>
                            </div>
                        </dl>
                    </div>
                </div>
            </header>

            <div class="mx-intro__block">
                <h3>Exam Structure</h3>
                <ol id="mxInstrSections" class="mx-intro__sections"></ol>
            </div>

            <div class="mx-intro__block">
                <h3>Before You Start</h3>
                <ul class="mx-intro__tips">
                    <li><svg class="mx-intro__check" viewBox="0 0 20 20" aria-hidden="true" focusable="false"><path d="M4 10.5l4 4 8-9" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"/></svg><span>Select one answer for each question. You may change it before submitting.</span></li>
                    <li><svg class="mx-intro__check" viewBox="0 0 20 20" aria-hidden="true" focusable="false"><path d="M4 10.5l4 4 8-9" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"/></svg><span>Use the navigator to move between or flag questions.</span></li>
                    <li><svg class="mx-intro__check" viewBox="0 0 20 20" aria-hidden="true" focusable="false"><path d="M4 10.5l4 4 8-9" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"/></svg><span><strong>The timer starts as soon as you press Start</strong> and cannot be paused.</span></li>
                    <li><svg class="mx-intro__check" viewBox="0 0 20 20" aria-hidden="true" focusable="false"><path d="M4 10.5l4 4 8-9" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"/></svg><span>The exam auto-submits when time expires.</span></li>
                </ul>
            </div>

            <div class="mx-intro__block mx-intro__actions">
                <button type="button" id="mxStartBtn" class="btn btn-primary">Start Mock Exam</button>
                <a class="btn btn-secondary mx-back-to-sets" href="~/Learner/MockExam.aspx" runat="server">Back to Exam Sets</a>
            </div>
        </section>

        <!-- ================= ATTEMPT ================= -->
        <section id="mxViewAttempt" class="mx-attempt" hidden>

            <div class="card mx-header">
                <div class="mx-header__left">
                    <div id="mxExamTitle" class="mx-header__title"></div>
                    <div id="mxPosition" class="mx-header__position"></div>
                </div>
                <div id="mxTimerWrap" class="mx-timer">
                    <span class="mx-timer__label">Time Remaining</span>
                    <span id="mxTimer" class="mx-timer__value" role="timer" aria-live="off">00:00</span>
                </div>
                <div id="mxProgressBar" class="progress-track mx-header__progress" role="progressbar" aria-valuemin="1" aria-valuemax="1" aria-valuenow="1" aria-label="Question progress">
                    <div id="mxProgressFill" class="progress-track__fill" style="width: 0%;"></div>
                </div>
            </div>
            <div id="mxAnnounce" class="mx-sr-only" aria-live="polite" role="status"></div>

            <div class="mx-layout">

                <div class="mx-main">
                    <div id="mxQuestionCard" class="card question-card">
                        <div class="question-card__head mx-question-head">
                            <span id="mxSection" class="badge mx-section-badge"></span>
                            <button type="button" id="mxFlagBtn" class="btn btn-secondary btn-small mx-flag" aria-pressed="false">Flag</button>
                        </div>

                        <div id="mxImageWrap" class="question-card__image" hidden>
                            <img id="mxImage" alt="" class="lesson-image" />
                        </div>

                        <h3 id="mxQuestionText" class="question-card__text"></h3>

                        <div id="mxOptions" class="question-options" role="radiogroup" aria-labelledby="mxQuestionText"></div>
                    </div>

                    <div class="lesson-nav mx-question-nav">
                        <div class="lesson-nav__side">
                            <button type="button" id="mxPrevBtn" class="btn btn-secondary">Previous</button>
                        </div>
                        <div class="lesson-nav__center"></div>
                        <div class="lesson-nav__side lesson-nav__side--end">
                            <button type="button" id="mxNextBtn" class="btn btn-primary">Next</button>
                            <button type="button" id="mxFinalSubmitBtn" class="btn btn-primary" hidden>Submit Exam</button>
                        </div>
                    </div>
                </div>

                <aside class="mx-nav card" aria-label="Question Navigator">
                    <button type="button" id="mxNavToggle" class="mx-nav__toggle" aria-expanded="false" aria-controls="mxNavPanel">
                        <span>Question Navigator</span>
                        <span id="mxNavSummary" class="mx-nav__summary"></span>
                    </button>

                    <div id="mxNavPanel" class="mx-nav__panel">
                        <h3 class="mx-nav__heading">Questions</h3>

                        <ul class="mx-legend" aria-label="Legend">
                            <li><span class="mx-legend__swatch mx-legend__swatch--current"></span>Current</li>
                            <li><span class="mx-legend__swatch mx-legend__swatch--answered"></span>Answered</li>
                            <li><span class="mx-legend__swatch mx-legend__swatch--unanswered"></span>Unanswered</li>
                            <li><span class="mx-legend__swatch mx-legend__swatch--flagged"></span>Flagged</li>
                        </ul>

                        <div id="mxNavSections" class="mx-nav__scroll"></div>

                        <button type="button" id="mxNavSubmitBtn" class="btn btn-primary mx-nav__submit">Submit Exam</button>
                    </div>
                </aside>
            </div>
        </section>

        <!-- ================= SUBMIT CONFIRMATION ================= -->
        <div id="mxConfirmOverlay" class="modal-overlay" hidden>
            <div class="modal-panel card" role="alertdialog" aria-modal="true" aria-labelledby="mxConfirmTitle">
                <h3 id="mxConfirmTitle">Submit Mock Exam?</h3>
                <p id="mxConfirmText"></p>
                <div class="form-actions">
                    <button type="button" id="mxConfirmContinueBtn" class="btn btn-secondary">Go to Unanswered</button>
                    <button type="button" id="mxConfirmSubmitBtn" class="btn btn-primary">Submit Anyway</button>
                </div>
            </div>
        </div>

        <!-- ================= RESULT ================= -->
        <section id="mxViewResult" class="card practice-result mx-result" hidden>
            <h2 class="mx-result__heading">Mock Exam Result</h2>
            <p id="mxResultMeta" class="mx-result__exam"></p>

            <p id="mxResultTime" class="mx-result__exam"></p>

            <p id="mxResultAutoMsg" class="alert alert-warning" role="status" hidden>Time expired. Your Mock Exam was submitted automatically.</p>

            <div class="mx-result__overall-label">Overall Result</div>
            <div id="mxResultOverall" class="mx-result__overall"></div>

            <div class="mx-result__sections">
                <div class="stat-card">
                    <div class="stat-card__label">Colour Blind</div>
                    <div class="stat-card__value" id="mxResultCbScore"></div>
                    <span id="mxResultCbStatus"></span>
                    <div class="mx-result__req" id="mxResultCbReq"></div>
                </div>
                <div class="stat-card">
                    <div class="stat-card__label">Theory Test</div>
                    <div class="stat-card__value" id="mxResultThScore"></div>
                    <span id="mxResultThStatus"></span>
                    <div class="mx-result__req" id="mxResultThReq"></div>
                </div>
            </div>

            <p id="mxResultWeak" class="mx-result__weak" hidden></p>

            <div class="mx-result__scores">
                <h3>Theory Breakdown</h3>
                <ul id="mxResultSectionScores" class="mx-result__score-list"></ul>
            </div>

            <div class="form-actions">
                <a id="mxNextSetLink" class="btn btn-primary" href="#" hidden>Next Set</a>
                <button type="button" id="mxReviewBtn" class="btn btn-secondary">Review Answers</button>
                <button type="button" id="mxTryAgainBtn" class="btn btn-secondary">Try Again</button>
                <a class="btn btn-secondary mx-back-to-sets" href="~/Learner/MockExam.aspx" runat="server">Back to Exam Sets</a>
            </div>
        </section>

        <!-- ================= REVIEW ================= -->
        <section id="mxViewReview" class="practice-review mx-review" hidden>
            <div class="mx-review__top">
                <button type="button" id="mxBackToResultTopBtn" class="btn btn-secondary btn-small">&larr; Back</button>
            </div>
            <div class="page-toolbar">
                <h2>Review Answers</h2>
                <label class="practice-review__filter"><input type="checkbox" id="mxMistakesOnly" /> Show mistakes only</label>
            </div>
            <p id="mxReviewSub" class="mx-review__sub"></p>

            <div id="mxReviewList" class="practice-review__list"></div>

            <div class="form-actions">
                <button type="button" id="mxBackToResultBtn" class="btn btn-secondary">Back to Result</button>
                <a class="btn btn-primary mx-back-to-sets" href="~/Learner/MockExam.aspx" runat="server">Back to Exam Sets</a>
            </div>
        </section>

    </div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script src="<%= ResolveUrl("~/Scripts/mock-exam-demo.js") + "?v=" + MockExamScriptVersion %>"></script>
</asp:Content>
