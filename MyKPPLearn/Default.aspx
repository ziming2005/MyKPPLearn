<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="MyKPPLearn.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">MyKPP Learn | Malaysian Driving Theory Learning &amp; Practice</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <!-- ================= HERO ================= -->
    <section class="hero">
        <div class="container hero__inner">
            <div class="hero__content">
                <span class="section-eyebrow">Malaysian Driving Theory Learning Platform</span>
                <h1>Prepare for Your Malaysian Driving Theory Test</h1>
                <p class="hero__lede">Learn essential Malaysian driving theory, practise by topic, and prepare for mock examinations in one structured learning platform.</p>

                <div class="hero__actions">
                    <a class="btn btn-primary" href="Register.aspx">Get Started</a>
                    <a class="btn btn-secondary" href="Login.aspx">Login</a>
                </div>

                <ul class="feature-tags">
                    <li class="feature-tag">
                        <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M20 6L9 17l-5-5" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" /></svg>
                        Structured Learning
                    </li>
                    <li class="feature-tag">
                        <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M20 6L9 17l-5-5" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" /></svg>
                        Interactive Practice
                    </li>
                    <li class="feature-tag">
                        <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M20 6L9 17l-5-5" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" /></svg>
                        Progress Tracking
                    </li>
                </ul>
            </div>

            <div class="hero__media">
                <img src="Content/images/illustrations/driving-theory.svg" alt="Driving theory learning illustration with a car, road signs and study checklist." width="560" height="440" />
            </div>
        </div>
    </section>

    <!-- ================= WHAT YOU CAN DO ================= -->
    <section class="section">
        <div class="container">
            <div class="section-header section-header--center">
                <h2>What You Can Do</h2>
                <p>MyKPP Learn brings together structured lessons, guided practice and realistic assessments so you can prepare for your driving theory test with confidence.</p>
            </div>

            <div class="card-grid card-grid--4">
                <div class="card">
                    <div class="card-icon">
                        <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M4 5.5A2.5 2.5 0 0 1 6.5 3H20v15H6.5A2.5 2.5 0 0 0 4 20.5v-15Z" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round" /><path d="M8 8h8M8 12h8" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" /></svg>
                    </div>
                    <h3>Learn Driving Theory</h3>
                    <p>Study structured Malaysian driving theory lessons and important road knowledge.</p>
                </div>

                <div class="card">
                    <div class="card-icon">
                        <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><rect x="4" y="4" width="16" height="16" rx="3" stroke="currentColor" stroke-width="1.8" /><path d="M9 12h6M9 16h4" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" /><path d="M9 8h1" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" /></svg>
                    </div>
                    <h3>Practice Questions</h3>
                    <p>Build confidence through topic-based A/B/C practice activities.</p>
                </div>

                <div class="card">
                    <div class="card-icon">
                        <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><circle cx="12" cy="12" r="8" stroke="currentColor" stroke-width="1.8" /><path d="M12 7v5l3.5 2" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" /></svg>
                    </div>
                    <h3>Mock Examination</h3>
                    <p>Prepare under timed conditions using structured mock examinations.</p>
                </div>

                <div class="card">
                    <div class="card-icon">
                        <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M4 20V10M11 20V4M18 20v-7" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" /></svg>
                    </div>
                    <h3>Track Progress</h3>
                    <p>Monitor completed learning activities and assessment performance.</p>
                </div>
            </div>
        </div>
    </section>

    <!-- ================= LEARNING CATEGORIES ================= -->
    <section id="learn" class="section" style="background-color: var(--color-surface); border-top: 1px solid var(--color-border); border-bottom: 1px solid var(--color-border);">
        <div class="container">
            <div class="section-header section-header--center">
                <h2>Learning Categories</h2>
                <p>Explore the key topics covered throughout MyKPP Learn.</p>
            </div>

            <div class="card-grid card-grid--4">
                <div class="card">
                    <div class="category-card__icon">
                        <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><circle cx="8" cy="8" r="4" fill="#DC2626" /><circle cx="16" cy="8" r="4" fill="#F59E0B" /><circle cx="12" cy="15" r="4" fill="#16A34A" /></svg>
                    </div>
                    <h3>Colour Recognition</h3>
                    <p>Understand the meaning of traffic light and road-marking colours.</p>
                    <a class="card-action" href="Login.aspx">Learn More</a>
                </div>

                <div class="card">
                    <div class="category-card__icon">
                        <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M12 3 L20 8 L12 13 L4 8 Z" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round" /><path d="M12 13v8" stroke="currentColor" stroke-width="1.8" /></svg>
                    </div>
                    <h3>Road Signs</h3>
                    <p>Recognise warning, regulatory and informational road signs.</p>
                    <a class="card-action" href="Login.aspx">Learn More</a>
                </div>

                <div class="card">
                    <div class="category-card__icon">
                        <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M4 17h16M6 17V9a6 6 0 0 1 12 0v8" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" /></svg>
                    </div>
                    <h3>Rules of the Road</h3>
                    <p>Learn the regulations that keep all road users safe.</p>
                    <a class="card-action" href="Login.aspx">Learn More</a>
                </div>

                <div class="card">
                    <div class="category-card__icon">
                        <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M12 3l7 3v6c0 4.5-3 7.5-7 9-4-1.5-7-4.5-7-9V6l7-3Z" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round" /></svg>
                    </div>
                    <h3>KEJARA &amp; Safety</h3>
                    <p>Understand the demerit points system and safe driving practices.</p>
                    <a class="card-action" href="Login.aspx">Learn More</a>
                </div>
            </div>
        </div>
    </section>

    <!-- ================= PRACTICE ================= -->
    <section id="practice" class="section">
        <div class="container">
            <div class="section-header section-header--center">
                <h2>Practice</h2>
                <p>Reinforce what you have learned through focused, topic-based practice activities.</p>
            </div>

            <div class="practice-grid">
                <div class="card practice-card">
                    <div class="card-icon">
                        <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><circle cx="8" cy="8" r="4" fill="#DC2626" /><circle cx="16" cy="8" r="4" fill="#F59E0B" /><circle cx="12" cy="15" r="4" fill="#16A34A" /></svg>
                    </div>
                    <h3>Colour Recognition</h3>
                    <p>Practice identifying the meaning of key road and signal colours.</p>
                    <div class="practice-card__footer">
                        <a class="btn btn-secondary btn-block" href="Login.aspx">Start Practice</a>
                    </div>
                </div>

                <div class="card practice-card">
                    <div class="card-icon">
                        <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M12 3 L20 8 L12 13 L4 8 Z" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round" /><path d="M12 13v8" stroke="currentColor" stroke-width="1.8" /></svg>
                    </div>
                    <h3>Road Signs</h3>
                    <p>Test your knowledge of warning, regulatory and information signs.</p>
                    <div class="practice-card__footer">
                        <a class="btn btn-secondary btn-block" href="Login.aspx">Start Practice</a>
                    </div>
                </div>

                <div class="card practice-card">
                    <div class="card-icon">
                        <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M4 17h16M6 17V9a6 6 0 0 1 12 0v8" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" /></svg>
                    </div>
                    <h3>Rules of the Road</h3>
                    <p>Review common driving scenarios and correct road behaviour.</p>
                    <div class="practice-card__footer">
                        <a class="btn btn-secondary btn-block" href="Login.aspx">Start Practice</a>
                    </div>
                </div>

                <div class="card practice-card">
                    <div class="card-icon">
                        <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M12 3l7 3v6c0 4.5-3 7.5-7 9-4-1.5-7-4.5-7-9V6l7-3Z" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round" /></svg>
                    </div>
                    <h3>KEJARA &amp; Safety</h3>
                    <p>Practice questions on the demerit points system and road safety.</p>
                    <div class="practice-card__footer">
                        <a class="btn btn-secondary btn-block" href="Login.aspx">Start Practice</a>
                    </div>
                </div>

                <div class="card practice-card practice-card--featured">
                    <div class="card-icon">
                        <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M12 3 L20 8 L12 13 L4 8 Z" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round" /><circle cx="12" cy="8.5" r="2" fill="currentColor" /></svg>
                    </div>
                    <div class="card-heading-row">
                        <h3>Hidden Road Sign</h3>
                        <span class="badge badge-accent">Interactive</span>
                    </div>
                    <p>Locate hidden road signs within a road scene and answer identification questions.</p>
                    <div class="practice-card__footer">
                        <a class="btn btn-secondary btn-block" href="Login.aspx">Start Hidden Road Sign</a>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <!-- ================= MOCK EXAMINATION ================= -->
    <section id="mock-exam" class="section">
        <div class="container">
            <div class="mock-exam">
                <div class="mock-exam__intro">
                    <span class="section-eyebrow">Mock Examination</span>
                    <h2>Mock Examination</h2>
                    <p>Prepare under timed conditions with a structured mock examination containing Colour Blind, Part A, Part B and Part C sections.</p>

                    <div class="mock-exam__stats">
                        <div>
                            <div class="mock-exam__stat-value">78</div>
                            <div class="mock-exam__stat-label">Total Questions</div>
                        </div>
                        <div>
                            <div class="mock-exam__stat-value">4</div>
                            <div class="mock-exam__stat-label">Exam Sections</div>
                        </div>
                    </div>

                    <a class="btn btn-primary" href="Login.aspx">Start Mock Exam</a>
                </div>

                <div class="mock-exam__preview" aria-hidden="true">
                    <div class="mock-exam__preview-row">
                        <span class="mock-exam__timer">
                            <svg viewBox="0 0 24 24" width="16" height="16" fill="none"><circle cx="12" cy="12" r="8" stroke="currentColor" stroke-width="1.8" /><path d="M12 8v4l3 2" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" /></svg>
                            18:42 remaining
                        </span>
                        <span class="mock-exam__stat-label">Question 12 of 78</span>
                    </div>

                    <div class="mock-exam__question-card">
                        <p>Which sign indicates a pedestrian crossing ahead?</p>
                        <div class="mock-exam__option">A. Triangular warning sign</div>
                        <div class="mock-exam__option">B. Circular regulatory sign</div>
                        <div class="mock-exam__option">C. Rectangular information sign</div>
                    </div>

                    <div class="mock-exam__progress-label">
                        <span>Overall Progress</span>
                        <span>42%</span>
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
                    <span class="section-eyebrow">Progress Tracking</span>
                    <h2>Track Your Progress</h2>
                    <p>Registered learners can monitor their preparation across every part of MyKPP Learn.</p>

                    <ul class="progress-list">
                        <li>
                            <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M20 6L9 17l-5-5" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" /></svg>
                            <span>Learning Progress across all theory topics</span>
                        </li>
                        <li>
                            <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M20 6L9 17l-5-5" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" /></svg>
                            <span>Practice Performance by category</span>
                        </li>
                        <li>
                            <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M20 6L9 17l-5-5" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" /></svg>
                            <span>Hidden Road Sign Performance</span>
                        </li>
                        <li>
                            <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M20 6L9 17l-5-5" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" /></svg>
                            <span>Mock Exam Performance over time</span>
                        </li>
                    </ul>

                    <a class="btn btn-primary" href="Login.aspx">View Progress</a>
                </div>

                <div class="card" aria-hidden="true">
                    <h3>Preparation Overview</h3>

                    <div class="progress-item">
                        <div class="progress-row"><span>Learning Progress</span><span>65%</span></div>
                        <div class="progress-track"><div class="progress-track__fill" style="width: 65%;"></div></div>
                    </div>

                    <div class="progress-item">
                        <div class="progress-row"><span>Practice Performance</span><span>48%</span></div>
                        <div class="progress-track"><div class="progress-track__fill" style="width: 48%;"></div></div>
                    </div>

                    <div class="progress-item">
                        <div class="progress-row"><span>Hidden Road Sign</span><span>30%</span></div>
                        <div class="progress-track"><div class="progress-track__fill" style="width: 30%;"></div></div>
                    </div>

                    <div class="progress-item">
                        <div class="progress-row"><span>Mock Exam Performance</span><span>55%</span></div>
                        <div class="progress-track"><div class="progress-track__fill" style="width: 55%;"></div></div>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <!-- ================= FINAL CALL TO ACTION ================= -->
    <section class="section">
        <div class="container">
            <div class="final-cta">
                <h2>Ready to Start Learning?</h2>
                <p>Create your learner account and begin preparing with structured lessons, practice activities and mock examinations.</p>
                <div class="final-cta__actions">
                    <a class="btn btn-primary" href="Register.aspx">Register</a>
                    <a class="btn btn-secondary" href="Login.aspx">Login</a>
                </div>
            </div>
        </div>
    </section>

</asp:Content>
