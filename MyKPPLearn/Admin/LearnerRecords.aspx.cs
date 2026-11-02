using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Web.Services;
using MyKPPLearn.Common;
using MyKPPLearn.Data;
using MyKPPLearn.Security;

namespace MyKPPLearn.Admin
{
    /// <summary>
    /// Admin Learner Records. The page markup is a shell; Scripts/learner-records-admin.js calls the page methods below
    /// (JSON over POST) to list, add, edit, enable/disable and delete learner accounts. Searching and paging happen in
    /// the browser. Every method re-checks the Administrator role because page methods bypass Page_Load.
    /// </summary>
    public partial class LearnerRecords : System.Web.UI.Page
    {
        public const int MaxNameLength = 150;
        public const int MaxEmailLength = 254;

        // Same shape the Register page accepts: something@something.something, no spaces.
        private static readonly Regex EmailPattern = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        protected void Page_Load(object sender, EventArgs e)
        {
            SessionHelper.RequireAdministrator(this);
            ((AdminMaster)Master).PageHeading = "Learner Records";
        }

        public sealed class LearnerDto
        {
            public int id;
            public string name;
            public string email;
            public string status;
            public string joined;     // yyyy-MM-dd, local time
        }

        /// <summary>A validation problem for one field ("name", "email" or "password"), or a general error.</summary>
        public sealed class SaveResultDto
        {
            public string field;
            public string error;
            public LearnerDto learner;
        }

        [WebMethod(EnableSession = true)]
        public static List<LearnerDto> LoadLearners()
        {
            SessionHelper.DemandAdministrator();

            List<LearnerDto> learners = new List<LearnerDto>();

            foreach (DataRow row in LearnerRecordRepository.GetAll().Rows)
            {
                learners.Add(ToLearner(row));
            }

            return learners;
        }

        /// <summary>Adds a learner account with a password chosen by the administrator (the learner can change it in Profile).</summary>
        [WebMethod(EnableSession = true)]
        public static SaveResultDto Create(string name, string email, string password)
        {
            SessionHelper.DemandAdministrator();

            string fullName = (name ?? string.Empty).Trim();
            string normalizedEmail = (email ?? string.Empty).Trim().ToLowerInvariant();

            SaveResultDto problem = Validate(fullName, normalizedEmail);

            if (problem != null)
            {
                return problem;
            }

            if (!PasswordPolicy.IsValid(password))
            {
                return new SaveResultDto { field = "password", error = PasswordPolicy.Message };
            }

            int userId;
            LearnerSaveResult result = LearnerRecordRepository.Create(fullName, normalizedEmail, PasswordHasher.HashPassword(password), out userId);

            if (result == LearnerSaveResult.EmailTaken)
            {
                return new SaveResultDto { field = "email", error = "An account with this email already exists." };
            }

            return new SaveResultDto { learner = ToLearner(LearnerRecordRepository.GetById(userId)) };
        }

        [WebMethod(EnableSession = true)]
        public static SaveResultDto Update(int id, string name, string email)
        {
            SessionHelper.DemandAdministrator();

            string fullName = (name ?? string.Empty).Trim();
            string normalizedEmail = (email ?? string.Empty).Trim().ToLowerInvariant();

            SaveResultDto problem = Validate(fullName, normalizedEmail);

            if (problem != null)
            {
                return problem;
            }

            switch (LearnerRecordRepository.Update(id, fullName, normalizedEmail))
            {
                case LearnerSaveResult.EmailTaken:
                    return new SaveResultDto { field = "email", error = "Another account already uses this email." };
                case LearnerSaveResult.NotFound:
                    return new SaveResultDto { error = "Learner not found. The account may have been deleted." };
                default:
                    return new SaveResultDto { learner = ToLearner(LearnerRecordRepository.GetById(id)) };
            }
        }

        /// <summary>Enables (active = true) or disables a learner. A disabled learner cannot sign in.</summary>
        [WebMethod(EnableSession = true)]
        public static SaveResultDto SetStatus(int id, bool active)
        {
            SessionHelper.DemandAdministrator();

            if (!LearnerRecordRepository.SetStatus(id, active))
            {
                return new SaveResultDto { error = "Learner not found. The account may have been deleted." };
            }

            return new SaveResultDto { learner = ToLearner(LearnerRecordRepository.GetById(id)) };
        }

        /// <summary>Deletes the learner and all of their records; returns "Deleted" or a message.</summary>
        [WebMethod(EnableSession = true)]
        public static string Delete(int id)
        {
            SessionHelper.DemandAdministrator();

            string profileImagePath;

            if (!LearnerRecordRepository.Delete(id, out profileImagePath))
            {
                return "Learner not found. The account may already have been deleted.";
            }

            ProfileImageStorage.Delete(profileImagePath);
            return "Deleted";
        }

        /// <summary>Server-side form validation, the same rules as the Register page. Returns null when valid.</summary>
        private static SaveResultDto Validate(string fullName, string normalizedEmail)
        {
            if (fullName.Length == 0)
            {
                return new SaveResultDto { field = "name", error = "Full name is required." };
            }

            if (fullName.Length > MaxNameLength)
            {
                return new SaveResultDto { field = "name", error = "Full name must be 150 characters or fewer." };
            }

            if (normalizedEmail.Length == 0)
            {
                return new SaveResultDto { field = "email", error = "Email is required." };
            }

            if (normalizedEmail.Length > MaxEmailLength || !EmailPattern.IsMatch(normalizedEmail))
            {
                return new SaveResultDto { field = "email", error = "Enter a valid email address." };
            }

            return null;
        }

        internal static string LocalDate(object utcValue)
        {
            DateTime utc = DateTime.SpecifyKind(Convert.ToDateTime(utcValue), DateTimeKind.Utc);
            return utc.ToLocalTime().ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        }

        private static LearnerDto ToLearner(DataRow row)
        {
            return new LearnerDto
            {
                id = Convert.ToInt32(row["UserId"]),
                name = Convert.ToString(row["FullName"]),
                email = Convert.ToString(row["Email"]),
                status = Convert.ToString(row["Status"]),
                joined = LocalDate(row["CreatedAt"])
            };
        }
    }
}
