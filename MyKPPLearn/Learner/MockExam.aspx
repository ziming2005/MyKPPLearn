<%@ Page Title="" Language="C#" MasterPageFile="~/Learner.Master" AutoEventWireup="true" CodeBehind="MockExam.aspx.cs" Inherits="MyKPPLearn.Learner.MockExam" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Mock Exam | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="dashboard-greeting">
        <h2>Mock Exam</h2>
        <p>Choose an exam type, then select a Mock Exam Set.</p>
    </div>

    <div class="card-grid card-grid--2">

        <div class="card mx-type-card stat-card--blue">
            <span class="dash-icon dash-icon--lg"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><circle cx="12" cy="9" r="6"/><path d="M8.5 14 7 22l5-3 5 3-1.5-8"/></svg></span>
            <h3>KPP Standard (Car or Motorcycle Only)</h3>
            <ul class="mx-type-card__list" aria-label="Exam summary">
                <li class="mx-type-card__row"><svg class="mx-type-card__icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M9 9a3 3 0 1 1 4.5 2.6c-.9.5-1.5 1.1-1.5 2.4"/><circle cx="12" cy="17.5" r=".6"/><circle cx="12" cy="12" r="10"/></svg>58 Questions</li>
                <li class="mx-type-card__row mx-type-card__row--time"><svg class="mx-type-card__icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><circle cx="12" cy="13" r="8"/><path d="M12 9v4l2.5 2.5"/><path d="M9 2h6"/></svg>45 Minutes</li>
                <li class="mx-type-card__row mx-type-card__row--sections"><svg class="mx-type-card__icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12z"/><circle cx="12" cy="12" r="3"/></svg>Colour Blind, Road Signs, Rules of the Road, KEJARA &amp; Safety</li>
                <li class="mx-type-card__row"><svg class="mx-type-card__icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/><path d="m9 12 2 2 4-4"/></svg>Pass: 8/8 Colour Blind &middot; 42/50 Theory</li>
            </ul>
            <a class="btn btn-primary" href="~/Learner/MockExamSets.aspx?type=kpp-standard" runat="server">View Exam Sets</a>
        </div>

        <div class="card mx-type-card stat-card--purple">
            <span class="dash-icon dash-icon--lg"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><circle cx="12" cy="13" r="8"/><path d="M12 9v4l2.5 2.5"/><path d="M9 2h6"/></svg></span>
            <h3>KPP Combined (Car &amp; Motorcycle)</h3>
            <ul class="mx-type-card__list" aria-label="Exam summary">
                <li class="mx-type-card__row"><svg class="mx-type-card__icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M9 9a3 3 0 1 1 4.5 2.6c-.9.5-1.5 1.1-1.5 2.4"/><circle cx="12" cy="17.5" r=".6"/><circle cx="12" cy="12" r="10"/></svg>78 Questions</li>
                <li class="mx-type-card__row mx-type-card__row--time"><svg class="mx-type-card__icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><circle cx="12" cy="13" r="8"/><path d="M12 9v4l2.5 2.5"/><path d="M9 2h6"/></svg>75 Minutes</li>
                <li class="mx-type-card__row mx-type-card__row--sections"><svg class="mx-type-card__icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12z"/><circle cx="12" cy="12" r="3"/></svg>Colour Blind, Road Signs, Rules of the Road, KEJARA &amp; Safety</li>
                <li class="mx-type-card__row"><svg class="mx-type-card__icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/><path d="m9 12 2 2 4-4"/></svg>Pass: 8/8 Colour Blind &middot; 56/70 Theory</li>
            </ul>
            <a class="btn btn-primary" href="~/Learner/MockExamSets.aspx?type=combined" runat="server">View Exam Sets</a>
        </div>

    </div>

</asp:Content>
