using System;

namespace MyKPPLearn.Learner
{
    public partial class MockExam : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ((LearnerMaster)Master).PageHeading = "Mock Exam";
        }
    }
}
