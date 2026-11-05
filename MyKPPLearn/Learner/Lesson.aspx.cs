using System;
using System.Data;
using System.Globalization;
using System.Web;
using MyKPPLearn.Common;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Learner
{
    public partial class Lesson : System.Web.UI.Page
    {
        private int _lessonId;
        private int _userId;

        // Where "Complete & Next" goes after saving: the next lesson or chapter, or null for the very last lesson.
        private string _nextUrl;
        private string _nextLabel;

        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireLearner(this);
            ((LearnerMaster)Master).PageHeading = "Learn";

            _userId = SessionHelper.CurrentUserId(Session).Value;

            if (!int.TryParse(Request.QueryString["id"], NumberStyles.None, CultureInfo.InvariantCulture, out _lessonId))
            {
                RedirectToLearn();
                return;
            }

            DataRow lesson;
            bool isCompleted;
            DataRow previous;
            DataRow next;
            DataRow nextChapter = null;

            try
            {
                lesson = LessonRepository.GetLessonById(_lessonId);

                // Lesson and parent chapter must both exist and be active; checked on every
                // request (including the Complete Lesson postback), so direct URLs cannot bypass it.
                if (lesson == null || !(bool)lesson["IsActive"] || !(bool)lesson["ChapterIsActive"])
                {
                    RedirectToLearn();
                    return;
                }

                isCompleted = LearningProgressRepository.IsLessonCompleted(_userId, _lessonId);
                int chapterId = Convert.ToInt32(lesson["ChapterId"]);
                int displayOrder = Convert.ToInt32(lesson["DisplayOrder"]);
                previous = LessonRepository.GetPreviousActiveLessonInChapter(chapterId, displayOrder);
                next = LessonRepository.GetNextActiveLessonInChapter(chapterId, displayOrder);

                // Last lesson of the chapter: there is no next lesson here, so offer the next chapter instead.
                if (next == null)
                {
                    nextChapter = LearningChapterRepository.GetNextChapterWithLesson(Convert.ToInt32(lesson["ChapterOrder"]));
                }
            }
            catch (Exception)
            {
                RedirectToLearn();
                return;
            }

            string number = LessonRepository.FormatLessonNumber(lesson["ChapterOrder"], lesson["DisplayOrder"]);
            string title = Convert.ToString(lesson["Title"]);
            string chapterUrl = ResolveUrl("~/Learner/Chapter.aspx?id=" + Convert.ToInt32(lesson["ChapterId"]).ToString(CultureInfo.InvariantCulture));

            if (!IsPostBack)
            {
                FlashMessage.ShowPending(Session, lblMessage);
            }

            lnkBackToChapter.HRef = chapterUrl;
            litLessonNumber.Text = number;
            litTitle.Text = HttpUtility.HtmlEncode(title);
            litContent.Text = LearningText.ToSafeHtml(Convert.ToString(lesson["Content"]));

            string imagePath = lesson["ImagePath"] as string;

            if (!string.IsNullOrEmpty(imagePath))
            {
                string imageUrl = ResolveUrl("~/" + imagePath);
                bool isAbove = LessonRepository.NormalizeImagePosition(Convert.ToString(lesson["ImagePosition"])) == LessonRepository.ImagePositionAbove;

                if (isAbove)
                {
                    imgLessonAbove.Src = imageUrl;
                    imgLessonAbove.Alt = title;
                    pnlImageAbove.Visible = true;
                }
                else
                {
                    imgLessonBelow.Src = imageUrl;
                    imgLessonBelow.Alt = title;
                    pnlImageBelow.Visible = true;
                }
            }

            litStatus.Text = isCompleted
                ? "<span class=\"status-badge status-badge--active\">Completed</span>"
                : "<span class=\"status-badge status-badge--inactive\">Not Completed</span>";
            btnComplete.Visible = !isCompleted;

            // Previous / Next Lesson stay inside the chapter; the last lesson offers Next Chapter instead of Next Lesson.
            if (previous != null)
            {
                lnkPrevious.HRef = LessonUrl(previous["LessonId"]);
                lnkPrevious.InnerHtml = "&lt; Previous Lesson " + LessonNumber(previous["ChapterOrder"], previous["DisplayOrder"]);
                lnkPrevious.Visible = true;
            }

            if (next != null)
            {
                _nextUrl = LessonUrl(next["LessonId"]);
                _nextLabel = "Next Lesson " + LessonRepository.FormatLessonNumber(next["ChapterOrder"], next["DisplayOrder"]);
            }
            else if (nextChapter != null)
            {
                // Last lesson of the chapter: Next Chapter opens the next chapter's page.
                _nextUrl = ResolveUrl("~/Learner/Chapter.aspx?id=" + Convert.ToInt32(nextChapter["ChapterId"]).ToString(CultureInfo.InvariantCulture));
                _nextLabel = "Next Chapter " + Convert.ToInt32(nextChapter["DisplayOrder"]).ToString(CultureInfo.InvariantCulture);
            }

            if (!isCompleted)
            {
                // The very last lesson leads back to Learning, where every chapter now shows its progress.
                btnComplete.Text = "Complete & " + (_nextUrl == null ? "Back to Learning" : _nextLabel) + " >";
            }
            else if (_nextUrl != null)
            {
                lnkNext.HRef = _nextUrl;
                lnkNext.InnerText = _nextLabel + " >";
                lnkNext.Visible = true;
            }
        }

        private string LessonUrl(object lessonId)
        {
            return ResolveUrl("~/Learner/Lesson.aspx?id=" + Convert.ToInt32(lessonId).ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>The "2.1" shown in the Previous / Next Lesson button text (digits and a dot, so safe to put in markup).</summary>
        private static string LessonNumber(object chapterOrder, object lessonOrder)
        {
            return HttpUtility.HtmlEncode(LessonRepository.FormatLessonNumber(chapterOrder, lessonOrder));
        }

        protected void btnComplete_Click(object sender, EventArgs e)
        {
            // Page_Load has already re-verified that the lesson and chapter are accessible.
            try
            {
                LearningProgressRepository.MarkLessonCompleted(_userId, _lessonId);
            }
            catch (Exception)
            {
                // Nothing was saved: stay on this lesson, at its buttons, and say so.
                FlashMessage.Set(Session, "Your progress could not be saved. Please try again.", true);
                Response.Redirect(Request.RawUrl + "#lessonActions", true);
                return;
            }

            // Saved: straight on to the next lesson or chapter; after the very last lesson, back to Learning.
            Response.Redirect(_nextUrl ?? ResolveUrl("~/Learner/Learn.aspx"), true);
        }

        private void RedirectToLearn()
        {
            Response.Redirect("~/Learner/Learn.aspx", true);
        }
    }
}
