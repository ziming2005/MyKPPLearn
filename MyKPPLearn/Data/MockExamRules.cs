using System.Collections.Generic;
using System.Linq;

namespace MyKPPLearn.Data
{
    /// <summary>One exam section and the Question Bank category its questions come from.</summary>
    public sealed class MockExamSection
    {
        public string Id;             // as used by Scripts/mock-exam.js: colourBlind, roadSigns, rulesOfTheRoad, kejaraSafety
        public string Label;
        public string CategoryCode;   // QuestionCategories.Code
        public int Count;
        public bool IsColourBlind { get { return Id == "colourBlind"; } }
    }

    /// <summary>One of the two fixed exam types (MockExamSets.ExamTypeCode).</summary>
    public sealed class MockExamType
    {
        public string Code;
        public string Name;
        public string Title;
        public int Minutes;
        public int ColourBlindPass;
        public int TheoryPass;
        public IList<MockExamSection> Sections;

        public int ColourBlindCount { get { return Sections.Where(s => s.IsColourBlind).Sum(s => s.Count); } }
        public int TheoryCount { get { return Sections.Where(s => !s.IsColourBlind).Sum(s => s.Count); } }
        public int TotalCount { get { return Sections.Sum(s => s.Count); } }
    }

    /// <summary>
    /// The fixed Mock Exam rules: the two exam types with their section counts, time limits and pass marks.
    /// These are the rules the server enforces (saving a set's questions, activating a set, marking an exam).
    /// Scripts/mock-exam.js holds the same numbers for display (EXAM_TYPES); keep both in step.
    /// </summary>
    public static class MockExamRules
    {
        public static readonly IList<MockExamType> Types = new List<MockExamType>
        {
            new MockExamType
            {
                Code = "combined", Name = "KPP Combined", Title = "KPP Combined (Car & Motorcycle)",
                Minutes = 75, ColourBlindPass = 8, TheoryPass = 56,
                Sections = Sections(21, 35, 14)
            },
            new MockExamType
            {
                Code = "kpp-standard", Name = "KPP Standard", Title = "KPP Standard (Car or Motorcycle Only)",
                Minutes = 45, ColourBlindPass = 8, TheoryPass = 42,
                Sections = Sections(15, 25, 10)
            }
        };

        public static MockExamType Find(string code)
        {
            return Types.FirstOrDefault(t => t.Code == code);
        }

        /// <summary>The section a question of this category fills, or null when the category is not part of the exam.</summary>
        public static MockExamSection SectionForCategory(MockExamType type, string categoryCode)
        {
            return type.Sections.FirstOrDefault(s => s.CategoryCode == categoryCode);
        }

        private static IList<MockExamSection> Sections(int roadSigns, int rulesOfTheRoad, int kejaraSafety)
        {
            return new List<MockExamSection>
            {
                new MockExamSection { Id = "colourBlind", Label = "Colour Blind Test", CategoryCode = "colour-recognition", Count = 8 },
                new MockExamSection { Id = "roadSigns", Label = "Road Signs", CategoryCode = "road-signs", Count = roadSigns },
                new MockExamSection { Id = "rulesOfTheRoad", Label = "Rules of the Road", CategoryCode = "rules-of-the-road", Count = rulesOfTheRoad },
                new MockExamSection { Id = "kejaraSafety", Label = "KEJARA & Safety", CategoryCode = "kejara-safety", Count = kejaraSafety }
            };
        }
    }
}
