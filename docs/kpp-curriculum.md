# English KPP learning curriculum

The agreed outline contains 47 lessons (not the initially stated 45), across ten chapters. The local database has been populated with introductory English teaching text, key points, self-check questions and answers, source page references and original SVG learning aids.

## Source and scope

Source: JPJ, Kurikulum Pendidikan Pemandu Motokar dan Motosikal, fourth edition, 2025. Page references refer to PDF pages rather than the inconsistent contents-page references.

This is a concise teaching adaptation, not a complete translation of every paragraph or a certification of current legal requirements. Detailed sign catalogues, full procedural instructions and practical demonstrations can be expanded through the existing lesson editor. Each lesson has an original key-point graphic; selected road-sign and observation lessons include pictorial diagrams. No original PDF photographs are redistributed.

The mock-exam format has deliberately not been changed. The source's theory format differs from the app's current format and requires a separate product decision. Detailed penalties, foreign-licence procedures, child-restraint specifications and emergency treatment instructions require current authoritative validation before expansion.

## Files

- App_Data/KppCurriculum.json: structured authoring reference, with source pages and images.
- App_Data/SeedKppCurriculum.sql: the one-time database migration that imported the lesson content (no longer run).
- Content/images/learning/kpp-2025/: original accessible SVG learning aids.
- tests/kpp-curriculum.ps1: read-only LocalDB verification.

## Import behaviour

The curriculum has been imported into App_Data/MyKPPLearn.mdf, and administrators now manage it through the lesson editor. Application startup no longer runs the import scripts, and the CurriculumImports, KppPreviousChapters and KppPreviousLessonImages tables have been removed.

A new empty database therefore starts with no lessons, so the project must be run with its existing MyKPPLearn.mdf. Do not run SeedKppCurriculum.sql, UpdateKppLessonImages.sql or UpdateKppLessonContent.sql against this database: without the removed tables they would import the curriculum again. They are kept only as a record of the import.

## Verification

Run from the repository root:

    powershell -NoProfile -ExecutionPolicy Bypass -File tests/kpp-curriculum.ps1

The test verifies the database file belongs to this workspace, checks all 47 image files as XML, validates basic content completeness and checks the active 10/47 counts. It only reads the database.
