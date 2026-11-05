-- KPP fourth edition (2025), English learning curriculum.
-- One-time transactional import. Existing lessons/progress are retained under archived chapters.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @LockResult int;
EXEC @LockResult = sys.sp_getapplock @Resource=N'KppEnglishCurriculum2025', @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=30000;
IF @LockResult < 0 THROW 51000, 'Could not acquire curriculum migration lock.', 1;
IF OBJECT_ID('dbo.CurriculumImports', 'U') IS NULL
    CREATE TABLE dbo.CurriculumImports (Version nvarchar(80) NOT NULL PRIMARY KEY, ImportedAt datetime2 NOT NULL);
IF NOT EXISTS (SELECT 1 FROM dbo.CurriculumImports WHERE Version=N'kpp-2025-english-v1')
BEGIN
    IF OBJECT_ID('dbo.KppPreviousChapters', 'U') IS NULL
        CREATE TABLE dbo.KppPreviousChapters (ChapterId int NOT NULL PRIMARY KEY, DisplayOrder int NOT NULL, IsActive bit NOT NULL);
    INSERT dbo.KppPreviousChapters (ChapterId, DisplayOrder, IsActive)
        SELECT ChapterId, DisplayOrder, IsActive FROM dbo.LearningChapters;
    DECLARE @MaxOrder int = (SELECT ISNULL(MAX(DisplayOrder),0) FROM dbo.LearningChapters);
    IF @MaxOrder > 2147480000 THROW 51001, 'Chapter orders cannot be safely archived.', 1;
    -- Move every old chapter above both the old range and the new 1..10 range.
    SET @MaxOrder = CASE WHEN @MaxOrder < 10 THEN 10 ELSE @MaxOrder END;
    ;WITH Archived AS (
        SELECT ChapterId, ROW_NUMBER() OVER(ORDER BY DisplayOrder, ChapterId) AS RowNumber FROM dbo.LearningChapters
    )
    UPDATE c SET DisplayOrder=@MaxOrder+a.RowNumber, IsActive=0, UpdatedAt=SYSUTCDATETIME()
        FROM dbo.LearningChapters c INNER JOIN Archived a ON a.ChapterId=c.ChapterId;
    DECLARE @ChapterId int;
    INSERT dbo.LearningChapters(Title,Description,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(N'Malaysian Driving Licences',N'Understand eligibility, licence types and the journey from learner to competent driver.',1,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    SET @ChapterId=CONVERT(int,SCOPE_IDENTITY());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Who Can Apply for a Driving Licence?',N'**What you will learn**
- Eligibility
- Health and vision
- Application documents

**Who Can Apply for a Driving Licence?**
A driving licence confirms that you have met the requirements for a particular vehicle class. The 2025 KPP edition describes minimum ages of 16 for motorcycles and 17 for cars. Age alone does not establish eligibility: applicants must also satisfy the relevant health and vision requirements. Applicants with disabilities follow a medical assessment and approved vehicle-adaptation pathway. Foreign applicants must meet the document and residency conditions applicable to their application.

**Check your understanding**
You are old enough for a car licence. Can you skip the health and vision requirements?

**Answer and explanation**
No. Age, health, vision and application requirements must all be satisfied.

**Key reminder**
Eligibility → Health and vision → Application documents.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 8–9. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/1-1-illustration.png',N'Below',1,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Understanding Licence Types and Classes',N'**What you will learn**
- LDL: learning
- PDL: probation
- CDL: competent

**Understanding Licence Types and Classes**
An LDL is a learner licence used during the learning stage. A PDL is issued after the required practical tests and carries probationary responsibilities. A CDL follows successful completion of the probationary stage. A licence type describes your licensing stage or purpose; a vehicle class describes what you may drive. D and DA distinguish car permissions, including automatic-only vehicles. Motorcycle classes are separate. Vocational licences, armed-forces licences and international driving permits serve different purposes and should not be treated as interchangeable.

**Check your understanding**
Does a car licence automatically authorise every motorcycle?

**Answer and explanation**
No. Check the vehicle class shown on the licence.

**Key reminder**
LDL: learning → PDL: probation → CDL: competent.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 9–22. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/1-2-illustration-v2.png',N'Below',2,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Your Journey from Learner to Competent Driver',N'**What you will learn**
- Theory and assessment
- Practical training
- Probation and progression

**Your Journey from Learner to Competent Driver**
Begin with the theory course and assessment, then obtain the learner licence before progressing through practical training. Circuit training develops vehicle-control skills; road training develops observation, hazard management and safe interaction with traffic. A pre-test assessment prepares candidates for the practical tests. Successful candidates progress to a probationary licence and later to a competent licence after satisfying the applicable conditions. Car and motorcycle training contain different exercises and training-hour requirements. This learning module supports theory study; it does not replace supervised practical instruction.

**Check your understanding**
Why are both circuit and road training included?

**Answer and explanation**
Circuit training develops control; road training applies that control to changing traffic and hazards.

**Key reminder**
Theory and assessment → Practical training → Probation and progression.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 11–19. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/1-3-illustration.png',N'Below',3,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Licence Holder Responsibilities',N'**What you will learn**
- Display correct plates
- Observe licence conditions
- Renew and progress

**Licence Holder Responsibilities**
Learner drivers must follow the conditions attached to their licence, including appropriate supervision and restrictions on passengers or loads. Display the required L or P identification in the specified positions for the vehicle. The PDF illustrates different placement for a car and a motorcycle. A probationary licence comes with additional responsibilities and consequences for offences. Check validity dates and the procedure for progressing or adding a class rather than assuming that a new entitlement is automatic.

**Check your understanding**
Does passing a practical test remove all probationary responsibilities?

**Answer and explanation**
No. The probationary stage has its own conditions, identification requirements and consequences.

**Key reminder**
Display correct plates → Observe licence conditions → Renew and progress.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 10–16,21–22. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/1-4-illustration.png',N'Below',4,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Foreign Licences and Accessible Driving',N'**What you will learn**
- Check recognition
- Confirm the pathway
- Use approved adaptations

**Foreign Licences and Accessible Driving**
Foreign licence recognition depends on the applicable arrangements, the licence and supporting documents. An international driving permit is not a substitute for understanding the underlying licence entitlement. Singapore-related provisions are treated separately in the source. For applicants with disabilities, the PDF describes assessment, documentation and approved vehicle adaptations, together with identification placement. These are distinct pathways: select the one that matches the applicant rather than applying a general rule to everyone. Confirm current application details with JPJ before applying.

**Check your understanding**
Should every foreign licence holder follow the same conversion procedure?

**Answer and explanation**
No. Recognition, applicant category and current JPJ requirements determine the appropriate pathway.

**Key reminder**
Check recognition → Confirm the pathway → Use approved adaptations.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 21–29. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/1-5-illustration.png',N'Below',5,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.LearningChapters(Title,Description,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(N'Traffic Signals and Road Markings',N'Interpret lights, hand signals and road markings and connect them to safe decisions.',2,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    SET @ChapterId=CONVERT(int,SCOPE_IDENTITY());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'How to Read the Road',N'**What you will learn**
- Recognise
- Understand
- Respond

**How to Read the Road**
Road signs and signals organise traffic, warn of hazards and guide road users. Shape, colour and symbol work together; do not rely on colour alone. Warning, prohibitory, mandatory, information and temporary signs have different purposes. Once you recognise a sign, use the information to choose a safe position and speed. Continue observing the actual road because a sign does not describe every possible hazard. Following another driver who ignores a sign does not excuse an unsafe decision.

**Check your understanding**
Is recognising a sign enough?

**Answer and explanation**
No. You must understand its meaning and choose the appropriate action.

**Key reminder**
Recognise → Understand → Respond.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 30–32. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/2-1-illustration.png',N'Below',1,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Traffic Lights',N'**What you will learn**
- Read the signal
- Check the junction
- Proceed safely

**Traffic Lights**
Traffic lights control conflicting movements. Read the signal applying to your lane and direction, including arrows. A green indication does not remove the need to check that the junction is clear and that other road users are not creating a conflict. Stop appropriately for a red indication and understand the source''s amber guidance as a warning to stop unless stopping is unsafe. If signals fail, approach cautiously and follow applicable junction controls and authorised traffic directions; do not assume priority.

**Check your understanding**
The light turns green but your exit is blocked. Should you enter?

**Answer and explanation**
No. Wait until you can proceed safely without blocking the junction.

**Key reminder**
Read the signal → Check the junction → Proceed safely.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 33–34. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/2-2-illustration.png',N'Below',2,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Police and Road User Hand Signals',N'**What you will learn**
- Observe the gesture
- Identify its direction
- Respond cautiously

**Police and Road User Hand Signals**
Traffic police use hand and arm positions to stop or release traffic from particular directions. Learn each gesture in relation to the officer and your approach, rather than memorising an isolated pose. Drivers and riders may also use hand signals to communicate turning or slowing. A signal communicates intention, not a guarantee that the movement is safe. Look for other road users and confirm that the intended path is clear before acting.

**Check your understanding**
A rider signals a turn. Does that guarantee the rider will turn immediately?

**Answer and explanation**
No. Treat it as information about intention and continue observing.

**Key reminder**
Observe the gesture → Identify its direction → Respond cautiously.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 35–36. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/2-3-illustration.png',N'Below',3,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Lines, Crossings, and Lane Markings',N'**What you will learn**
- Read lane lines
- Respect reserved areas
- Keep crossings clear

**Lines, Crossings, and Lane Markings**
Broken, continuous and mixed lines communicate different restrictions. With a mixed line, the marking on your side matters. Road-edge lines, yellow markings, pedestrian crossings and reserved lanes also have specific purposes. Yellow boxes and hatched or chevron areas are not spare space for queuing or overtaking. Learn the layout and permitted movement together. Before changing lanes, combine the markings with signs, mirrors, signalling and a blind-spot check.

**Check your understanding**
Can an empty hatched area be used as an extra driving lane?

**Answer and explanation**
No. Its markings define its purpose and restrictions; empty space is not permission to enter.

**Key reminder**
Read lane lines → Respect reserved areas → Keep crossings clear.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 37–39. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/2-4-illustration.png',N'Below',4,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.LearningChapters(Title,Description,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(N'Road Signs and Their Meanings',N'Recognise warning, prohibitory, mandatory, information and temporary signs.',3,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    SET @ChapterId=CONVERT(int,SCOPE_IDENTITY());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Warning Signs: Road Layout and Surface',N'**What you will learn**
- Identify the hazard
- Reduce risk early
- Maintain control

**Warning Signs: Road Layout and Surface**
Warning signs announce hazards such as junctions, bends, narrowing roads, steep gradients, uneven surfaces, slippery roads and soft shoulders. Similar-looking signs can describe different layouts, so examine the symbol carefully. Adjust speed before reaching the hazard, choose a suitable position and maintain space. A bend warning is a prompt to plan rather than to brake sharply inside the bend. A slippery-road warning calls for smooth control and greater caution.

**Check your understanding**
What should you do when a bend warning appears?

**Answer and explanation**
Prepare early: assess visibility, reduce speed as needed and maintain a safe position before the bend.

**Key reminder**
Identify the hazard → Reduce risk early → Maintain control.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 40–53. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/3-1-illustration.png',N'Below',1,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Warning Signs: People, Animals, and Crossings',N'**What you will learn**
- Look beyond the sign
- Anticipate movement
- Be ready to stop

**Warning Signs: People, Animals, and Crossings**
Schools, playgrounds, pedestrian crossings, disabled road users, animals and railway crossings require anticipation. A warning identifies a place where someone or something may enter your path, including from behind an obstruction. Look beyond the sign toward the crossing or roadside. Reduce risk before the hazard develops. At railway crossings, observe the controls and ensure the route is clear rather than following another vehicle without checking.

**Check your understanding**
A school warning is visible but no children are on the road. Can you ignore it?

**Answer and explanation**
No. Children may emerge unexpectedly; adjust your observation and speed to the situation.

**Key reminder**
Look beyond the sign → Anticipate movement → Be ready to stop.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 52–56. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/3-2-illustration.png',N'Below',2,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Prohibitory Signs and Restrictions',N'**What you will learn**
- Identify the restriction
- Check its scope
- Comply

**Prohibitory Signs and Restrictions**
Prohibitory signs restrict entry, overtaking, turning, stopping, parking or particular vehicles. No Parking and No Stopping are different restrictions. Speed, height, width and weight limits require careful reading of the value and unit. A vehicle that fits physically may still be prohibited by a restriction. Keep authentic Malay wording such as BERHENTI on sign examples and learn its English meaning: STOP. Always interpret a sign with any supplementary plate.

**Check your understanding**
Are No Parking and No Stopping interchangeable?

**Answer and explanation**
No. They impose different restrictions; read the symbol and any supplementary information.

**Key reminder**
Identify the restriction → Check its scope → Comply.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 57–62. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/3-3-illustration-v2.png',N'Below',3,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Mandatory Signs',N'**What you will learn**
- Required direction
- Correct lane
- Permitted users

**Mandatory Signs**
Mandatory signs specify an action or route to follow. Arrows may require keeping to one side, proceeding in a particular direction or using a designated route. Special lanes and facilities are intended for the road users shown. Match the sign to lane markings and choose your lane early. If you discover that you are in the wrong lane, avoid a sudden manoeuvre across other traffic.

**Check your understanding**
You are in a lane with a required direction that differs from your intended route. What matters most?

**Answer and explanation**
Follow the controls safely and adjust your route instead of making an abrupt prohibited movement.

**Key reminder**
Required direction → Correct lane → Permitted users.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 63–66. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/3-4-illustration-v2.png',N'Below',4,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Information and Direction Signs',N'**What you will learn**
- Read destinations
- Plan your lane
- Locate services

**Information and Direction Signs**
Information signs identify destinations, junctions, services and facilities. The source includes parking, medical services, fuel, charging, dead ends and route guidance. Read destination and exit information early enough to make a safe lane choice. Service symbols help you plan stops but do not remove the need to use the correct entrance. Missing an exit is a route-planning issue, not a reason to reverse or cut across lanes.

**Check your understanding**
What is the safe response to missing an exit?

**Answer and explanation**
Continue safely and use a suitable later route; do not reverse or make a sudden lane change.

**Key reminder**
Read destinations → Plan your lane → Locate services.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 67–71. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/3-5-illustration.png',N'Below',5,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Temporary Signs and Roadworks',N'**What you will learn**
- Expect changed conditions
- Follow temporary controls
- Protect workers

**Temporary Signs and Roadworks**
Temporary signs warn of roadworks, diversions, narrowed lanes, closures and changed traffic flow. Work-zone hazards may include workers, machinery, loose surfaces and vehicles entering the road. Follow temporary controls and authorised flag directions. Prepare early for a lane closure and merge only when safe. Do not assume your usual route or lane layout is unchanged. Maintain observation until you have completely passed the work area.

**Check your understanding**
Why should a familiar road still receive extra attention during works?

**Answer and explanation**
The layout, surface and traffic controls may have changed, and workers or machinery may enter the road.

**Key reminder**
Expect changed conditions → Follow temporary controls → Protect workers.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 72–78. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/3-6-illustration.png',N'Below',6,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.LearningChapters(Title,Description,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(N'Responsible Driving and KEJARA',N'Build responsible road behaviour and understand the purpose of demerit consequences.',4,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    SET @ChapterId=CONVERT(int,SCOPE_IDENTITY());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Driver Ethics and Personal Responsibility',N'**What you will learn**
- Be patient
- Consider others
- Accept responsibility

**Driver Ethics and Personal Responsibility**
Responsible driving combines knowledge, vehicle-control skills and considerate behaviour. Patience and discipline help prevent risky decisions when traffic is slow or another person makes a mistake. Avoid retaliation and allow space for vulnerable road users. A safe driver follows controls even when enforcement is not visible. Responsibility also means recognising personal limitations and choosing not to drive when unfit.

**Check your understanding**
Another driver cuts in front of you. Should you close the gap to punish them?

**Answer and explanation**
No. Restore a safe gap and avoid escalating the situation.

**Key reminder**
Be patient → Consider others → Accept responsibility.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 100–101. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/4-1-illustration.png',N'Below',1,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Integrity During Driver Training',N'**What you will learn**
- Attend honestly
- Reject improper influence
- Respect others

**Integrity During Driver Training**
Training and assessment depend on honest participation. Do not falsify attendance, impersonate another candidate or ask someone to act in your place. Bribery, rewards for guaranteed results and improper outside influence undermine competent licensing. Follow attendance and dress requirements, treat instructors and candidates respectfully and avoid harassment. The purpose of training is to gain real competence, not merely a certificate.

**Check your understanding**
Someone offers a guaranteed pass for a payment outside the normal process. Is this acceptable?

**Answer and explanation**
No. Use legitimate training and assessment procedures and reject improper influence.

**Key reminder**
Attend honestly → Reject improper influence → Respect others.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 100–103. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/4-2-illustration.png',N'Below',2,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Understanding KEJARA',N'**What you will learn**
- Scheduled offence
- Recorded demerits
- Licence consequences

**Understanding KEJARA**
KEJARA is the road-safety demerit system described in the source. Its purpose is to discourage repeated offences, improve discipline and address unsafe drivers. It relates to scheduled offences and the associated enforcement process. Demerit points are not the same as a simple count of every traffic mistake. The consequences depend on the applicable licence and rules. Use current JPJ information for the detailed schedule of offences and points.

**Check your understanding**
Is KEJARA only a way of collecting fines?

**Answer and explanation**
No. It is a demerit system intended to improve behaviour and impose licence-related consequences.

**Key reminder**
Scheduled offence → Recorded demerits → Licence consequences.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 93–94. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/4-3-illustration.png',N'Below',3,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Warnings, Suspension, and Cancellation',N'**What you will learn**
- Identify licence type
- Understand consequences
- Follow reinstatement rules

**Warnings, Suspension, and Cancellation**
The PDF distinguishes probationary licence cancellation from the warning and suspension sequence described for other licence holders. Do not merge these into a single penalty ladder. Suspension or cancellation means the person must not continue driving. The source also discusses rehabilitation and conditional point reductions; these are not automatic rewards or permission to ignore an offence. Confirm the current requirements and personal status with JPJ before relying on a reinstatement or reduction.

**Check your understanding**
Can a suspended driver continue driving while waiting for rehabilitation?

**Answer and explanation**
No. Rehabilitation does not itself override a suspension.

**Key reminder**
Identify licence type → Understand consequences → Follow reinstatement rules.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 14–16,95–96. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/4-4-illustration.png',N'Below',4,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.LearningChapters(Title,Description,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(N'Hazard Awareness and Defensive Driving',N'Identify hazards, predict changes and plan safe responses before a conflict develops.',5,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    SET @ChapterId=CONVERT(int,SCOPE_IDENTITY());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Recognising Different Hazards',N'**What you will learn**
- Road and environment
- People and vehicles
- Your own actions

**Recognising Different Hazards**
A hazard is anything that may create danger. The source groups hazards around road design, other users, surface and weather conditions, vehicle condition and the driver''s own actions. Hazards can be obvious, such as a damaged surface, or hidden, such as a pedestrian behind a parked van. Motorcycle risks include limited visibility, unsafe lane changes and road-surface defects. Look for combinations of hazards rather than only one isolated object.

**Check your understanding**
Can your own distraction be a hazard?

**Answer and explanation**
Yes. Your decisions and attention can create danger even when the road appears clear.

**Key reminder**
Road and environment → People and vehicles → Your own actions.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 79–83. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/5-1-illustration.png',N'Below',1,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'How Hazards Lead to Collisions',N'**What you will learn**
- Contributing conditions
- Trigger and conflict
- Prevent or reduce harm

**How Hazards Lead to Collisions**
A collision can develop through a chain of events. A triggering event creates an immediate conflict, while contributing conditions make the conflict more likely or more severe. For example, changing lanes without checking may trigger a conflict; poor observation or excessive speed may contribute. Early preparation can break the chain before a collision occurs. Protective equipment reduces harm but does not remove the need to prevent the conflict.

**Check your understanding**
What is the difference between a trigger and a contributing factor?

**Answer and explanation**
A trigger directly creates the conflict; a contributing factor increases its likelihood or severity.

**Key reminder**
Contributing conditions → Trigger and conflict → Prevent or reduce harm.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 84–87. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/5-2-illustration.png',N'Below',2,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Observation and Situational Awareness',N'**What you will learn**
- Observe
- Understand
- Anticipate

**Observation and Situational Awareness**
Observation means actively interpreting your surroundings, not merely looking ahead. Use relevant visual and audible information while keeping attention on driving. Ask what you can see, what is hidden and what might happen next. Prioritise hazards by their possible effect and how soon they may affect you. Motorcycles can be difficult to detect, especially when obscured or moving quickly. Recheck the scene as positions change.

**Check your understanding**
A parked van blocks your view of the pavement. What should you anticipate?

**Answer and explanation**
A person may emerge from behind it; create time and space for a safe response.

**Key reminder**
Observe → Understand → Anticipate.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 88–91,107–113. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/5-3-illustration.png',N'Below',3,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Making a Safe Driving Plan',N'**What you will learn**
- Identify and predict
- Decide
- Act and reassess

**Making a Safe Driving Plan**
The IPDA model means Identify, Predict, Decide and Act. Identify a hazard, predict how it may develop, choose a suitable response and carry it out. The driving action plan organises information, position, speed, gear where applicable and acceleration. Information remains important throughout the manoeuvre. A decision that was safe a moment ago may need changing when another road user moves. Do not accelerate past a hazard until the path is safe.

**Check your understanding**
Is a driving plan fixed once you decide?

**Answer and explanation**
No. Continue observing and revise the plan when the situation changes.

**Key reminder**
Identify and predict → Decide → Act and reassess.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 106–114. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/5-4-illustration.png',N'Below',4,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Speed, Stress, and Driver Readiness',N'**What you will learn**
- Check readiness
- Allow response time
- Stop when unfit

**Speed, Stress, and Driver Readiness**
Higher speed reduces the time available to process a hazard and respond. Stress, fatigue, anger and distraction can undermine judgement and observation. Alcohol and drugs can impair driving; some medicines also affect alertness. Plan journeys to avoid rushing and recognise signs that concentration is deteriorating. If you cannot maintain safe attention, stop in a suitable place and address the cause before continuing.

**Check your understanding**
Will opening a window reliably solve dangerous fatigue?

**Answer and explanation**
No. Treat fatigue as a readiness problem and stop safely rather than relying on a temporary sensation of alertness.

**Key reminder**
Check readiness → Allow response time → Stop when unfit.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 89–93,108,138. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/5-5-illustration.png',N'Below',5,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.LearningChapters(Title,Description,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(N'Safe Road Positioning and Manoeuvres',N'Apply observation routines, safe spacing and deliberate manoeuvres in everyday traffic.',6,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    SET @ChapterId=CONVERT(int,SCOPE_IDENTITY());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Mirrors, Signals, and Blind Spots',N'**What you will learn**
- Mirrors
- Signal and blind spot
- Manoeuvre when safe

**Mirrors, Signals, and Blind Spots**
CITO is the source''s routine for checking mirrors, signalling, checking the blind spot and manoeuvring. Mirrors do not show every area beside or behind a vehicle. Before moving, gather information from the mirrors and a suitable shoulder check without losing control or attention ahead. Signal your intention and move only when the available space is safe. Other vehicles, particularly large vehicles, also have blind spots; avoid assuming that their drivers have seen you.

**Check your understanding**
Does signalling give you priority to change lanes?

**Answer and explanation**
No. It communicates intention; you must still check and wait for a safe opportunity.

**Key reminder**
Mirrors → Signal and blind spot → Manoeuvre when safe.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 115–116. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/6-1-illustration.png',N'Below',1,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Following Distance and Looking Ahead',N'**What you will learn**
- Choose a fixed marker
- Count the time gap
- Increase space as needed

**Following Distance and Looking Ahead**
The source teaches a two-second following method and a four-second method for conditions needing extra space. Select a fixed roadside object and count from when the vehicle ahead passes it until you reach it. Reaching it too soon means you need more space. Wet or slippery roads, towing and traffic behaviour can require a larger margin. These methods are practical guides, not guarantees that every stopping distance is adequate. Look well beyond the vehicle immediately ahead.

**Check your understanding**
You reach the marker before finishing the count. What should you do?

**Answer and explanation**
Ease back safely to increase the gap and check again using another fixed marker.

**Key reminder**
Choose a fixed marker → Count the time gap → Increase space as needed.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 117–122. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/6-2-illustration.png',N'Below',2,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Overtaking and Being Overtaken',N'**What you will learn**
- Assess before passing
- Pass with space
- Return safely

**Overtaking and Being Overtaken**
Before overtaking, check the road markings, visibility, traffic ahead and behind, and whether enough space exists for the whole manoeuvre. Use the safety routine before moving out and again before returning. Avoid overtaking near hazards such as restricted bends, crossings and junctions. Do not rush back in front of the passed vehicle. When someone overtakes you, maintain predictable control and avoid obstructing their safe return.

**Check your understanding**
Is it enough to see clear space beside the vehicle you want to pass?

**Answer and explanation**
No. You need visibility and sufficient space for the entire manoeuvre, including a safe return.

**Key reminder**
Assess before passing → Pass with space → Return safely.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 122–124. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/6-3-illustration.png',N'Below',3,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Junctions and Turning',N'**What you will learn**
- Plan and position
- Observe and slow
- Turn when clear

**Junctions and Turning**
Know your intended direction before reaching a junction. Choose the appropriate lane and position, signal early enough to communicate and adjust speed to the view and controls. Observe pedestrians, riders and conflicting traffic. Having priority does not remove the need to anticipate another user''s mistake. At a restricted view, be prepared to wait until you can judge the situation safely. Do not change direction suddenly at the last moment.

**Check your understanding**
What should you do if another vehicle appears likely to ignore your priority?

**Answer and explanation**
Reduce the risk and be prepared to stop; priority is not protection from a collision.

**Key reminder**
Plan and position → Observe and slow → Turn when clear.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 126–132. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/6-4-illustration.png',N'Below',4,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Roundabouts and Traffic Flow',N'**What you will learn**
- Choose the lane
- Observe circulating traffic
- Exit predictably

**Roundabouts and Traffic Flow**
Plan the intended exit and follow the roundabout''s signs and markings. Approach at a speed that allows observation and a safe decision. Check circulating traffic and maintain a suitable position. Use the safety routine when changing position or leaving, watching especially for motorcycles and vehicles beside you. Roundabout layouts differ, so a memorised diagram must not override the actual lane controls.

**Check your understanding**
Should one memorised lane rule override a marked lane at a roundabout?

**Answer and explanation**
No. Read the local signs and markings and plan your route accordingly.

**Key reminder**
Choose the lane → Observe circulating traffic → Exit predictably.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 133–135. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/6-5-illustration.png',N'Below',5,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Manoeuvres and Collision Prevention',N'**What you will learn**
- Enter or cross traffic
- Move and change direction
- Leave traffic safely

**Manoeuvres and Collision Prevention**
The PDF groups manoeuvres around entering, travelling with, crossing and leaving traffic, overtaking, and changing direction. Each requires observation, adequate space and controlled speed. Reversing and turning around also expose areas outside the driver''s direct view. Recognise front, rear, side and overtaking conflicts and avoid creating one through a sudden change. A safe position should give you both a useful view and room to respond.

**Check your understanding**
Why should you check behind even when mainly concerned about traffic ahead?

**Answer and explanation**
A sudden stop or movement can create a rear or side conflict; safe decisions account for the surrounding traffic.

**Key reminder**
Enter or cross traffic → Move and change direction → Leave traffic safely.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 125,136–147. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/6-6-illustration.png',N'Below',6,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.LearningChapters(Title,Description,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(N'Driving in Different Environments',N'Adapt observation and control to different roads, visibility and weather.',7,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    SET @ChapterId=CONVERT(int,SCOPE_IDENTITY());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Urban Roads and Vulnerable Road Users',N'**What you will learn**
- Scan the roadside
- Anticipate emergence
- Share space

**Urban Roads and Vulnerable Road Users**
Urban roads combine junctions, parked vehicles, pedestrians, cyclists, motorcycles and frequent stops. People may cross near shops, schools or markets, sometimes from behind an obstruction. Allow space and avoid abrupt lane changes. Observe motorcycle movements beside the vehicle and check carefully before turning. Patience matters in congestion: a small time saving is not worth creating a conflict.

**Check your understanding**
What extra risk does a line of parked cars create?

**Answer and explanation**
It can hide people and vehicles emerging into your path and may include opening doors.

**Key reminder**
Scan the roadside → Anticipate emergence → Share space.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 150–154. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/7-1-illustration.png',N'Below',1,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Rural Roads and Tunnels',N'**What you will learn**
- Expect surface changes
- Manage limited views
- Adapt to lighting

**Rural Roads and Tunnels**
Rural roads may have narrow sections, uneven surfaces, animals and sharp bends with limited sight distance. Choose speed according to what you can see and the road condition. Tunnel entrances and exits change lighting and visibility. Use appropriate lights, maintain spacing and avoid sudden movements while your view adapts. A quiet road is not necessarily a low-risk road.

**Check your understanding**
Does low traffic on a rural road mean high speed is safe?

**Answer and explanation**
No. Surface condition, bends, animals and limited visibility still determine risk.

**Key reminder**
Expect surface changes → Manage limited views → Adapt to lighting.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 151–153. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/7-2-illustration.png',N'Below',2,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Expressways',N'**What you will learn**
- Plan entry and exit
- Maintain lane discipline
- Use appropriate stopping places

**Expressways**
Expressways involve higher speeds and different entry and exit layouts. Read guidance early and select lanes in time. Maintain suitable spacing and avoid last-minute cuts across lanes. Use designated rest and stopping facilities; the emergency lane is not a normal driving lane. Where dedicated motorcycle facilities exist, understand their use. Prepare for an exit while still maintaining awareness of surrounding traffic.

**Check your understanding**
Can the emergency lane be used to bypass a queue?

**Answer and explanation**
No. It is not an additional traffic lane for ordinary travel.

**Key reminder**
Plan entry and exit → Maintain lane discipline → Use appropriate stopping places.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 154–155. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/7-3-illustration.png',N'Below',3,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Night Driving',N'**What you will learn**
- Check lights
- Manage glare
- Protect visibility

**Night Driving**
At night, hazards can become visible later than in daylight. Ensure the lights and windscreen are in suitable condition and use lighting appropriately for surrounding traffic. Avoid dazzling others and adjust speed to the distance you can see clearly. Look for pedestrians, animals and unlit obstacles. Fatigue may compound poor visibility, so plan rest and recognise loss of concentration.

**Check your understanding**
Why should speed be reconsidered at night even on a familiar road?

**Answer and explanation**
The visible distance and time available to identify unexpected hazards may be reduced.

**Key reminder**
Check lights → Manage glare → Protect visibility.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 156–157. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/7-4-illustration.png',N'Below',4,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Rain, Flooding, Fog, and Crosswinds',N'**What you will learn**
- Reduce demands on grip
- Increase observation and space
- Choose a safe alternative

**Rain, Flooding, Fog, and Crosswinds**
Rain and standing water reduce grip and may conceal surface defects. Aquaplaning occurs when water separates the tyres from effective road contact. Use smooth control and allow more space. Fog reduces visibility; crosswinds can affect stability, especially near exposed areas or large vehicles. Do not assume flooded water is shallow or the road beneath it is intact. If conditions prevent safe progress, choose an alternative route or stop somewhere suitable.

**Check your understanding**
Should you follow another vehicle into floodwater simply because it went first?

**Answer and explanation**
No. Its passage does not establish a safe depth, current or road condition for your vehicle.

**Key reminder**
Reduce demands on grip → Increase observation and space → Choose a safe alternative.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 158–159. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/7-5-illustration.png',N'Below',5,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.LearningChapters(Title,Description,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(N'Breakdowns and Road Emergencies',N'Recognise emergencies, reach safety and understand the priorities after a breakdown or collision.',8,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    SET @ChapterId=CONVERT(int,SCOPE_IDENTITY());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Recognising Vehicle Emergencies',N'**What you will learn**
- Recognise the fault
- Maintain controlled movement
- Seek a safe stop

**Recognising Vehicle Emergencies**
The source discusses brake problems, skidding, a stuck accelerator, tyre failure, failed lights, an open bonnet, damaged glass and fire. Different failures require different responses; there is no single emergency manoeuvre suitable for all vehicles. Prioritise control, awareness of surrounding traffic and a safe stopping opportunity. Learn vehicle-specific procedures through instruction and the vehicle manual before an emergency occurs. Once stopped, seek appropriate assistance rather than continuing with a serious defect.

**Check your understanding**
Should every vehicle fault be handled with the same abrupt braking action?

**Answer and explanation**
No. The response depends on the fault and vehicle; maintaining control and reaching safety are the priorities.

**Key reminder**
Recognise the fault → Maintain controlled movement → Seek a safe stop.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 159–162. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/8-1-illustration.png',N'Below',1,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Stopping Safely After a Breakdown',N'**What you will learn**
- Find a safe position
- Warn and protect
- Call for assistance

**Stopping Safely After a Breakdown**
A breakdown can create a secondary collision risk. Where possible, move clear of live traffic and warn other road users using suitable warning equipment. Consider visibility, bends and approaching traffic before placing equipment or leaving the vehicle. Do not expose yourself to moving traffic to recover an object or attempt repairs. Contact the appropriate assistance provider and describe your location clearly. Highway breakdowns require particular attention to the risk from fast-moving traffic.

**Check your understanding**
What is the main danger after the vehicle has stopped?

**Answer and explanation**
Other traffic may strike the stopped vehicle or people nearby; protect people from that secondary risk.

**Key reminder**
Find a safe position → Warn and protect → Call for assistance.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 159,161–162. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/8-2-illustration.png',N'Below',2,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Emergency Equipment',N'**What you will learn**
- Know the equipment
- Check its condition
- Use it appropriately

**Emergency Equipment**
The source illustrates warning equipment, a torch, first-aid supplies and basic tools, with different kits for cars and motorcycles. Know where your equipment is stored and check it periodically. Carrying a tool does not mean every roadside repair is safe. A jack needs correct positioning and suitable ground; unfamiliar electrical or mechanical work should be left to competent assistance. Equipment supports a safe plan rather than replacing one.

**Check your understanding**
Does carrying a toolkit make a repair beside fast traffic safe?

**Answer and explanation**
No. Location, competence and the nature of the fault determine whether a repair is appropriate.

**Key reminder**
Know the equipment → Check its condition → Use it appropriately.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 163–164,193. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/8-3-illustration.png',N'Below',3,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'What to Do After a Collision',N'**What you will learn**
- Protect the scene
- Request appropriate help
- Record and report

**What to Do After a Collision**
After a collision, first consider immediate danger to yourself and others. Request emergency assistance when required and follow the responder''s instructions. Help within your training and avoid actions that could worsen an injury. Once it is safe, exchange and record relevant vehicle, driver, location and incident details. Follow the applicable police and insurance reporting processes and retain the records. This lesson introduces priorities; it does not teach medical treatment or replace first-aid training.

**Check your understanding**
Should collecting photographs take priority over an immediate danger to people?

**Answer and explanation**
No. Scene safety and appropriate emergency help come first.

**Key reminder**
Protect the scene → Request appropriate help → Record and report.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 199–200. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/8-4-illustration.png',N'Below',4,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.LearningChapters(Title,Description,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(N'Vehicle Checks and Maintenance',N'Know the main components and build a practical inspection and maintenance routine.',9,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    SET @ChapterId=CONVERT(int,SCOPE_IDENTITY());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Getting to Know Your Vehicle',N'**What you will learn**
- Identify components
- Understand their purpose
- Know your vehicle type

**Getting to Know Your Vehicle**
Locate the main controls, lights, mirrors, tyres and braking components before using an unfamiliar vehicle. The source shows different motorcycle layouts and distinguishes fuel, fluids, electrical parts and rubber components. Petrol, diesel and electric vehicles have different systems; an electric vehicle is not maintained exactly like an engine-powered one. Use the manufacturer''s guidance to identify which checks apply. Familiarity helps you recognise a change or defect before it becomes a road hazard.

**Check your understanding**
Should every vehicle receive an engine-oil check?

**Answer and explanation**
No. Checks depend on the powertrain and equipment; use the guidance for that vehicle.

**Key reminder**
Identify components → Understand their purpose → Know your vehicle type.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 168–171,180. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/9-1-illustration.png',N'Below',1,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Motorcycle Inspection and Maintenance',N'**What you will learn**
- Check tyres and brakes
- Inspect drive and power
- Check lights and mirrors

**Motorcycle Inspection and Maintenance**
Inspect fuel or charge, applicable oil levels, tyres, brakes, drive system, lights, horn and mirrors. The PDF illustrates chain, belt and shaft drive, which do not share every maintenance step. Tyre damage, incorrect pressure and worn components affect control. Check that controls operate correctly and mirrors provide a useful view. Maintenance limits and procedures depend on the model. Arrange competent attention to defects instead of assuming a short journey will be safe.

**Check your understanding**
Should a shaft drive be adjusted using a chain-tension procedure?

**Answer and explanation**
No. Different drive systems have different inspection and maintenance requirements.

**Key reminder**
Check tyres and brakes → Inspect drive and power → Check lights and mirrors.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 168–180. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/9-2-illustration.png',N'Below',2,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Car Inspection and Maintenance',N'**What you will learn**
- Fluids and cooling
- Electrical and visibility
- Tyres and rubber parts

**Car Inspection and Maintenance**
Check the items applicable to the car: fuel or charge, engine and other fluids, cooling system, washer supply, lights, tyres, wipers, hoses and belts. Look for leaks, damage, low levels or unusual behaviour. Follow the vehicle manual for safe inspection conditions and the correct fluid specification. Do not apply a generic service interval to every model. Electric and hybrid vehicles have specific systems that require suitable knowledge and equipment.

**Check your understanding**
Is any available fluid suitable for topping up a reservoir?

**Answer and explanation**
No. Identify the reservoir and use the specification required by the manufacturer.

**Key reminder**
Fluids and cooling → Electrical and visibility → Tyres and rubber parts.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 180–186. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/9-3-illustration.png',N'Below',3,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Building a Maintenance Routine',N'**What you will learn**
- Inspect before travel
- Record changes
- Service on schedule

**Building a Maintenance Routine**
Pre-journey checks help identify immediate problems; planned servicing addresses wear over time. Keep records of inspections, servicing and reported faults. Follow the manufacturer''s time and distance guidance and consider the vehicle''s use. Defects in tyres, brakes, lights or visibility deserve prompt attention. Maintenance supports safety, reliability and vehicle life, and reduces the chance that a defect will become an emergency on the road.

**Check your understanding**
Does a recent service remove the need for pre-journey observation?

**Answer and explanation**
No. Damage or faults can develop between service visits.

**Key reminder**
Inspect before travel → Record changes → Service on schedule.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 185–186. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/9-4-illustration.png',N'Below',4,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.LearningChapters(Title,Description,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(N'Rider and Passenger Protection',N'Use protective equipment and understand the purpose and limits of vehicle safety systems.',10,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    SET @ChapterId=CONVERT(int,SCOPE_IDENTITY());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Motorcycle Protective Equipment',N'**What you will learn**
- Fit and fasten
- Protect hands and feet
- Improve visibility

**Motorcycle Protective Equipment**
The source describes helmets, footwear, gloves, jackets, reflective features and rainwear. A helmet must fit properly and be securely fastened; simply wearing it loosely does not provide the intended protection. Clothing should support protection and visibility without interfering with control. Gloves and suitable footwear help protect vulnerable areas. Choose equipment appropriate to riding and check its condition. Protective equipment reduces injury risk but cannot make an unsafe manoeuvre safe.

**Check your understanding**
Is a loosely fastened helmet equivalent to a properly secured one?

**Answer and explanation**
No. Correct fit and fastening are essential to its intended protection.

**Key reminder**
Fit and fasten → Protect hands and feet → Improve visibility.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 190–193. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/10-1-illustration.png',N'Below',1,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Seat Belts and Head Restraints',N'**What you will learn**
- Wear the belt correctly
- Adjust the restraint
- Protect every occupant

**Seat Belts and Head Restraints**
Seat belts help restrain occupants during sudden movement or a collision. Use the belt correctly rather than placing it in a more convenient but ineffective position. Head restraints reduce the risk of certain neck injuries when properly positioned. Adjust the seat and restraint to the occupant and follow the vehicle guidance. Passenger protection matters in every seating position, not only for the driver.

**Check your understanding**
Does a head restraint replace the seat belt?

**Answer and explanation**
No. They serve different protective functions and should be used together correctly.

**Key reminder**
Wear the belt correctly → Adjust the restraint → Protect every occupant.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 193–194. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/10-2-illustration.png',N'Below',2,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'Child Passenger Safety',N'**What you will learn**
- Choose a suitable restraint
- Install correctly
- Understand child locks

**Child Passenger Safety**
A child restraint must be appropriate for the child and installed according to its instructions and the vehicle guidance. Adult belts alone may not fit a small child correctly. The source distinguishes child seats and booster arrangements and illustrates child locks on rear doors. A child lock prevents a door being opened from inside; it does not restrain the child in a collision. Follow seat-specific and vehicle-specific instructions, including airbag-related restrictions.

**Check your understanding**
Does switching on the child lock make a child seat unnecessary?

**Answer and explanation**
No. The lock controls the door; the restraint protects the child during vehicle movement and a collision.

**Key reminder**
Choose a suitable restraint → Install correctly → Understand child locks.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 194–196. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/10-3-illustration.png',N'Below',3,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.Lessons(ChapterId,Title,Content,ImagePath,ImagePosition,DisplayOrder,IsActive,CreatedAt,UpdatedAt) VALUES(@ChapterId,N'ABS and Airbags',N'**What you will learn**
- Understand the system
- Use restraints
- Keep safe margins

**ABS and Airbags**
ABS helps manage wheel locking during braking, but it does not create unlimited grip or remove the need for space. Airbags supplement the vehicle''s restraint system and are not substitutes for seat belts. System behaviour and coverage differ between vehicles. Read the vehicle guidance, use correct seating and restraints, and keep the same habits of observation and controlled speed. Safety technology supports responsible driving rather than compensating for every unsafe choice.

**Check your understanding**
Can airbags replace wearing a seat belt?

**Answer and explanation**
No. Airbags are supplementary protection and should work with correctly used restraints.

**Key reminder**
Understand the system → Use restraints → Keep safe margins.

**Source**
Adapted in English from JPJ KPP Motokar dan Motosikal, fourth edition (2025), PDF pages 196–198. This lesson supports theory learning alongside practical instruction.',N'Content/images/learning/kpp-2025/10-4-illustration.png',N'Below',4,1,SYSUTCDATETIME(),SYSUTCDATETIME());
    INSERT dbo.CurriculumImports(Version,ImportedAt) VALUES(N'kpp-2025-english-v1',SYSUTCDATETIME());
END;
COMMIT TRANSACTION;

