-- ============================================================================
-- MyKPP Learn: Standard Practice content (Practice Sets, Questions, Question Options).
-- Exported from the team's Question Bank so every developer's local database has the same content.
--   11 Practice Sets, 101 questions, 303 options. Question images: Content/images/questions/ (committed).
--
-- Run once against your local database AFTER the application has created the schema
-- (sqlcmd -S "(localdb)\MSSQLLocalDB" -d <your database> -i SeedPracticeQuestions.sql,
--  or open it in Visual Studio's SQL Server Object Explorer and execute).
--
-- Safe to run more than once: categories are matched by Code and sets by category + SetNumber; a set's questions
-- are only added when that set has no questions yet. Existing rows are never changed or deleted.
-- Everything runs in one transaction; any error rolls the whole script back.
-- ============================================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;   -- required by the database's filtered unique indexes (sqlcmd defaults it to OFF)
SET ANSI_NULLS ON;

BEGIN TRANSACTION;

DECLARE @SetId INT;
DECLARE @CategoryId INT;
DECLARE @QuestionId INT;


-- ----------------------------------------------------------------------------
-- colour-recognition: Practice Set 1
-- ----------------------------------------------------------------------------
SELECT @CategoryId = QuestionCategoryId FROM QuestionCategories WHERE Code = N'colour-recognition';
IF @CategoryId IS NULL THROW 50010, 'Question category colour-recognition is missing. Start the application once so it creates and seeds the schema.', 1;

IF NOT EXISTS (SELECT 1 FROM PracticeSets WHERE QuestionCategoryId = @CategoryId AND SetNumber = 1)
    INSERT INTO PracticeSets (QuestionCategoryId, SetNumber, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

SELECT @SetId = PracticeSetId FROM PracticeSets WHERE QuestionCategoryId = @CategoryId AND SetNumber = 1;

IF NOT EXISTS (SELECT 1 FROM Questions WHERE PracticeSetId = @SetId)
BEGIN
    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What is this number?', N'This is number 2.', N'Content/images/questions/0418001741a6489292751f621dfc4be1.png', N'Number2', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'1', 1, 0),
        (@QuestionId, N'2', 2, 1),
        (@QuestionId, N'3', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What is the number?', N'Number 10', N'Content/images/questions/e3323610a86f4437961ef7d9827827a3.webp', N'Number 10', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'11', 1, 0),
        (@QuestionId, N'10', 2, 1),
        (@QuestionId, N'70', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What is this number?', N'This is number 3.', N'Content/images/questions/f8d397c035844e34ac7c7cc89e5110aa.jpg', N'number 3', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'2', 1, 0),
        (@QuestionId, N'4', 2, 0),
        (@QuestionId, N'3', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What is this number?', N'42', N'Content/images/questions/6285776969334fdd99be9386ecf4d9b3.jpg', N'42', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'42', 1, 1),
        (@QuestionId, N'44', 2, 0),
        (@QuestionId, N'43', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What is this number', N'35', N'Content/images/questions/5c85273dfc5d41e7b245d59a080f05f3.jpg', N'35', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'26', 1, 0),
        (@QuestionId, N'36', 2, 0),
        (@QuestionId, N'35', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does this sign warn drivers about?', N'Watch for cyclists crossing or entering the road.', N'Content/images/questions/df2964df33f446359393aae406762817.jpg', N'Bicycle crossing', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Bicycle crossing', 1, 1),
        (@QuestionId, N'Motorcycle parking', 2, 0),
        (@QuestionId, N'No bicycles', 3, 0);

END

SET @CategoryId = NULL;

-- ----------------------------------------------------------------------------
-- colour-recognition: Practice Set 2
-- ----------------------------------------------------------------------------
SELECT @CategoryId = QuestionCategoryId FROM QuestionCategories WHERE Code = N'colour-recognition';
IF @CategoryId IS NULL THROW 50010, 'Question category colour-recognition is missing. Start the application once so it creates and seeds the schema.', 1;

IF NOT EXISTS (SELECT 1 FROM PracticeSets WHERE QuestionCategoryId = @CategoryId AND SetNumber = 2)
    INSERT INTO PracticeSets (QuestionCategoryId, SetNumber, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, 2, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

SELECT @SetId = PracticeSetId FROM PracticeSets WHERE QuestionCategoryId = @CategoryId AND SetNumber = 2;

IF NOT EXISTS (SELECT 1 FROM Questions WHERE PracticeSetId = @SetId)
BEGIN
    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What is this number?', N'Number 4', N'Content/images/questions/b14d0e5a600c4d7788b24bb4a9c4732f.jpg', N'Number 4', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'5', 1, 0),
        (@QuestionId, N'6', 2, 0),
        (@QuestionId, N'4', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What is this number?', N'74', N'Content/images/questions/469d4b42017e477396c019e0e036c90b.jpg', N'74', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'75', 1, 0),
        (@QuestionId, N'74', 2, 1),
        (@QuestionId, N'76', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What is this number?', N'57', N'Content/images/questions/6f713c0e212c40c5b14b4dd6d353d2b2.jpg', N'57', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'57', 1, 1),
        (@QuestionId, N'58', 2, 0),
        (@QuestionId, N'59', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What is this question?', N'73', N'Content/images/questions/3e467c942a8e461e80c3234de13f984d.jpg', N'73', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'76', 1, 0),
        (@QuestionId, N'77', 2, 0),
        (@QuestionId, N'73', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What is this question', N'25', N'Content/images/questions/d059b0dba8994850af3879dbb3313579.jpg', N'25', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'25', 1, 1),
        (@QuestionId, N'22', 2, 0),
        (@QuestionId, N'26', 3, 0);

END

SET @CategoryId = NULL;

-- ----------------------------------------------------------------------------
-- road-signs: Practice Set 1
-- ----------------------------------------------------------------------------
SELECT @CategoryId = QuestionCategoryId FROM QuestionCategories WHERE Code = N'road-signs';
IF @CategoryId IS NULL THROW 50010, 'Question category road-signs is missing. Start the application once so it creates and seeds the schema.', 1;

IF NOT EXISTS (SELECT 1 FROM PracticeSets WHERE QuestionCategoryId = @CategoryId AND SetNumber = 1)
    INSERT INTO PracticeSets (QuestionCategoryId, SetNumber, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

SELECT @SetId = PracticeSetId FROM PracticeSets WHERE QuestionCategoryId = @CategoryId AND SetNumber = 1;

IF NOT EXISTS (SELECT 1 FROM Questions WHERE PracticeSetId = @SetId)
BEGIN
    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What is this sign meaning?', N'This sign mean slippery road', N'Content/images/questions/4de0d9bec7d54d15a09222fbdfd50238.png', N'slippery', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Slippery road', 1, 1),
        (@QuestionId, N'Can drift legally', 2, 0),
        (@QuestionId, N'Behave of surrounding car', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does a red traffic light mean?', N'Red means stop before the stop line.', N'Content/images/questions/5fa47ccd406842379f11828847405d92.jpg', N'TrafficLight', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Proceed carefully', 1, 0),
        (@QuestionId, N'Stop', 2, 1),
        (@QuestionId, N'Speed up', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does this red sign mean?', N'The red octagonal sign requires drivers to stop completely', N'Content/images/questions/19f63cb1283a40f88dbebaf829259caa.jpg', N'BHTI', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Stop', 1, 1),
        (@QuestionId, N'Give Way', 2, 0),
        (@QuestionId, N'No Entry', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does the red circular sign with a white horizontal bar mean?', N'Vehicles are prohibited from entering that road.', N'Content/images/questions/810ff525507c48a2b227fca2885a51ca.jpg', N'no entry', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'No Parking', 1, 0),
        (@QuestionId, N'No Overtaking', 2, 0),
        (@QuestionId, N'No Entry', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does this sign mean?', N'The number inside the red circle indicates the maximum permitted speed.', N'Content/images/questions/4ab5ed600a214d14b9ac7a56c8b71d12.jpg', N'sl', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Recommended speed 60 km/h', 1, 0),
        (@QuestionId, N'Maximum speed 60 km/h', 2, 1),
        (@QuestionId, N'Minimum speed 60 km/h', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does this sign mean?', N'Sounding the horn is prohibited except where necessary to prevent danger, subject to applicable rules.', N'Content/images/questions/584b3adb1003458db6f918001f002e95.jpg', N'No horn', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Use horn', 1, 0),
        (@QuestionId, N'No horn', 2, 1),
        (@QuestionId, N'Emergencies vehicles only', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does this sign warn you about?', N'Reduce speed and use an appropriate gear when travelling downhill.', N'Content/images/questions/890cc1fac11048c6a0fdf3275162f3ec.jpg', N'Steep descent', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Sharp right turn', 1, 0),
        (@QuestionId, N'Steep uphill road', 2, 0),
        (@QuestionId, N'Steep downhill road', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does the letter H indicate?', N'It indicates a hospital or hospital direction.', N'Content/images/questions/a50ca81332a34ec6851acc2f9f0ca538.jpg', N'Hospital sign', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Hotel', 1, 0),
        (@QuestionId, N'Highway', 2, 0),
        (@QuestionId, N'Hospital', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does this sign mean?', N'The road does not continue through to another road.', N'Content/images/questions/5498aa43d4b24981b4a87274734f984f.jpg', N'Dead end', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'No through road', 1, 1),
        (@QuestionId, N'One-way road', 2, 0),
        (@QuestionId, N'Highway entrance', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does this sign warn drivers about?', N'Watch for cyclists crossing or entering the road.', N'Content/images/questions/5e202391e485491dac4450e1a80eec74.jpg', N'bicycle', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Motorcycle parking', 1, 0),
        (@QuestionId, N'No bicycles', 2, 0),
        (@QuestionId, N'Bicycle crossing', 3, 1);

END

SET @CategoryId = NULL;

-- ----------------------------------------------------------------------------
-- road-signs: Practice Set 2
-- ----------------------------------------------------------------------------
SELECT @CategoryId = QuestionCategoryId FROM QuestionCategories WHERE Code = N'road-signs';
IF @CategoryId IS NULL THROW 50010, 'Question category road-signs is missing. Start the application once so it creates and seeds the schema.', 1;

IF NOT EXISTS (SELECT 1 FROM PracticeSets WHERE QuestionCategoryId = @CategoryId AND SetNumber = 2)
    INSERT INTO PracticeSets (QuestionCategoryId, SetNumber, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, 2, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

SELECT @SetId = PracticeSetId FROM PracticeSets WHERE QuestionCategoryId = @CategoryId AND SetNumber = 2;

IF NOT EXISTS (SELECT 1 FROM Questions WHERE PracticeSetId = @SetId)
BEGIN
    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does this sign mean?', N'Drivers should slow down and give way to pedestrians crossing safely.', N'Content/images/questions/8e5b5eb83a1845659debdecc8125e87b.jpg', N'ws', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Slippery road', 1, 0),
        (@QuestionId, N'No entry', 2, 0),
        (@QuestionId, N'Pedestrian crossing', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does this sign mean?', N'Drivers are not allowed to make a U-turn where this sign is displayed.', N'Content/images/questions/16a5d41def6440418c4e070d92727e19.jpg', N'nouturn', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'U-turn allowed', 1, 0),
        (@QuestionId, N'No U-turn', 2, 1),
        (@QuestionId, N'One way Road', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does this sign indicate?', N'Vehicles must not overtake other vehicles in the restricted area.', N'Content/images/questions/146dce3129de4d53b745157279b626aa.jpg', N'ot', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'No overtaking', 1, 1),
        (@QuestionId, N'No parking', 2, 0),
        (@QuestionId, N'Two-way traffic', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does this warning sign mean?', N'The road becomes narrower. Reduce speed and be alert.', N'Content/images/questions/03d4526432b046449b4e16deb43f752b.jpg', N'Road narrows', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Road closed', 1, 0),
        (@QuestionId, N'Road narrows', 2, 1),
        (@QuestionId, N'Bridge ahead', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does this sign warn you about?', N'Slow down and watch for children crossing the road.', N'Content/images/questions/f389a520e79a46b9a4036fde23597ac3.jpg', N'sa', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Hospital ahead', 1, 0),
        (@QuestionId, N'School or children crossing ahead', 2, 1),
        (@QuestionId, N'Bus station', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does this sign mean?', N'Be alert for approaching trains and obey railway signals.', N'Content/images/questions/b9b8558ab5724c828e3e6de23d62c454.jpg', N'raiway crossing', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Airport ahead', 1, 0),
        (@QuestionId, N'Railway crossing ahead', 2, 1),
        (@QuestionId, N'Bus stop ahead', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does this sign mean?', N'You must not park in the restricted area.', N'Content/images/questions/8b8f8987e3ff4135a1bef3c5a679362e.jpg', N'no parking', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Parking allowed', 1, 0),
        (@QuestionId, N'No Parking', 2, 1),
        (@QuestionId, N'Emergency Parking', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What should drivers do?', N'Follow the arrow and keep to the left side.', N'Content/images/questions/8e0f2f6a23a340748a5ef6a03d6c9408.jpg', N'kl', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Keep left', 1, 0),
        (@QuestionId, N'Keep right', 2, 1),
        (@QuestionId, N'Make a U-turn', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does this sign indicate?', N'Slow down and prepare to give way according to the roundabout rules.', N'Content/images/questions/2874b1fac5f948eba27df2ea468b0bba.jpg', N'ra', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Traffic light ahead', 1, 0),
        (@QuestionId, N'Roundabout ahead', 2, 1),
        (@QuestionId, N'Dead end', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does this sign mean?', N'Traffic will travel in both directions.', N'Content/images/questions/f68fc5cb933d46d28b4c899be6913d2e.jpg', N'two way traffic', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Two-way traffic ahead', 1, 1),
        (@QuestionId, N'One-way traffic', 2, 0),
        (@QuestionId, N'No vehicles allowed', 3, 0);

END

SET @CategoryId = NULL;

-- ----------------------------------------------------------------------------
-- road-signs: Practice Set 3
-- ----------------------------------------------------------------------------
SELECT @CategoryId = QuestionCategoryId FROM QuestionCategories WHERE Code = N'road-signs';
IF @CategoryId IS NULL THROW 50010, 'Question category road-signs is missing. Start the application once so it creates and seeds the schema.', 1;

IF NOT EXISTS (SELECT 1 FROM PracticeSets WHERE QuestionCategoryId = @CategoryId AND SetNumber = 3)
    INSERT INTO PracticeSets (QuestionCategoryId, SetNumber, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, 3, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

SELECT @SetId = PracticeSetId FROM PracticeSets WHERE QuestionCategoryId = @CategoryId AND SetNumber = 3;

IF NOT EXISTS (SELECT 1 FROM Questions WHERE PracticeSetId = @SetId)
BEGIN
    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'Animals crossing', N'Drivers should slow down and watch for animals crossing the road.', N'Content/images/questions/320540c2c7e44516bfdd9c9c306c6bc0.jpg', N'Animals crossing', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Wild animals may cross', 1, 1),
        (@QuestionId, N'nimal parking area', 2, 0),
        (@QuestionId, N'Zoo nearby', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does this sign warn drivers about?', N'Reduce speed before approaching the sharp right-hand bend.', N'Content/images/questions/c3df1a5dfc56413ab57e702c5a771a80.jpg', N'Sharp bend to the right', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Sharp bend to the left', 1, 0),
        (@QuestionId, N'Road closed', 2, 0),
        (@QuestionId, N'Sharp bend to the right', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does this sign indicate?', N'Rocks may fall onto the road. Drive carefully through the area.', N'Content/images/questions/54af8b2e8c444c3c842c539f6e3c81a6.jpg', N'Falling rocks', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Construction area', 1, 0),
        (@QuestionId, N'Falling rocks ahead', 2, 1),
        (@QuestionId, N'Uneven road', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does this sign mean?', N'The road surface may be rough or uneven. Reduce speed.', N'Content/images/questions/20b5d697d93f4bedb286254f2329f9a8.jpg', N'Uneven road', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Uneven road ahead', 1, 1),
        (@QuestionId, N'Road narrows', 2, 0),
        (@QuestionId, N'Bridge ahead', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does this warning sign mean?', N'Prepare to slow down and obey the traffic lights.', N'Content/images/questions/933b4a8e9fbd4c29b14d0d670efa50fc.jpg', N'Traffic signals ahead', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Traffic police ahead', 1, 0),
        (@QuestionId, N'Traffic lights ahead', 2, 1),
        (@QuestionId, N'No entry', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does this sign warn you about?', N'Traffic may enter from side roads. Look carefully in both directions.', N'Content/images/questions/9eec93bcf29947a7bc426277cc796a27.jpg', N'Two-way traffic crossing', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Crossroads ahead', 1, 1),
        (@QuestionId, N'No U-turn', 2, 0),
        (@QuestionId, N'Railway crossing', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does this sign mean?', N'Motorcycles are prohibited from entering the restricted road.', N'Content/images/questions/eda4515a0fe34e19bf7cd367eac4aaa4.jpg', N'No motorcycles', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Motorcycles only', 1, 0),
        (@QuestionId, N'Motorcycle repair shop', 2, 0),
        (@QuestionId, N'No motorcycles', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does this sign indicate?', N'Vehicles taller than the displayed height must not proceed.', N'Content/images/questions/a8362916654843578d4759187dcadd23.jpg', N'Maximum vehicle height', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Maximum vehicle height', 1, 1),
        (@QuestionId, N'Minimum bridge height', 2, 0),
        (@QuestionId, N'Maximum vehicle width', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does this sign indicate?', N'The lane is designated for buses during the applicable restrictions.', N'Content/images/questions/565cf2f7d25640828cb003a2772c7eaf.jpg', N'Bus lane', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Bus lane', 1, 1),
        (@QuestionId, N'School zone', 2, 0),
        (@QuestionId, N'Taxi parking', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What does this sign mean?', N'Vehicles exceeding the displayed weight limit are not permitted.', N'Content/images/questions/7f254e0856c944fcb93fffa3e8c768c9.jpg', N'Maximum vehicle weight', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Maximum vehicle speed', 1, 0),
        (@QuestionId, N'Minimum vehicle weight', 2, 0),
        (@QuestionId, N'Maximum vehicle weight restriction', 3, 1);

END

SET @CategoryId = NULL;

-- ----------------------------------------------------------------------------
-- rules-of-the-road: Practice Set 1
-- ----------------------------------------------------------------------------
SELECT @CategoryId = QuestionCategoryId FROM QuestionCategories WHERE Code = N'rules-of-the-road';
IF @CategoryId IS NULL THROW 50010, 'Question category rules-of-the-road is missing. Start the application once so it creates and seeds the schema.', 1;

IF NOT EXISTS (SELECT 1 FROM PracticeSets WHERE QuestionCategoryId = @CategoryId AND SetNumber = 1)
    INSERT INTO PracticeSets (QuestionCategoryId, SetNumber, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

SELECT @SetId = PracticeSetId FROM PracticeSets WHERE QuestionCategoryId = @CategoryId AND SetNumber = 1;

IF NOT EXISTS (SELECT 1 FROM Questions WHERE PracticeSetId = @SetId)
BEGIN
    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What you should do?', N'Yellow means the signal is changing to red. Stop before the stop line if you can do so safely. Do not speed up to beat the red light.', N'Content/images/questions/92abb19411424b9db287699b13ec220c.jpg', N'Traffic light turns yellow', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Accelerate quickly', 1, 0),
        (@QuestionId, N'Stop if safe to do so', 2, 1),
        (@QuestionId, N'Ignore the light', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What should you do?', N'Emergency vehicles need a clear route. Move aside safely without creating danger for other road users.', N'Content/images/questions/0dbadfa9bcb14c19bc5cbdb42bd61366.jpg', N'Ambulance approaching', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Give way safely', 1, 1),
        (@QuestionId, N'Race ahead', 2, 0),
        (@QuestionId, N'Follow closely behind', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What should you do?', N'Wet roads reduce tyre grip and increase stopping distance. Maintain a larger gap between vehicles.', N'Content/images/questions/0b12a31dbc49413bab6551b07c649b5b.jpg', N'Heavy Rain', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Increase speed', 1, 0),
        (@QuestionId, N'Drive close to the car ahead', 2, 0),
        (@QuestionId, N'Slow down and increase following distance', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What should you do?', N'Children may move unpredictably. Stop and give them enough time to cross safely.', N'Content/images/questions/0ee543043708429dbb3e9591c110a055.jpg', N'Child crossing the road', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Sound your horn and continue', 1, 0),
        (@QuestionId, N'Stop and allow the child to cross', 2, 1),
        (@QuestionId, N'Overtake the vehicle ahead', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What should you do first?', N'Check your mirrors, signal your intention and check your blind spot before moving into another lane.', N'Content/images/questions/5d8ea39a772249ab8aa3adf558fce905.jpg', N'Changing lanes', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Brake suddenly', 1, 0),
        (@QuestionId, N'Turn immediately', 2, 0),
        (@QuestionId, N'Check mirrors and blind spots, then signal', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What should you do?', N'Move to a safe location if possible, switch on hazard warning lights and arrange assistance.', N'Content/images/questions/6ff5b70726f84567a0987c99213804a4.jpg', N'Vehicle breakdown', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Leave the vehicle in the middle lane', 1, 0),
        (@QuestionId, N'Stop in a safe place and seek assistance', 2, 1),
        (@QuestionId, N'Continue driving at high speed', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What should you do?', N'Slow down, check for vehicles already circulating and enter when there is a safe gap.', N'Content/images/questions/4a3ded625c3c4510ba948569e9d453c4.jpg', N'Approaching a roundabout', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Enter without checking', 1, 0),
        (@QuestionId, N'Stop permanently', 2, 0),
        (@QuestionId, N'Give way to circulating traffic', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What is the safest action?', N'Large vehicles have blind spots and can block your view of the road ahead. Keep a safe distance.', N'Content/images/questions/add0a8e00f2a425ebf1659c91524316f.jpg', N'ss', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Follow very closely', 1, 0),
        (@QuestionId, N'Drive in the lorry''s blind spot', 2, 0),
        (@QuestionId, N'Maintain a safe distance', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'The warning lights are flashing. What should you do?', N'Flashing warning lights indicate danger. Stop and wait until the signals indicate it is safe to cross.', N'Content/images/questions/0bd11252454a4a6986a779afac811c7a.jpg', N'ss', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Stop and wait', 1, 1),
        (@QuestionId, N'Drive around the barrier', 2, 0),
        (@QuestionId, N'Speed across', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What should you do?', N'Sleepiness reduces concentration and reaction time. Rest before continuing your journey.', N'Content/images/questions/ca43df1397704b4ab73c50a854dff2ae.jpg', N'sleep', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Increase the radio volume', 1, 0),
        (@QuestionId, N'Drive faster', 2, 0),
        (@QuestionId, N'Stop somewhere safe and rest', 3, 1);

END

SET @CategoryId = NULL;

-- ----------------------------------------------------------------------------
-- rules-of-the-road: Practice Set 2
-- ----------------------------------------------------------------------------
SELECT @CategoryId = QuestionCategoryId FROM QuestionCategories WHERE Code = N'rules-of-the-road';
IF @CategoryId IS NULL THROW 50010, 'Question category rules-of-the-road is missing. Start the application once so it creates and seeds the schema.', 1;

IF NOT EXISTS (SELECT 1 FROM PracticeSets WHERE QuestionCategoryId = @CategoryId AND SetNumber = 2)
    INSERT INTO PracticeSets (QuestionCategoryId, SetNumber, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, 2, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

SELECT @SetId = PracticeSetId FROM PracticeSets WHERE QuestionCategoryId = @CategoryId AND SetNumber = 2;

IF NOT EXISTS (SELECT 1 FROM Questions WHERE PracticeSetId = @SetId)
BEGIN
    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'Overtaking when approaching a sharp bend', N'Slow down before entering the bend. This helps maintain control and reduces the risk of skidding.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Overtake another vehicle', 1, 0),
        (@QuestionId, N'Reduce speed before the bend', 2, 1),
        (@QuestionId, N'Accelerate sharply', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What should you do when emergency vehicle behind you?', N'Move aside when safe and allow the emergency vehicle to pass.', N'Content/images/questions/538d7d9d8fb442c6b8b6f4702e08b082.jpg', N'dd', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Block its path', 1, 0),
        (@QuestionId, N'Ignore it', 2, 0),
        (@QuestionId, N'Give way safely', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'Pedestrians are crossing. What should you do?', N'Give pedestrians priority at the crossing. Do not overtake a vehicle that has stopped for pedestrians.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Stop and let them cross', 1, 1),
        (@QuestionId, N'Accelerate', 2, 0),
        (@QuestionId, N'Overtake another car', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What should you do?', N'Use low-beam headlights or appropriate fog lights. High beams can reflect off fog and make visibility worse.', N'Content/images/questions/ff35805f34c04c148a89671ffaad8691.jpg', N'Use high beam continuously', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Use high beam continuously', 1, 0),
        (@QuestionId, N'Slow down and use appropriate lights', 2, 1),
        (@QuestionId, N'Switch off all lights', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What should you do when you missed highway exit?', N'Never reverse or make a U-turn on a highway. Continue safely and take the next available exit.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Reverse on the highway', 1, 0),
        (@QuestionId, N'Make a U-turn', 2, 0),
        (@QuestionId, N'Continue to the next exit', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'Why should you maintain a safe following distance?', N'A safe following distance gives you enough time to react and brake if the vehicle ahead stops suddenly.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'To have time to react', 1, 1),
        (@QuestionId, N'To prevent overtaking', 2, 0),
        (@QuestionId, N'To drive faster', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What should you do?', N'Children may cross in front of or behind the bus. Be prepared to stop.', N'Content/images/questions/0ed0515b18e64762b87254ef3da860ee.jpg', N'ee', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Overtake immediately', 1, 0),
        (@QuestionId, N'Slow down and watch for children', 2, 1),
        (@QuestionId, N'Sound the horn continuously', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What you should do when tyre suddenly bursts?', N'Keep the vehicle steady, release the accelerator gradually and move to a safe place. Avoid sudden braking or steering.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Brake as hard as possible', 1, 0),
        (@QuestionId, N'Turn the steering sharply', 2, 0),
        (@QuestionId, N'Hold the steering firmly and slow down gradually', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'The traffic light is green, but the officer signals you to stop. What should you do?', N'Follow the lawful directions of traffic police, even when they differ from the traffic light.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Obey the officer', 1, 1),
        (@QuestionId, N'Follow the green light', 2, 0),
        (@QuestionId, N'Ignore the officer', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What should you do?', N'Apply the parking brake and select the appropriate gear or Park mode to prevent the vehicle from rolling.', N'Content/images/questions/b3bd272c8c9f4d1e97ff517279a3a4c5.jpg', N'ss', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Leave the car in neutral without securing it', 1, 0),
        (@QuestionId, N'Apply the parking brake and secure the vehicle', 2, 1),
        (@QuestionId, N'Leave the engine running', 3, 0);

END

SET @CategoryId = NULL;

-- ----------------------------------------------------------------------------
-- rules-of-the-road: Practice Set 3
-- ----------------------------------------------------------------------------
SELECT @CategoryId = QuestionCategoryId FROM QuestionCategories WHERE Code = N'rules-of-the-road';
IF @CategoryId IS NULL THROW 50010, 'Question category rules-of-the-road is missing. Start the application once so it creates and seeds the schema.', 1;

IF NOT EXISTS (SELECT 1 FROM PracticeSets WHERE QuestionCategoryId = @CategoryId AND SetNumber = 3)
    INSERT INTO PracticeSets (QuestionCategoryId, SetNumber, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, 3, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

SELECT @SetId = PracticeSetId FROM PracticeSets WHERE QuestionCategoryId = @CategoryId AND SetNumber = 3;

IF NOT EXISTS (SELECT 1 FROM Questions WHERE PracticeSetId = @SetId)
BEGIN
    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What you should do when oncoming vehicle at night?', N'Avoid staring at bright headlights. Keep your vehicle within your lane and reduce speed if visibility is affected.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Switch on high beam directly at the driver', 1, 0),
        (@QuestionId, N'Close your eyes', 2, 0),
        (@QuestionId, N'Avoid looking directly at the glare and reduce speed if needed', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What should you do if you are the Car A?', N'Traffic on the main road generally has priority. Wait until there is a safe gap before joining.', N'Content/images/questions/b53a98d4871f49a585ab7c42c580189b.jpg', N'cc', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Give way to Car B whic is on the main road', 1, 1),
        (@QuestionId, N'Enter immediately', 2, 0),
        (@QuestionId, N'Sound your horn and enter', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What should you do?', N'Deep water can damage your vehicle or cause loss of control. Never drive through floodwater if its depth or safety is uncertain.', N'Content/images/questions/f03e4ab146d24148886bb8692d1172e1.jpg', N'cc', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Accelerate through it', 1, 0),
        (@QuestionId, N'Slow down and assess the water depth', 2, 1),
        (@QuestionId, N'Brake suddenly inside the puddle', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What should you do when motorcycle beside your vehicle before changing lanes?', N'Motorcycles can be hidden in blind spots. Check carefully before changing lanes.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Change lanes immediately', 1, 0),
        (@QuestionId, N'Ignore the motorcycle', 2, 0),
        (@QuestionId, N'Check mirrors and blind spots', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'Your exit is blocked by traffic. What should you do?', N'Keep the yellow box clear. Enter only when there is sufficient space to clear the junction.', N'Content/images/questions/24476f1536594adfa63f339c3a1afb4f.jpg', N'cc', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Wait before entering the yellow box', 1, 1),
        (@QuestionId, N'Enter and block the junction', 2, 0),
        (@QuestionId, N'Stop diagonally inside the box', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What should you do?', N'A lower gear helps with engine braking and reduces reliance on the footbrake.', N'Content/images/questions/fc675fb752c54db093c34172a3fd25d0.jpg', N'dd', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Switch off the engine', 1, 0),
        (@QuestionId, N'Use an appropriate lower gear to control speed', 2, 1),
        (@QuestionId, N'Put the vehicle in neutral', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What should you do when you want to reverse car?', N'Check mirrors, blind spots and the area behind your vehicle. Reverse slowly and stop if anyone enters your path.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Reverse quickly', 1, 0),
        (@QuestionId, N'Look only at the screen', 2, 0),
        (@QuestionId, N'Check all around the vehicle', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What you should do when spotted an accident ahead?', N'Avoid causing another accident. Keep the road clear for emergency vehicles.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Slow down safely and avoid obstructing traffic', 1, 1),
        (@QuestionId, N'Stop in the middle of the road to watch', 2, 0),
        (@QuestionId, N'Take photographs while driving', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What should you do?', N'A green light does not give permission to block an intersection. Wait until you can proceed without obstructing traffic.', N'Content/images/questions/9c97233e56b34bd4bf681f45ba6a29cb.jpg', N'ss', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Enter because the light is green', 1, 0),
        (@QuestionId, N'Wait until there is room to clear the junction', 2, 1),
        (@QuestionId, N'Block the pedestrian crossing', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What should you do when car behind intend to pass you?', N'Do not compete with an overtaking vehicle. Maintain a predictable speed and give it room to complete the manoeuvre safely.', N'Content/images/questions/e661c9e3059f41ecad3d1a20378415dd.jpg', N'ff', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Accelerate to stop it', 1, 0),
        (@QuestionId, N'Move suddenly to the right', 2, 0),
        (@QuestionId, N'Maintain a steady speed and allow it to pass safely', 3, 1);

END

SET @CategoryId = NULL;

-- ----------------------------------------------------------------------------
-- kejara-safety: Practice Set 1
-- ----------------------------------------------------------------------------
SELECT @CategoryId = QuestionCategoryId FROM QuestionCategories WHERE Code = N'kejara-safety';
IF @CategoryId IS NULL THROW 50010, 'Question category kejara-safety is missing. Start the application once so it creates and seeds the schema.', 1;

IF NOT EXISTS (SELECT 1 FROM PracticeSets WHERE QuestionCategoryId = @CategoryId AND SetNumber = 1)
    INSERT INTO PracticeSets (QuestionCategoryId, SetNumber, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

SELECT @SetId = PracticeSetId FROM PracticeSets WHERE QuestionCategoryId = @CategoryId AND SetNumber = 1;

IF NOT EXISTS (SELECT 1 FROM Questions WHERE PracticeSetId = @SetId)
BEGIN
    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What is the main purpose of KEJARA?', N'KEJARA records demerit points for specified traffic offences to encourage responsible driving.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Reward drivers for buying new vehicles', 1, 0),
        (@QuestionId, N'Monitor and discourage traffic offences', 2, 1),
        (@QuestionId, N'Give free driving licences', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'When are KEJARA demerit points recorded?', N'Receiving a summons alone does not start the demerit-point calculation.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Immediately when a summons is issued', 1, 0),
        (@QuestionId, N'When the driver pays the compound or fine, or receives a court sentence', 2, 1),
        (@QuestionId, N'When the vehicle is renewed', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What happens if a driver commits no scheduled offence for 12 months after the last offence?', N'JPJ provides a 50% point-reduction incentive under the specified conditions.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'May receive a 50% reduction in demerit points', 1, 1),
        (@QuestionId, N'Automatically received a new licence', 2, 0),
        (@QuestionId, N'Must retake the driving test', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'How many times can the 50% reduction incentive be given?', N'The reduction incentive is limited to once before the relevant suspension or cancellation.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'A. Every month', 1, 0),
        (@QuestionId, N'B. Unlimited times', 2, 0),
        (@QuestionId, N'C. Once before suspension or cancellation', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'Can a driver receive a 50% reduction through a JPJ rehabilitation course?', N'JPJ may offer a rehabilitation course as one of the conditions for the reduction incentive.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Yes, if offered by JPJ', 1, 1),
        (@QuestionId, N'No, never', 2, 0),
        (@QuestionId, N'Only by paying a fine', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'Which of the following offences can attract KEJARA points?', N'Running a red light is one of the scheduled offences.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Wearing a seat belt', 1, 0),
        (@QuestionId, N'Running a red traffic light', 2, 1),
        (@QuestionId, N'Checking your mirrors', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What is the purpose of a KEJARA warning?', N'A warning informs the driver that their demerit record requires attention.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'To inform drivers about accumulated demerit points', 1, 1),
        (@QuestionId, N'To give drivers permission to speed', 2, 0),
        (@QuestionId, N'To cancel vehicle insurance', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'Can a driver accumulate demerit points while their licence is suspended?', N'JPJ states that points can still be awarded for scheduled offences committed during a suspension.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'No', 1, 0),
        (@QuestionId, N'Yes', 2, 1),
        (@QuestionId, N'Only on public holidays', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'Where can drivers check their KEJARA points?', N'Drivers can check their demerit records through JPJ''s available checking services.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'MyJPJ or JPJ public services', 1, 1),
        (@QuestionId, N'Petrol station', 2, 0),
        (@QuestionId, N'Shopping centre', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'Does KEJARA apply to both PDL and CDL holders?', N'Both probationary and competent licence holders are covered, although their consequences differ.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'A. Yes', 1, 1),
        (@QuestionId, N'B. Only CDL holders', 2, 0),
        (@QuestionId, N'C. Only motorcycle riders', 3, 0);

END

SET @CategoryId = NULL;

-- ----------------------------------------------------------------------------
-- kejara-safety: Practice Set 2
-- ----------------------------------------------------------------------------
SELECT @CategoryId = QuestionCategoryId FROM QuestionCategories WHERE Code = N'kejara-safety';
IF @CategoryId IS NULL THROW 50010, 'Question category kejara-safety is missing. Start the application once so it creates and seeds the schema.', 1;

IF NOT EXISTS (SELECT 1 FROM PracticeSets WHERE QuestionCategoryId = @CategoryId AND SetNumber = 2)
    INSERT INTO PracticeSets (QuestionCategoryId, SetNumber, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, 2, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

SELECT @SetId = PracticeSetId FROM PracticeSets WHERE QuestionCategoryId = @CategoryId AND SetNumber = 2;

IF NOT EXISTS (SELECT 1 FROM Questions WHERE PracticeSetId = @SetId)
BEGIN
    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What is the consequence when a PDL holder reaches 20 demerit points?', N'A Probationary Driving Licence (PDL) can be cancelled when the accumulated points reach 20 or more.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Receives a free licence', 1, 0),
        (@QuestionId, N'Licence is cancelled', 2, 1),
        (@QuestionId, N'Licence is automatically renewed', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What is the first suspension period for a CDL holder under the JPJ KEJARA ?', N'JPJ''s FAQ specifies a first suspension period of 3-6 months.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'1-2 months', 1, 0),
        (@QuestionId, N'3-6 months', 2, 1),
        (@QuestionId, N'12-18 months', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What is the second suspension period for a CDL holder?', N'The second suspension period is 6-8 months.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'6-8 months', 1, 1),
        (@QuestionId, N'1-3 months', 2, 0),
        (@QuestionId, N'12-15 months', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What is the third suspension period for a CDL holder?', N'JPJ specifies 8-10 months for the third suspension.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'2-4 months', 1, 0),
        (@QuestionId, N'6-8 months', 2, 0),
        (@QuestionId, N'8-10 months', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What happens if a CDL holder is suspended three times within five years under KEJARA?', N'JPJ can cancel a CDL after three suspensions within a five-year period.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Licence may be cancelled', 1, 1),
        (@QuestionId, N'Licence becomes permanent', 2, 0),
        (@QuestionId, N'Driver receives a new PDL automatically', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What happens after a driving licence is cancelled under KEJARA?', N'The driver must wait 12 months and restart the required licensing process.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Continue driving normally', 1, 0),
        (@QuestionId, N'Apply for a new licence after the prescribed 12-month period', 2, 1),
        (@QuestionId, N'Renew the cancelled licence immediately', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'Can a driver operate a vehicle while their licence is suspended?', N'Driving while suspended or cancelled is an offence.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Yes, during daytime', 1, 0),
        (@QuestionId, N'Yes, for short distances', 2, 0),
        (@QuestionId, N'No', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What is the maximum fine stated by JPJ for driving while a licence is suspended or cancelled?', N'The stated penalty is a fine of RM3,000-RM10,000 or imprisonment of up to three years, or both as provided by law.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'RM500', 1, 0),
        (@QuestionId, N'RM10,000', 2, 1),
        (@QuestionId, N'RM1,000', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What happens to a GDL or PSV licence if the holder''s CDL is suspended or cancelled?', N'Suspension or cancellation of the CDL also affects the vocational licence.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'It is also affected', 1, 1),
        (@QuestionId, N'It remains completely unaffected', 2, 0),
        (@QuestionId, N'It automatically becomes a PDL', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'Can a driver apply for another driving licence during the suspension period?', N'JPJ states that the driver cannot apply for another licence class during the suspension or cancellation period.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Yes, any class', 1, 0),
        (@QuestionId, N'No', 2, 1),
        (@QuestionId, N'Only a motorcycle licence', 3, 0);

END

SET @CategoryId = NULL;

-- ----------------------------------------------------------------------------
-- kejara-safety: Practice Set 3
-- ----------------------------------------------------------------------------
SELECT @CategoryId = QuestionCategoryId FROM QuestionCategories WHERE Code = N'kejara-safety';
IF @CategoryId IS NULL THROW 50010, 'Question category kejara-safety is missing. Start the application once so it creates and seeds the schema.', 1;

IF NOT EXISTS (SELECT 1 FROM PracticeSets WHERE QuestionCategoryId = @CategoryId AND SetNumber = 3)
    INSERT INTO PracticeSets (QuestionCategoryId, SetNumber, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, 3, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

SELECT @SetId = PracticeSetId FROM PracticeSets WHERE QuestionCategoryId = @CategoryId AND SetNumber = 3;

IF NOT EXISTS (SELECT 1 FROM Questions WHERE PracticeSetId = @SetId)
BEGIN
    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'How many KEJARA points can be given for running a red traffic light?', N'Running a red light is a serious traffic offence and carries 10 demerit points in the published government examples.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'3 points', 1, 0),
        (@QuestionId, N'6 points', 2, 0),
        (@QuestionId, N'10 points', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'How many KEJARA points can be given for not wearing a seat belt?', N'Not wearing a seat belt is a listed example of an offence carrying 3 demerit points.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'3 points', 1, 1),
        (@QuestionId, N'10 points', 2, 0),
        (@QuestionId, N'15 points', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'How many KEJARA points can be given for using a mobile phone while driving?', N'Using a mobile phone while driving is listed as an offence carrying 10 demerit points.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'2 points', 1, 0),
        (@QuestionId, N'10 points', 2, 1),
        (@QuestionId, N'5 points', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'How many KEJARA points can speeding attract?', N'The published government guidance gives a range of 6-8 points for speeding, depending on the offence category.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'1-2 points', 1, 0),
        (@QuestionId, N'3-4 points', 2, 0),
        (@QuestionId, N'6-8 points', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'Which offence carries 10 KEJARA points in the published examples?', N'Running a red traffic light carries 10 points in the government guidance.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Running a red light', 1, 1),
        (@QuestionId, N'Wearing a seat belt', 2, 0),
        (@QuestionId, N'Checking your mirrors', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What happens if a driver ignores a notice of licence suspension?', N'JPJ states that suspension or cancellation can take effect after the applicable 21-day period even if the physical licence has not been surrendered.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'They can continue driving', 1, 0),
        (@QuestionId, N'The suspension can still take effect', 2, 1),
        (@QuestionId, N'Their points disappear', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'How long does a driver have to surrender their physical licence after receiving the relevant suspension or cancellation notice?', N'JPJ specifies a 21-day period for surrendering the physical licence after service of the notice.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'7 days', 1, 0),
        (@QuestionId, N'14 days', 2, 0),
        (@QuestionId, N'21 days', 3, 1);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'Can a driver appeal simply to request a reduction of accumulated KEJARA points?', N'JPJ''s FAQ says the 50% reduction incentive is subject to specific conditions, not an ordinary appeal.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Yes, whenever they want', 1, 0),
        (@QuestionId, N'No, not through a general appeal for point reduction', 2, 1),
        (@QuestionId, N'Yes, by paying an additional fee', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What should a driver do to maintain a good KEJARA record?', N'Safe and responsible driving helps prevent demerit points and licence penalties.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'Follow traffic laws and avoid scheduled offences', 1, 1),
        (@QuestionId, N'Ignore speed limits', 2, 0),
        (@QuestionId, N'Drive aggressively', 3, 0);

    INSERT INTO Questions (QuestionCategoryId, PracticeSetId, QuestionText, Explanation, ImagePath, ImageAltText, IsActive, CreatedAt, UpdatedAt)
    VALUES (@CategoryId, @SetId, N'What is the main objective of KEJARA punishment?', N'The system aims to discourage traffic offences and improve road safety for everyone.', NULL, N'', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @QuestionId = SCOPE_IDENTITY();
    INSERT INTO QuestionOptions (QuestionId, OptionText, OptionOrder, IsCorrect) VALUES
        (@QuestionId, N'To encourage drivers to drive faster', 1, 0),
        (@QuestionId, N'To increase vehicle prices', 2, 0),
        (@QuestionId, N'To improve road discipline and reduce dangerous driving', 3, 1);

END

SET @CategoryId = NULL;

COMMIT TRANSACTION;

SELECT (SELECT COUNT(*) FROM PracticeSets) AS PracticeSets,
       (SELECT COUNT(*) FROM Questions) AS Questions,
       (SELECT COUNT(*) FROM QuestionOptions) AS QuestionOptions;
