<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="MyKPPLearn.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">MyKPP Learn | Malaysian Driving Theory Learning &amp; Practice</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <!-- ================= HERO ================= -->
    <section class="hero">
        <div class="container hero__inner">
            <div class="hero__content">
                <span class="section-eyebrow">Driving Theory Learning Platform</span>
                <h1>Prepare for Your Malaysian Driving Theory Test</h1>
                <p class="hero__lede">Study each topic, practise with questions and sit full mock exams, all in one place.</p>

                <div class="hero__actions">
                    <asp:PlaceHolder ID="phHeroGuest" runat="server">
                        <a class="btn btn-primary" href="Register.aspx">Get Started</a>
                        <a class="hero__login" href="Login.aspx">Already have an account? Login</a>
                    </asp:PlaceHolder>
                    <a id="lnkHeroDashboard" runat="server" class="btn btn-primary" visible="false">Go to Dashboard</a>
                </div>

                <ul class="feature-tags">
                    <li class="feature-tag">
                        <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M20 6L9 17l-5-5" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" /></svg>
                        Two mock exam types
                    </li>
                    <li class="feature-tag">
                        <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M20 6L9 17l-5-5" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" /></svg>
                        Road Sign Game
                    </li>
                    <li class="feature-tag">
                        <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M20 6L9 17l-5-5" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" /></svg>
                        Works on phone and desktop
                    </li>
                </ul>
            </div>

            <div class="hero__media">
                <img src="Content/images/illustrations/driving-theory.svg" alt="Driving theory learning illustration with a car, road signs and study checklist." width="560" height="440" />
            </div>
        </div>
    </section>

    <!-- ================= HOW IT WORKS ================= -->
    <section class="section">
        <div class="container">
            <div class="section-header section-header--center">
                <h2>How It Works</h2>
                <p>Four simple steps from your first lesson to exam day.</p>
            </div>

            <ol class="steps">
                <li class="steps__item">
                    <span class="steps__number">1</span>
                    <h3>Register</h3>
                    <p>Create a learner account to get started.</p>
                </li>
                <li class="steps__item">
                    <span class="steps__number">2</span>
                    <h3><a class="steps__link" href="#learn">Learn</a></h3>
                    <p>Study lessons chapter by chapter.</p>
                </li>
                <li class="steps__item">
                    <span class="steps__number">3</span>
                    <h3><a class="steps__link" href="#road-sign-game">Practise</a></h3>
                    <p>Answer practice questions or play the Road Sign Game.</p>
                </li>
                <li class="steps__item">
                    <span class="steps__number">4</span>
                    <h3><a class="steps__link" href="#mock-exam">Test Yourself</a></h3>
                    <p>Sit a timed mock exam and review your answers.</p>
                </li>
            </ol>
        </div>
    </section>

    <!-- ================= TOPICS (learn + practise) ================= -->
    <section id="learn" class="section topics-section">
        <div class="container">
            <div class="section-header section-header--center">
                <h2>Learn and Practise by Topic</h2>
                <p>Study each topic through structured lessons, then reinforce it with focused A/B/C practice questions.</p>
            </div>

            <div class="card-grid card-grid--4">
                <div class="card topic-card topic-card--red">
                    <div class="card-icon">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="13.5" cy="6.5" r="1" /><circle cx="17.5" cy="10.5" r="1" /><circle cx="8.5" cy="7.5" r="1" /><circle cx="6.5" cy="12.5" r="1" /><path d="M12 2C6.5 2 2 6.5 2 12s4.5 10 10 10c.93 0 1.65-.75 1.65-1.69 0-.44-.18-.83-.44-1.12-.29-.29-.44-.65-.44-1.13a1.64 1.64 0 0 1 1.67-1.67h2c3.05 0 5.55-2.5 5.55-5.55C21.97 6.01 17.46 2 12 2z" /></svg>
                    </div>
                    <h3>Colour Recognition</h3>
                    <p>Understand the meaning of traffic light and road-marking colours.</p>
                    <div class="topic-card__actions">
                        <a class="btn btn-secondary btn-small" href="Login.aspx">Learn</a>
                        <a class="btn btn-secondary btn-small" href="Login.aspx">Practice</a>
                    </div>
                </div>
                <div class="card topic-card topic-card--blue">
                    <div class="card-icon">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M10.3 3.9 1.8 18a2 2 0 0 0 1.7 3h17a2 2 0 0 0 1.7-3L13.7 3.9a2 2 0 0 0-3.4 0z" /><path d="M12 9v4M12 17h.01" /></svg>
                    </div>
                    <h3>Road Signs</h3>
                    <p>Recognise warning, regulatory and informational road signs.</p>
                    <div class="topic-card__actions">
                        <a class="btn btn-secondary btn-small" href="Login.aspx">Learn</a>
                        <a class="btn btn-secondary btn-small" href="Login.aspx">Practice</a>
                    </div>
                </div>
                <div class="card topic-card topic-card--green">
                    <div class="card-icon">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M4 22 8 2M20 22 16 2M12 5v3M12 11v3M12 17v3" /></svg>
                    </div>
                    <h3>Rules of the Road</h3>
                    <p>Learn the regulations and driving behaviour that keep all road users safe.</p>
                    <div class="topic-card__actions">
                        <a class="btn btn-secondary btn-small" href="Login.aspx">Learn</a>
                        <a class="btn btn-secondary btn-small" href="Login.aspx">Practice</a>
                    </div>
                </div>
                <div class="card topic-card topic-card--purple">
                    <div class="card-icon">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" /><path d="m9 12 2 2 4-4" /></svg>
                    </div>
                    <h3>KEJARA &amp; Safety</h3>
                    <p>Understand the demerit points system and safe driving practices.</p>
                    <div class="topic-card__actions">
                        <a class="btn btn-secondary btn-small" href="Login.aspx">Learn</a>
                        <a class="btn btn-secondary btn-small" href="Login.aspx">Practice</a>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <!-- ================= ROAD SIGN GAME ================= -->
    <section id="road-sign-game" class="section game-section">
        <div class="container">
            <div class="game-feature">
                <div class="game-feature__content">
                    <span class="section-eyebrow">Interactive Game</span>
                    <h2>Road Sign Game</h2>
                    <p>Learn road signs by spotting them in a real road scene instead of memorising a list.</p>

                    <ol class="game-feature__steps">
                        <li>Explore a real road scene.</li>
                        <li>Find the road signs hidden inside it.</li>
                        <li>Answer a question about each sign you discover.</li>
                    </ol>

                    <a class="btn btn-primary" href="Login.aspx">Play Road Sign Game</a>
                </div>

                <div class="game-feature__scene" aria-hidden="true">
                    <svg viewBox="0 0 320 200" preserveAspectRatio="xMidYMid slice" focusable="false">
                        <rect width="320" height="200" fill="#DBEAFE" />
                        <circle cx="262" cy="42" r="20" fill="#FDE68A" />
                        <path d="M0 120 L70 80 L130 112 L200 70 L320 125 V200 H0 Z" fill="#BFDBFE" />
                        <rect y="130" width="320" height="70" fill="#94A3B8" />
                        <path d="M0 170 H320" stroke="#F8FAFC" stroke-width="3" stroke-dasharray="22 16" />
                        <rect x="38" y="104" width="6" height="40" fill="#64748B" />
                        <circle cx="41" cy="96" r="16" fill="#16A34A" />
                        <rect x="248" y="86" width="5" height="50" fill="#64748B" />
                        <path d="M250 60 L270 82 H230 Z" fill="#F59E0B" stroke="#B45309" stroke-width="2" stroke-linejoin="round" />
                        <rect x="150" y="100" width="5" height="34" fill="#64748B" />
                        <circle cx="152" cy="92" r="14" fill="#DC2626" />
                        <rect x="144" y="90" width="16" height="4" rx="1" fill="#FFFFFF" />
                        <circle cx="152" cy="92" r="23" fill="none" stroke="#FACC15" stroke-width="3" stroke-dasharray="5 4" />
                    </svg>
                </div>
            </div>
        </div>
    </section>

    <!-- ================= MOCK EXAMINATION ================= -->
    <section id="mock-exam" class="section">
        <div class="container">
            <div class="mock-exam">
                <div class="mock-exam__intro">
                    <span class="section-eyebrow">Timed Practice</span>
                    <h2>Mock Exam</h2>
                    <p>Sit a full timed exam, then review your answers section by section.</p>

                    <div class="mock-exam__types">
                        <div class="mock-exam__type">
                            <div class="mock-exam__type-name">KPP Standard</div>
                            <div class="mock-exam__stat-value">58 <span>questions</span></div>
                            <div class="mock-exam__stat-label">45 minutes</div>
                        </div>
                        <div class="mock-exam__type">
                            <div class="mock-exam__type-name">KPP Combined</div>
                            <div class="mock-exam__stat-value">78 <span>questions</span></div>
                            <div class="mock-exam__stat-label">75 minutes</div>
                        </div>
                    </div>

                    <ul class="mock-exam__sections" aria-label="Exam sections">
                        <li>Colour Blind</li>
                        <li>Road Signs</li>
                        <li>Rules of the Road</li>
                        <li>KEJARA &amp; Safety</li>
                    </ul>

                    <a id="lnkMockExam" runat="server" class="btn btn-primary" href="~/Login.aspx">Start Mock Exam</a>
                </div>

                <div class="mock-exam__preview" aria-hidden="true">
                    <span class="sample-tag">Sample</span>
                    <div class="mock-exam__preview-row">
                        <span class="mock-exam__timer">
                            <svg viewBox="0 0 24 24" width="16" height="16" fill="none"><circle cx="12" cy="12" r="8" stroke="currentColor" stroke-width="1.8" /><path d="M12 8v4l3 2" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" /></svg>
                            31:20 remaining
                        </span>
                        <span class="mock-exam__stat-label">Question 12 of 58</span>
                    </div>

                    <div class="mock-exam__question-card">
                        <span class="mock-exam__section-name">Road Signs</span>
                        <p>Which sign indicates a pedestrian crossing ahead?</p>
                        <div class="mock-exam__option">A. Triangular warning sign</div>
                        <div class="mock-exam__option">B. Circular regulatory sign</div>
                        <div class="mock-exam__option">C. Rectangular information sign</div>
                    </div>

                    <div class="mock-exam__progress-label">
                        <span>Progress</span>
                        <span>21%</span>
                    </div>
                    <div class="mock-exam__progress-track">
                        <div class="mock-exam__progress-fill"></div>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <!-- ================= PROGRESS TRACKING ================= -->
    <section id="progress" class="section">
        <div class="container">
            <div class="progress-section__inner">
                <div class="progress-section__intro">
                    <span class="section-eyebrow">For Registered Learners</span>
                    <h2>Track Your Progress</h2>
                    <p>See how your preparation is going, all in one place.</p>

                    <ul class="progress-list">
                        <li>
                            <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M20 6L9 17l-5-5" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" /></svg>
                            <span>Learning progress across every chapter</span>
                        </li>
                        <li>
                            <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M20 6L9 17l-5-5" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" /></svg>
                            <span>Practice history for each practice set</span>
                        </li>
                        <li>
                            <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M20 6L9 17l-5-5" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" /></svg>
                            <span>Mock exam history, section by section</span>
                        </li>
                        <li>
                            <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M20 6L9 17l-5-5" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" /></svg>
                            <span>Road Sign Game history</span>
                        </li>
                    </ul>

                    <a id="lnkProgress" runat="server" class="btn btn-primary" href="~/Login.aspx">View Progress</a>
                </div>

                <div class="card" aria-hidden="true">
                    <h3>Preparation Overview <span class="sample-tag">Sample</span></h3>

                    <div class="progress-item">
                        <div class="progress-row"><span>Learning</span><span>65%</span></div>
                        <div class="progress-track"><div class="progress-track__fill" style="width: 65%;"></div></div>
                        <div class="progress-item__caption">Lessons completed</div>
                    </div>

                    <div class="progress-item">
                        <div class="progress-row"><span>Practice</span><span>48%</span></div>
                        <div class="progress-track"><div class="progress-track__fill" style="width: 48%;"></div></div>
                        <div class="progress-item__caption">Best practice score</div>
                    </div>

                    <div class="progress-item">
                        <div class="progress-row"><span>Road Sign Game</span><span>30%</span></div>
                        <div class="progress-track"><div class="progress-track__fill" style="width: 30%;"></div></div>
                        <div class="progress-item__caption">Signs found</div>
                    </div>

                    <div class="progress-item">
                        <div class="progress-row"><span>Mock Exam</span><span>55%</span></div>
                        <div class="progress-track"><div class="progress-track__fill" style="width: 55%;"></div></div>
                        <div class="progress-item__caption">Best mock exam score</div>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <!-- ================= FAQ ================= -->
    <section class="section" id="faq">
        <div class="container">
            <div class="section-header section-header--center">
                <h2>Frequently Asked Questions</h2>
                <p>Quick answers before you start.</p>
            </div>

            <div class="faq">
                <details class="faq__item" open>
                    <summary>Is this the official JPJ test?</summary>
                    <p>No. MyKPP Learn is a study aid. Its lessons and practice questions are for learning only and are not official JPJ examination material.</p>
                </details>
                <details class="faq__item">
                    <summary>What topics are covered?</summary>
                    <p>Colour recognition, road signs, rules of the road, and KEJARA &amp; safety. Each topic has structured lessons by chapter and practice questions.</p>
                </details>
                <details class="faq__item">
                    <summary>What is the Road Sign Game?</summary>
                    <p>You explore a real road scene, find the road signs hidden inside it, then answer a question about each sign you discover.</p>
                </details>
                <details class="faq__item">
                    <summary>How do the mock exams work?</summary>
                    <p>Choose KPP Standard (58 questions, 45 minutes) or KPP Combined (78 questions, 75 minutes). Both are timed and cover the Colour Blind, Road Signs, Rules of the Road and KEJARA &amp; Safety sections.</p>
                </details>
                <details class="faq__item">
                    <summary>Do I need an account?</summary>
                    <p>You can browse this page without one. Lessons, practice, the Road Sign Game and mock exams need a learner account, which also saves your lesson progress and game results so you can pick up where you left off.</p>
                </details>
                <details class="faq__item">
                    <summary>Can I use it on my phone?</summary>
                    <p>Yes. The lessons, practice, Road Sign Game and mock exams are designed to work on phones, tablets and computers.</p>
                </details>
            </div>
        </div>
    </section>

    <!-- ================= FINAL CALL TO ACTION ================= -->
    <section class="section">
        <div class="container">
            <div class="final-cta">
                <h2 id="ctaHeading" runat="server">Start Preparing for Your Theory Test</h2>
                <p id="ctaText" runat="server">Create a learner account and begin with your first lesson.</p>
                <div class="final-cta__actions">
                    <asp:PlaceHolder ID="phCtaGuest" runat="server">
                        <a class="btn btn-primary" href="Register.aspx">Create Account</a>
                        <a class="final-cta__login" href="Login.aspx">Already have an account? Login</a>
                    </asp:PlaceHolder>
                    <a id="lnkCtaDashboard" runat="server" class="btn btn-primary" visible="false">Go to Dashboard</a>
                </div>
            </div>
        </div>
    </section>

</asp:Content>
