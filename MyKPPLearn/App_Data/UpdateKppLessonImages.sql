-- Replace original KPP diagrams with the generated, lesson-specific illustrations once.
-- Only databases seeded before the seed used the illustrations directly still had .svg paths; on newer ones this changes nothing.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @LockResult int;
EXEC @LockResult=sys.sp_getapplock @Resource=N'KppEnglishCurriculum2025', @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=30000;
IF @LockResult < 0 THROW 51000, 'Could not acquire curriculum migration lock.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.CurriculumImports WHERE Version=N'kpp-2025-generated-images-v1')
BEGIN
    DECLARE @Images TABLE (OldPath nvarchar(400) PRIMARY KEY, NewPath nvarchar(500) NOT NULL);
    INSERT @Images (OldPath,NewPath) VALUES
(N'Content/images/learning/kpp-2025/1-1.svg', N'Content/images/learning/kpp-2025/1-1-illustration.png'),
(N'Content/images/learning/kpp-2025/1-2.svg', N'Content/images/learning/kpp-2025/1-2-illustration-v2.png'),
(N'Content/images/learning/kpp-2025/1-3.svg', N'Content/images/learning/kpp-2025/1-3-illustration.png'),
(N'Content/images/learning/kpp-2025/1-4.svg', N'Content/images/learning/kpp-2025/1-4-illustration.png'),
(N'Content/images/learning/kpp-2025/1-5.svg', N'Content/images/learning/kpp-2025/1-5-illustration.png'),
(N'Content/images/learning/kpp-2025/2-1.svg', N'Content/images/learning/kpp-2025/2-1-illustration.png'),
(N'Content/images/learning/kpp-2025/2-2.svg', N'Content/images/learning/kpp-2025/2-2-illustration.png'),
(N'Content/images/learning/kpp-2025/2-3.svg', N'Content/images/learning/kpp-2025/2-3-illustration.png'),
(N'Content/images/learning/kpp-2025/2-4.svg', N'Content/images/learning/kpp-2025/2-4-illustration.png'),
(N'Content/images/learning/kpp-2025/3-1.svg', N'Content/images/learning/kpp-2025/3-1-illustration.png'),
(N'Content/images/learning/kpp-2025/3-2.svg', N'Content/images/learning/kpp-2025/3-2-illustration.png'),
(N'Content/images/learning/kpp-2025/3-3.svg', N'Content/images/learning/kpp-2025/3-3-illustration-v2.png'),
(N'Content/images/learning/kpp-2025/3-4.svg', N'Content/images/learning/kpp-2025/3-4-illustration-v2.png'),
(N'Content/images/learning/kpp-2025/3-5.svg', N'Content/images/learning/kpp-2025/3-5-illustration.png'),
(N'Content/images/learning/kpp-2025/3-6.svg', N'Content/images/learning/kpp-2025/3-6-illustration.png'),
(N'Content/images/learning/kpp-2025/4-1.svg', N'Content/images/learning/kpp-2025/4-1-illustration.png'),
(N'Content/images/learning/kpp-2025/4-2.svg', N'Content/images/learning/kpp-2025/4-2-illustration.png'),
(N'Content/images/learning/kpp-2025/4-3.svg', N'Content/images/learning/kpp-2025/4-3-illustration.png'),
(N'Content/images/learning/kpp-2025/4-4.svg', N'Content/images/learning/kpp-2025/4-4-illustration.png'),
(N'Content/images/learning/kpp-2025/5-1.svg', N'Content/images/learning/kpp-2025/5-1-illustration.png'),
(N'Content/images/learning/kpp-2025/5-2.svg', N'Content/images/learning/kpp-2025/5-2-illustration.png'),
(N'Content/images/learning/kpp-2025/5-3.svg', N'Content/images/learning/kpp-2025/5-3-illustration.png'),
(N'Content/images/learning/kpp-2025/5-4.svg', N'Content/images/learning/kpp-2025/5-4-illustration.png'),
(N'Content/images/learning/kpp-2025/5-5.svg', N'Content/images/learning/kpp-2025/5-5-illustration.png'),
(N'Content/images/learning/kpp-2025/6-1.svg', N'Content/images/learning/kpp-2025/6-1-illustration.png'),
(N'Content/images/learning/kpp-2025/6-2.svg', N'Content/images/learning/kpp-2025/6-2-illustration.png'),
(N'Content/images/learning/kpp-2025/6-3.svg', N'Content/images/learning/kpp-2025/6-3-illustration.png'),
(N'Content/images/learning/kpp-2025/6-4.svg', N'Content/images/learning/kpp-2025/6-4-illustration.png'),
(N'Content/images/learning/kpp-2025/6-5.svg', N'Content/images/learning/kpp-2025/6-5-illustration.png'),
(N'Content/images/learning/kpp-2025/6-6.svg', N'Content/images/learning/kpp-2025/6-6-illustration.png'),
(N'Content/images/learning/kpp-2025/7-1.svg', N'Content/images/learning/kpp-2025/7-1-illustration.png'),
(N'Content/images/learning/kpp-2025/7-2.svg', N'Content/images/learning/kpp-2025/7-2-illustration.png'),
(N'Content/images/learning/kpp-2025/7-3.svg', N'Content/images/learning/kpp-2025/7-3-illustration.png'),
(N'Content/images/learning/kpp-2025/7-4.svg', N'Content/images/learning/kpp-2025/7-4-illustration.png'),
(N'Content/images/learning/kpp-2025/7-5.svg', N'Content/images/learning/kpp-2025/7-5-illustration.png'),
(N'Content/images/learning/kpp-2025/8-1.svg', N'Content/images/learning/kpp-2025/8-1-illustration.png'),
(N'Content/images/learning/kpp-2025/8-2.svg', N'Content/images/learning/kpp-2025/8-2-illustration.png'),
(N'Content/images/learning/kpp-2025/8-3.svg', N'Content/images/learning/kpp-2025/8-3-illustration.png'),
(N'Content/images/learning/kpp-2025/8-4.svg', N'Content/images/learning/kpp-2025/8-4-illustration.png'),
(N'Content/images/learning/kpp-2025/9-1.svg', N'Content/images/learning/kpp-2025/9-1-illustration.png'),
(N'Content/images/learning/kpp-2025/9-2.svg', N'Content/images/learning/kpp-2025/9-2-illustration.png'),
(N'Content/images/learning/kpp-2025/9-3.svg', N'Content/images/learning/kpp-2025/9-3-illustration.png'),
(N'Content/images/learning/kpp-2025/9-4.svg', N'Content/images/learning/kpp-2025/9-4-illustration.png'),
(N'Content/images/learning/kpp-2025/10-1.svg', N'Content/images/learning/kpp-2025/10-1-illustration.png'),
(N'Content/images/learning/kpp-2025/10-2.svg', N'Content/images/learning/kpp-2025/10-2-illustration.png'),
(N'Content/images/learning/kpp-2025/10-3.svg', N'Content/images/learning/kpp-2025/10-3-illustration.png'),
(N'Content/images/learning/kpp-2025/10-4.svg', N'Content/images/learning/kpp-2025/10-4-illustration.png');
    IF OBJECT_ID('dbo.KppPreviousLessonImages','U') IS NULL
        CREATE TABLE dbo.KppPreviousLessonImages (LessonId int PRIMARY KEY, ImagePath nvarchar(500) NULL);
    INSERT dbo.KppPreviousLessonImages(LessonId,ImagePath)
        SELECT l.LessonId,l.ImagePath FROM dbo.Lessons l JOIN @Images i ON i.OldPath=l.ImagePath;
    UPDATE l SET ImagePath=i.NewPath,UpdatedAt=SYSUTCDATETIME()
        FROM dbo.Lessons l JOIN @Images i ON i.OldPath=l.ImagePath;
    INSERT dbo.CurriculumImports(Version,ImportedAt) VALUES(N'kpp-2025-generated-images-v1',SYSUTCDATETIME());
END;
COMMIT TRANSACTION;

