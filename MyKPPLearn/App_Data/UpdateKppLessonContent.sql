-- Rewrite the KPP 2025 lessons once: drop the question, answer, reminder and source sections and improve the text.
-- Only lessons still holding the original imported text are replaced; lessons an admin has edited are left alone.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @LockResult int;
EXEC @LockResult=sys.sp_getapplock @Resource=N'KppEnglishCurriculum2025', @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=30000;
IF @LockResult < 0 THROW 51000, 'Could not acquire curriculum migration lock.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.CurriculumImports WHERE Version=N'kpp-2025-content-v2')
BEGIN
    DECLARE @Content TABLE (Title nvarchar(200) PRIMARY KEY, Content nvarchar(max) NOT NULL);
    INSERT @Content (Title,Content) VALUES
(N'Who Can Apply for a Driving Licence?', N'**What you will learn**
- The minimum age for each vehicle type
- The health and vision requirements
- The documents needed to apply for a learner licence

**Minimum age**
- Motorcycle: 16 years old
- Car: 17 years old
- Other vehicles: 21 years old

**Health and vision**
Being old enough is not enough on its own. You must be physically fit, able to see clearly ahead to a distance of 23 metres and not colour blind. These requirements exist because driving depends on reading signs, signals and hazards quickly and correctly.

**Applicants with disabilities (OKU)**
OKU applicants submit the OKU Driving Licence Medical Examination Form, confirmed by a medical officer at a government health clinic.

**Foreign applicants**
Foreign citizens may apply if they hold a valid passport and will stay in Malaysia for at least 3 months.

**Applying for a learner licence (LDL)**
Apply at any JPJ counter or online with:
- The driving licence application form (JPJL1)
- Your original identity card or passport
- One colour photograph, 25 mm x 32 mm, with a white background'),
(N'Understanding Licence Types and Classes', N'**What you will learn**
- The licence types issued by JPJ
- The difference between a licence type and a licence class
- The car and motorcycle classes

**Licence types**
JPJ issues licences in physical and digital form (through the MyJPJ app). The three you will meet on your way to driving are:
- LDL (Learner Driving Licence): for learning to drive
- PDL (Probationary Driving Licence): issued after you pass the practical test
- CDL (Competent Driving Licence): issued after the probation period

Other licences include PSV (public service vehicles), conductor, GDL (goods vehicles), armed forces licences and the International Driving Permit (IDP).

**Licence type versus licence class**
The type shows your stage or purpose. The class shows which vehicles you may drive. Always check the class printed on your licence.

**Common classes**
- B2: motorcycle up to 250 cc
- B1: motorcycle up to 500 cc
- B: motorcycle above 500 cc
- D: car with an unladen weight up to 3,500 kg
- DA: automatic car (no clutch pedal) up to 3,500 kg
- A and A1: vehicles for disabled drivers (motorcycle and car)

A class D or DA car licence does not let you ride a motorcycle. Each class needs its own training and test.'),
(N'Your Journey from Learner to Competent Driver', N'**What you will learn**
- Each stage of getting a car or motorcycle licence
- The training hours for cars and motorcycles
- What happens after the practical test

**Step 1: Theory course and Part I test**
Attend the 6-hour Safe Driving Theory Course (KPP 01) and get your attendance certificate (JPJL2A). The computer-based Part I test has 70 questions in 1 hour 15 minutes, and you need at least 56 to pass. You must pass within 1 year of the certificate date, or take the course again.

**Step 2: Learner licence**
After passing Part I, you are issued your learner licence (LDL).

**Step 3: Circuit training (KPP 02)**
- Car: 5.5 hours, including vehicle checks, S and Z bends, hill start, side parking and three-point turn
- Motorcycle: 9.5 hours, including the plank ride, cone slalom, emergency braking, uneven road and figure 8

**Step 4: Road training (KPP 03)**
- Car: 10 hours
- Motorcycle: 6 hours

Road training teaches you to spot and handle hazards, manage speed, overtake and change lanes safely.

**Step 5: Pre-test assessment and practical test**
After a 30-minute pre-test assessment, you take the JPJ Part II and III practical tests. You get your result straight away. If you fail, you may retest after booking again.

**Step 6: Probationary licence**
Successful candidates apply for a probationary licence (PDL), which later leads to a competent licence (CDL).'),
(N'Licence Holder Responsibilities', N'**What you will learn**
- The rules for learner licence holders
- The rules for probationary licence holders
- What happens if a probationary driver breaks the rules

**Learner licence (LDL) holders must**
- Display an "L" plate at the front and back of the vehicle
- Drive only under the supervision of a driving institute instructor (this does not apply to motorcycles outside institute lessons)
- Not carry passengers or loads

**Probationary licence (PDL) holders must**
- Display a "P" plate at the front and back. On a car, place it at the top left corner of the front and rear windscreens. On a motorcycle, place it above or below the headlamp and above or below the rear number plate.
- Always carry the probationary licence
- Keep their breath, blood and urine alcohol level at 0.00
- Apply for the probationary licence within 1 year of passing the practical test

**Penalties**
Breaking a probationary condition can lead to a fine of up to RM2,000 or 6 months in jail for a first offence, and up to RM4,000 or 12 months in jail, or both, for later offences.

A probationary driver who collects 20 or more demerit points has the licence cancelled. They may not drive, hold or apply for any licence, including a learner licence, for 12 months.'),
(N'Foreign Licences and Accessible Driving', N'**What you will learn**
- When a foreign licence can be used in Malaysia
- The rules for Singapore licences
- How people with disabilities can get a licence

**Foreign licences**
- Visitors staying less than 12 months may drive with a valid International Driving Permit (IDP) from a country that signed the 1949 Geneva or 1968 Vienna convention.
- Visitors from ASEAN countries may use their home licence if it is in English.
- Maldives licences can be used without an IDP.

From 19 May 2025, foreign licences can no longer be converted to a Malaysian licence. Foreigners who need one must go through the normal process. Diplomats, MM2H participants and Malaysians who got a foreign licence before holding a Malaysian one are not affected.

**Singapore licences**
A valid Singapore licence can be used in Malaysia, in vehicles registered in either country. A Malaysian or Singapore citizen may hold only one of the two licences at a time.

**Getting an IDP to drive abroad**
Holders of a Malaysian competent licence (CDL) valid for at least 1 more year can apply at a JPJ office. The fee is RM150 for 1 year.

**Applicants with disabilities (OKU)**
OKU applicants need:
- An OKU card from the Social Welfare Department
- Confirmation from a registered medical practitioner on the OKU medical form
- Their own vehicle, modified for their needs and approved by the JPJ Automotive Engineering Division

They may be licensed for class A or A1 only.'),
(N'How to Read the Road', N'**What you will learn**
- What traffic signs are for
- How shape and colour tell you the type of sign
- How to respond to a sign

**Why signs matter**
Traffic signs control the flow of traffic, warn you of danger and tell you what to do. Obey them even when you cannot see any danger. If other road users ignore a sign, that is never a reason for you to do the same.

**Sign types by shape and colour**
- Warning signs: yellow diamond with black symbols
- Prohibitory signs: mostly red circles; some are hexagons (such as STOP), triangles or squares
- Mandatory signs: blue circles or blue rectangles
- Information signs: blue or green rectangles
- Temporary signs: orange diamonds, used at road works

**How to respond: the driving action plan**
- Information: look out for signs and understand what they mean
- Position: get into the correct lane in good time
- Speed: choose a safe speed and following distance
- Gear: select a gear that suits your speed and the traffic
- Acceleration: accelerate smoothly and with control

Before any manoeuvre, check your mirrors, signal, check your blind spot, then move.'),
(N'Traffic Lights', N'**What you will learn**
- What each light colour means
- Why green does not always mean go
- What to do when the lights are not working

**Where traffic lights are used**
Traffic lights control traffic from several directions, including pedestrians. You will find them at busy junctions, pedestrian crossings and railway crossings.

**What the colours mean**
- Red: stop behind the stop line and wait.
- Amber: stop, unless you have already crossed the stop line. Do not speed up to beat the light.
- Green: you may go, but only if the junction is clear.
- Green arrow: you may go in the direction of the arrow.

**Do not block the junction**
If your exit is blocked by queuing traffic, wait behind the line even when the light is green.

**When the lights fail**
- At a crossroads, no one has priority. Be patient and considerate, and go only when it is safe.
- At a T-junction, give way to traffic on the main road.

Never ignore a traffic light. It puts you, your passengers and other road users in danger.'),
(N'Police and Road User Hand Signals', N'**What you will learn**
- How traffic police direct traffic with hand signals
- The hand signals drivers and riders use
- Why a signal is not a guarantee

**Traffic police hand signals**
Traffic police directions override traffic lights. An officer can:
- Stop traffic coming from behind them
- Stop traffic coming from in front of them
- Wave traffic on from the left or the right
- Let traffic ahead turn left or right

Read the signal in relation to where you are coming from, then follow it carefully.

**Driver and rider hand signals**
If indicators fail, or to make your intention clearer, you can use hand signals:
- Turning right: arm stretched straight out to the right
- Turning left: arm moved in a circle to show a left turn
- Slowing down: arm stretched out and moved slowly up and down

**A signal shows intention, not safety**
A signal tells you what someone plans to do. It does not mean they will do it, or that the move is safe. Keep watching and wait until the path is clear.'),
(N'Lines, Crossings, and Lane Markings', N'**What you will learn**
- What broken and continuous lines mean
- What yellow boxes and hatched areas are for
- How markings work with signs and lane changes

**Centre and lane lines**
- Broken line: you may cross it when it is safe, for example to overtake.
- Continuous line: do not cross it or overtake.
- Double line, one broken and one continuous: follow the line nearest to you.
- Edge line: marks the edge of the road. Do not drive on the shoulder.

**Areas you must keep clear**
- Yellow box: do not enter unless your exit is clear. Never stop inside it.
- Hatched or chevron areas: separate traffic. They are not extra lanes for queuing or overtaking.
- Bus lanes and other reserved lanes: only for the vehicles shown.

**Pedestrian crossings**
Slow down near a crossing and stop for people who are crossing or waiting to cross. Do not overtake near a crossing.

**Changing lanes**
Follow the arrows on the road and choose your lane early. Before you change lanes, check your mirrors, signal and check your blind spot.'),
(N'Warning Signs: Road Layout and Surface', N'**What you will learn**
- The signs that warn of junctions and bends
- The signs that warn of road surface hazards
- How to prepare before the hazard

**Junction signs**
Signs can warn of a crossroads, a side road on the left or right, a T-junction or a staggered junction. When you see one:
- Slow down
- Signal early if you plan to turn
- Give way to traffic on the main road and watch for vehicles coming out

**Bend signs**
These warn of a single bend, a double bend or a series of bends. Bends limit your view and can cause you to lose control.
- Slow down and use a suitable gear before the bend, not in it
- Do not overtake
- Look as far ahead as you can

**Road layout signs**
- Narrow road ahead or narrow bridge: slow down and give way to oncoming traffic if needed
- Two-way traffic ahead: keep left and watch for oncoming vehicles
- Steep hill: use a lower gear going up and to control your speed going down

**Road surface signs**
- Slippery road: slow down and avoid harsh braking, accelerating or steering
- Uneven road or loose chippings: reduce speed and keep a safe distance
- Soft shoulder: keep off the edge of the road'),
(N'Warning Signs: People, Animals, and Crossings', N'**What you will learn**
- Signs for schools, pedestrians and disabled road users
- Signs for animals
- How to approach a railway crossing

**People ahead**
Signs warn of schools, playgrounds, pedestrian crossings and disabled people crossing. When you see one:
- Slow down and be ready to stop
- Look beyond the sign for people near the road
- Expect children to run out suddenly, including from behind parked vehicles

**Animals ahead**
Cattle, deer or other animals may be on or crossing the road. Slow down and do not sound your horn at them, because it may frighten them into your path.

**Railway crossings**
Signs warn of a level crossing with or without gates.
- Slow down as you approach
- Stop when the lights flash, the alarm sounds or the barriers come down
- Do not follow another vehicle onto the crossing. Make sure there is space to clear the tracks before you go.

**Why it matters**
A warning sign marks a place where someone may step into your path. Reducing your speed early gives you time to react.'),
(N'Prohibitory Signs and Restrictions', N'**What you will learn**
- What prohibitory signs look like
- The common prohibitions
- How to read speed, height, width and weight limits

**What they look like**
Most prohibitory signs are red circles. A red diagonal line means the action shown is not allowed. The STOP sign, which reads BERHENTI, is a red octagon.

**Common prohibitions**
- BERHENTI (STOP): stop completely at the line, then go only when it is safe
- No entry: you must not enter the road from this direction
- No overtaking
- No left turn, no right turn or no U-turn
- No parking: you may stop briefly to pick up or drop off passengers
- No stopping: you must not stop at all

**Limits**
- Speed limit: the maximum speed allowed. Drive slower when conditions require it.
- Height limit: arrows point up and down. Vehicles taller than the figure must not pass.
- Width limit: arrows point sideways. Vehicles wider than the figure must not pass.
- Weight limit: vehicles heavier than the figure must not pass.

Read the number and the unit carefully, and check for a plate below the sign. It may limit the rule to certain times or vehicles.'),
(N'Mandatory Signs', N'**What you will learn**
- What mandatory signs look like
- The common mandatory signs
- What to do if you are in the wrong lane

**What they look like**
Mandatory signs are usually blue circles with white symbols. They tell you what you must do.

**Common mandatory signs**
- Turn left or turn right: you must turn in that direction
- Go straight only, or go straight or left: only the directions shown are allowed
- Keep left or keep right: pass on the side the arrow points to
- Roundabout: give way and drive around in a clockwise direction
- Motorcycle lane or bus lane: only for the vehicles shown

**Choose your lane early**
Match the sign with the arrows on the road and move into the correct lane well before the junction.

**In the wrong lane?**
Do not swerve across other lanes at the last moment. Follow the direction of your lane, then find a safe way back to your route.'),
(N'Information and Direction Signs', N'**What you will learn**
- What information signs look like
- How to use direction signs to plan your lane
- What to do if you miss an exit

**What they look like**
Information signs are blue or green rectangles. Blue signs are usually used on ordinary roads, and green signs on expressways.

**Direction and destination signs**
These show the towns, roads and exits ahead. Read them early so you have time to move into the correct lane safely.

**Service and facility signs**
Symbols show parking, hospitals, petrol stations, electric vehicle charging, rest areas and other facilities. A "dead end" sign tells you the road has no way through.

**If you miss an exit**
Carry on safely to the next exit or junction and find another route. Never reverse, stop or cut across lanes to reach an exit you have missed.'),
(N'Temporary Signs and Roadworks', N'**What you will learn**
- What temporary signs look like
- The hazards in a work zone
- How to pass through road works safely

**What they look like**
Temporary signs are orange diamonds with black symbols. They warn of road works, lane closures, diversions and changed traffic flow.

**Work zone hazards**
- Workers and flagmen on or near the road
- Machinery and lorries entering or leaving the site
- Loose stones, uneven surfaces and sudden lane changes

**Passing through safely**
- Slow down early and obey any lower speed limit
- Follow temporary signs, cones and directions from flagmen
- Move into the open lane in good time and merge in turn
- Keep a safe distance from the vehicle in front
- Stay alert until you have completely passed the work area

Do not assume a familiar road is the same as usual. The layout may have changed.'),
(N'Driver Ethics and Personal Responsibility', N'**What you will learn**
- What makes a responsible driver
- How to handle other drivers'' mistakes
- When you should not drive

**A responsible driver**
Safe driving needs knowledge, skill and the right attitude. A responsible driver:
- Is patient and disciplined, even in slow traffic
- Is considerate and gives space to pedestrians, cyclists and motorcyclists
- Obeys the rules whether or not police are watching

**When others make mistakes**
If another driver cuts in or drives badly, stay calm. Do not chase, block or punish them. Drop back, restore a safe gap and let it go.

**Knowing your limits**
Do not drive if you are tired, unwell, upset, or have taken alcohol, drugs or medicine that makes you drowsy. Choosing not to drive is part of being responsible.'),
(N'Integrity During Driver Training', N'**What you will learn**
- The rules for honest training and testing
- What counts as improper influence
- How to behave during training

**Be honest**
- Attend all classes and training sessions yourself
- Never falsify attendance or ask someone to attend or sit a test for you

**Reject improper influence**
Never offer or accept bribes, gifts or promises of a guaranteed pass. Report anyone who offers to help you pass outside the normal process.

**Behave well**
- Follow the institute''s attendance and dress rules
- Treat instructors, examiners and other candidates with respect
- Do not harass or disturb others

**Why it matters**
The aim of training is to make you a competent driver. A licence obtained dishonestly does not make you safe on the road.'),
(N'Understanding KEJARA', N'**What you will learn**
- What KEJARA is
- Why it exists
- How demerit points are recorded

**What is KEJARA?**
KEJARA is Malaysia''s demerit point system for road offences. It operates under Sections 35 and 35A of the Road Transport Act 1987 and the Road Transport (Demerit Points) Rules 2017, amended in 2021.

**Its purpose**
- To discipline and educate road users and stop repeat offences
- To take action against drivers who break traffic laws
- To remove problem and high-risk drivers from the road
- To make drivers more disciplined, responsible and considerate

**How it works**
When you receive a summons for a scheduled offence, it is recorded on your KEJARA record and demerit points are added. Serious offences carry more points.

KEJARA is not just about fines. Collecting too many points can lead to a warning, suspension or cancellation of your licence.'),
(N'Warnings, Suspension, and Cancellation', N'**What you will learn**
- The penalties for collecting 20 demerit points
- The different rules for probationary drivers
- How points can be reduced

**Probationary licence (PDL) holders**
A probationary driver who collects 20 or more demerit points has the licence cancelled. They may not drive, hold or apply for any licence for 12 months.

**Other licence holders**
- 20 or more points, first time: a warning
- Second time: licence suspended for 6 to 8 months
- Third time: licence suspended for 8 to 10 months
- Fourth time: licence suspended for 10 to 12 months

A licence that has been suspended 3 times within 5 years will be cancelled.

**During a suspension**
You must not drive. The Director General of JPJ may order you to attend a rehabilitation course.

**Reducing points**
Your points may be cut by up to 50% if you:
- Commit no scheduled offence for 12 months
- Have no outstanding offences
- Complete a rehabilitation course successfully'),
(N'Recognising Different Hazards', N'**What you will learn**
- What a hazard is
- The main types of hazard
- The extra hazards motorcyclists face

**What is a hazard?**
A hazard is anything that could cause danger. Some are easy to see, such as a pothole. Others are hidden, such as a pedestrian behind a parked van.

**Types of hazard**
- Road design: sharp bends, junctions, narrow roads and steep hills
- Weather and surface: rain, fog, oil, sand and potholes
- Other road users: pedestrians, cyclists, motorcyclists, buses and lorries
- Vehicle condition: worn tyres, weak brakes and broken lights
- Yourself: tiredness, distraction, anger or rushing

**Hazards for motorcyclists**
Motorcyclists are hard for other drivers to see. They are also at risk from vehicles changing lanes without checking, and from potholes and loose surfaces.

**Look for combinations**
Danger often comes from several hazards together, such as rain, a bend and a parked lorry. Watch the whole scene, not just one thing.'),
(N'How Hazards Lead to Collisions', N'**What you will learn**
- How a collision develops
- The difference between a trigger and a contributing factor
- How to break the chain

**A chain of events**
Most collisions are not sudden accidents. They develop through a series of events that could have been stopped.

**Trigger**
The action that creates the immediate conflict, such as changing lanes into the path of a motorcycle without checking.

**Contributing factors**
Conditions that make a collision more likely or more serious:
- Poor observation, such as not checking the blind spot
- Driving too fast for the conditions
- Following too closely
- Tiredness or distraction

**Breaking the chain**
- Check your mirrors and blind spot before you move
- Keep a safe speed and distance so you have time to react
- Plan ahead and spot hazards early

**Reducing harm**
Seat belts, helmets and protective clothing reduce injuries, but they do not prevent the collision. Prevention always comes first.'),
(N'Observation and Situational Awareness', N'**What you will learn**
- What good observation means
- How to spot hidden hazards
- How to decide which hazard matters most

**More than just looking**
Good observation means understanding what is happening around you. Use your eyes and ears, and keep scanning ahead, to the sides and in your mirrors.

**Ask yourself**
- What can I see?
- What might be hidden?
- What could happen next?

**Hidden hazards**
Parked vehicles, bus stops, bends and hedges can hide people and vehicles. Expect someone to step out and give yourself time and space to react.

**Motorcycles**
Motorcycles are small and fast, and they are easily hidden by other vehicles or your car''s pillars. Look carefully before you turn or change lanes.

**Set priorities**
Deal first with the hazard that could affect you soonest or most seriously. Keep checking as the situation changes.'),
(N'Making a Safe Driving Plan', N'**What you will learn**
- The IPDA model for handling hazards
- The driving action plan
- Why your plan must change with the situation

**The IPDA model**
- Identify: spot the hazard
- Predict: work out how it might develop
- Decide: choose a safe response
- Act: carry out your decision smoothly

**The driving action plan**
Once you have decided what to do, work through:
- Information: keep observing all around you
- Position: put the vehicle in the safest position
- Speed: adjust to a safe speed
- Gear: choose a suitable gear for that speed
- Acceleration: accelerate smoothly once the way is clear

**Keep reassessing**
A plan that was safe a moment ago may need to change if another road user moves. Do not speed up past a hazard until you are sure the path is clear.'),
(N'Speed, Stress, and Driver Readiness', N'**What you will learn**
- How speed affects your reaction time
- How stress, tiredness and distraction affect driving
- When to stop driving

**Speed**
The faster you go, the less time you have to spot a hazard and react, and the longer it takes to stop. Choose a speed that suits the road, traffic and weather, not just the limit.

**Things that affect your judgement**
- Stress and anger: lead to risky decisions
- Tiredness: slows your reactions and can make you fall asleep
- Distraction: mobile phones, eating or passengers take your attention off the road
- Alcohol, drugs and some medicines: affect alertness and judgement

**Plan your journey**
Leave enough time so you are not rushing, and plan rest stops on long trips.

**When to stop**
Yawning, heavy eyes, drifting in your lane or missing signs are warning signs. Stop in a safe place such as a rest area and rest. Opening a window or turning up the music will not fix tiredness.'),
(N'Mirrors, Signals, and Blind Spots', N'**What you will learn**
- The CITO safety routine
- Where blind spots are
- The blind spots of large vehicles

**The CITO routine**
Use it before you move off, change lanes, turn or stop:
- C (Cermin): check your mirrors
- I (Isyarat): signal your intention
- T (Tinjau): look over your shoulder to check the blind spot
- O (Olah gerak): make the manoeuvre when it is safe

**Blind spots**
Mirrors do not show everything. There are areas beside and behind your vehicle that you can only see by turning your head. A motorcycle can easily be hidden there.

**Signalling**
A signal tells others what you plan to do. It does not give you the right to move. Wait until there is a safe gap.

**Large vehicles**
Lorries and buses have large blind spots beside and behind them. If you cannot see the driver in their mirror, they probably cannot see you. Do not stay alongside them for long.'),
(N'Following Distance and Looking Ahead', N'**What you will learn**
- The two-second rule
- When to use the four-second rule
- Why looking far ahead matters

**The two-second rule**
- Pick a fixed object ahead, such as a tree or lamp post
- When the vehicle in front passes it, count "one thousand and one, one thousand and two"
- If you reach the object before you finish counting, you are too close. Drop back.

**The four-second rule**
Leave at least four seconds in wet or slippery conditions, when towing, or when following large vehicles that block your view.

**Why it matters**
A safe gap gives you enough time and space to slow down or stop safely. It also gives you a wider view of the road ahead.

**Look well ahead**
Do not just watch the vehicle in front. Look further ahead for brake lights, junctions and hazards so you can react early.'),
(N'Overtaking and Being Overtaken', N'**What you will learn**
- When overtaking is safe
- How to overtake step by step
- What to do when someone overtakes you

**Before you overtake, ask**
- Is overtaking allowed? Check for continuous lines and No Overtaking signs.
- Can I see far enough ahead?
- Is there enough space to pass and return safely?

**Do not overtake**
- Near bends, hilltops, junctions or pedestrian crossings
- Where the road narrows
- When the vehicle in front is slowing for a hazard

**How to overtake**
- Use the CITO routine before moving out
- Pass with a safe gap beside the vehicle
- Check your mirror and signal before returning
- Return only when you can see the passed vehicle in your mirror. Do not cut in.

**Being overtaken**
Keep left, hold a steady speed and do not speed up. Leave room for the overtaking vehicle to pull back in.'),
(N'Junctions and Turning', N'**What you will learn**
- How to approach a junction
- How to turn safely
- Why priority does not guarantee safety

**Plan and position**
Know where you want to go before you reach the junction. Choose the correct lane early and signal in good time.

**Observe and slow down**
Reduce speed as you approach. Look out for pedestrians, cyclists, motorcyclists and traffic from all directions. If your view is blocked, edge forward slowly and wait until you can see clearly.

**Turning left**
Keep close to the left, check for motorcycles and cyclists on your left, and turn into the left lane.

**Turning right**
Move towards the centre of the road, give way to oncoming traffic and turn when there is a safe gap.

**Priority**
Having priority does not protect you from a collision. If another driver looks likely to pull out, slow down and be ready to stop. Do not change direction suddenly at the last moment.'),
(N'Roundabouts and Traffic Flow', N'**What you will learn**
- How to approach a roundabout
- How to choose your lane
- How to exit safely

**Approach**
Look for the signs that show the exits, and decide which one you need. Slow down so you have time to observe.

**Give way**
Give way to traffic already on the roundabout, which comes from your right. Drive around it in a clockwise direction.

**Choosing your lane**
Follow the signs and road markings. As a general guide:
- Left lane: for the first exit
- Right lane: for exits beyond straight ahead
- Either lane: for straight ahead, as marked

Layouts differ, so always follow the markings at each roundabout.

**Exiting**
Check your mirrors and signal left after you pass the exit before yours. Watch for motorcycles beside you as you leave.'),
(N'Manoeuvres and Collision Prevention', N'**What you will learn**
- The main types of manoeuvre
- The types of conflict to avoid
- How to reverse and turn around safely

**Types of manoeuvre**
- Entering traffic: from a parking space or side road
- Moving with traffic: keeping a safe speed and position
- Crossing traffic: at junctions
- Overtaking
- Changing direction: turning and U-turns
- Leaving traffic: stopping at the roadside or turning off

Each one needs good observation, enough space and controlled speed. Use the CITO routine every time.

**Types of conflict**
- Front: hitting the vehicle ahead
- Rear: being hit from behind after braking suddenly
- Side: during lane changes and turns
- Overtaking: with oncoming vehicles

**Reversing and turning around**
Your view behind is limited, so move slowly, look all around and ask someone to guide you if needed. Never reverse onto a main road.'),
(N'Urban Roads and Vulnerable Road Users', N'**What you will learn**
- The hazards on town roads
- How to share the road with vulnerable road users
- Why patience matters in traffic

**Town hazards**
Town roads have many junctions, traffic lights, parked cars, buses and frequent stops. Pedestrians often cross near shops, schools and markets, sometimes between parked vehicles.

**Vulnerable road users**
Pedestrians, cyclists and motorcyclists are easily hurt.
- Give them plenty of space
- Check for motorcycles before you turn or change lanes
- Watch for car doors opening and people stepping out from parked vehicles

**Keep a safe speed**
Drive slowly enough to stop if someone steps out. Be especially careful near schools and markets.

**Be patient**
In heavy traffic, avoid weaving between lanes. Saving a few seconds is never worth a collision.'),
(N'Rural Roads and Tunnels', N'**What you will learn**
- The hazards on country roads
- How to choose a safe speed
- How to drive through a tunnel

**Country road hazards**
- Narrow roads and sharp bends with limited views
- Uneven surfaces, potholes and soft shoulders
- Animals on the road
- Slow farm vehicles

**Choose a safe speed**
Drive at a speed that lets you stop within the distance you can see. A quiet road is not necessarily a safe road.

**Driving through tunnels**
- Switch on your headlights before entering
- Your eyes need time to adjust to the darkness, so slow down at the entrance
- Keep a safe distance and stay in your lane
- Expect bright light at the exit and be ready for glare'),
(N'Expressways', N'**What you will learn**
- How to join and leave an expressway
- Lane discipline on expressways
- The correct use of emergency lanes and rest areas

**Joining**
Use the slip road to build up your speed. Check your mirrors and blind spot, signal and merge into a safe gap.

**On the expressway**
- Keep left unless overtaking
- Keep a safe distance, because speeds are higher
- Read direction signs early and move into the correct lane in good time

**Leaving**
Watch for exit signs and move into the left lane early. Slow down on the exit ramp, not on the main road.

**Stopping**
- The emergency lane is only for emergencies and breakdowns. Never use it to overtake or skip a queue.
- Use rest areas (R&R) to take a break.
- Use motorcycle lanes or shelters where they are provided.'),
(N'Night Driving', N'**What you will learn**
- Why night driving is more dangerous
- How to use your lights correctly
- How to deal with glare and tiredness

**Why it is harder**
At night you can see less and hazards appear later. Pedestrians, cyclists and animals are hard to spot.

**Before you set off**
Check that all your lights work and that your windscreen and mirrors are clean.

**Using your lights**
- Use high beam on dark roads with no traffic ahead
- Switch to low beam for oncoming traffic or when following another vehicle, so you do not dazzle other drivers

**Dealing with glare**
If oncoming lights dazzle you, slow down and look towards the left edge of the road.

**Speed and tiredness**
Drive at a speed that lets you stop within the distance lit by your headlights. Tiredness is more likely at night, so take regular breaks.'),
(N'Rain, Flooding, Fog, and Crosswinds', N'**What you will learn**
- How rain affects your grip and view
- What aquaplaning is
- How to deal with floods, fog and strong winds

**Rain**
Wet roads reduce grip and make stopping distances longer. Switch on your headlights, slow down and keep at least four seconds behind the vehicle in front. Brake, steer and accelerate smoothly.

**Aquaplaning**
This happens when a layer of water builds up between your tyres and the road, and you lose control. If it happens, ease off the accelerator and hold the steering straight. Do not brake hard.

**Floods**
Never drive into floodwater if you cannot tell how deep it is. The road underneath may be damaged, and a current can sweep a vehicle away. Do not follow another vehicle through it. Turn back and find another route.

**Fog**
Slow down, use low-beam headlights or fog lights, and increase your following distance.

**Crosswinds**
Strong winds can push your vehicle sideways, especially on bridges, open roads and when passing large vehicles. Hold the steering firmly. Motorcyclists should take extra care.'),
(N'Recognising Vehicle Emergencies', N'**What you will learn**
- The common vehicle emergencies
- How to respond to each one
- Your priorities in any emergency

**Common emergencies and what to do**
- Brake failure: pump the brake pedal, change to a lower gear and use the handbrake gently
- Skidding: ease off the accelerator and steer gently in the direction you want to go
- Stuck accelerator: shift to neutral, brake and steer to a safe place
- Tyre burst: hold the steering firmly, do not brake hard and let the vehicle slow down
- Headlight failure at night: slow down, switch on hazard lights and stop safely
- Bonnet opening while driving: look through the gap or out of the side window and stop safely
- Fire: stop, switch off the engine, get everyone out and call for help

**Your priorities**
- Keep control of the vehicle
- Watch the traffic around you
- Stop in a safe place as soon as you can

Read your vehicle manual so you know what to do before an emergency happens.'),
(N'Stopping Safely After a Breakdown', N'**What you will learn**
- How to stop safely after a breakdown
- How to warn other road users
- How to call for help

**Get clear of traffic**
If you can, move the vehicle to the road shoulder or emergency lane, away from moving traffic. Avoid stopping on a bend or the top of a hill.

**Warn others**
- Switch on your hazard lights
- Place the warning triangle far enough behind your vehicle for approaching drivers to see it in time
- At night, keep your sidelights on

**Stay safe**
On an expressway, get all passengers out and stand behind the safety barrier, well away from traffic. Do not try to repair the vehicle next to moving traffic.

**Call for help**
Contact the expressway operator or a breakdown service. Give your location clearly, for example the kilometre marker and direction of travel.'),
(N'Emergency Equipment', N'**What you will learn**
- The emergency equipment to carry
- How to keep it ready
- The limits of roadside repairs

**What to carry**
- Car: warning triangle, torch, first-aid kit, spare tyre, jack and wheel spanner
- Motorcycle: small toolkit, torch, first-aid kit and reflective vest

**Keep it ready**
Know where each item is stored. Check it regularly. Make sure the spare tyre has the right pressure and the torch works.

**Using it safely**
- Use a jack only on firm, level ground, at the jacking points shown in the manual
- Do not attempt repairs you are not trained for, especially electrical or engine work
- If the location is not safe, call for assistance instead

Equipment helps you handle an emergency, but it does not replace a safe plan.'),
(N'What to Do After a Collision', N'**What you will learn**
- What to do immediately after a collision
- How to call for help
- What details to record and report

**Make the scene safe**
- Stop and warn other road users
- Switch on your hazard lights and place the warning triangle at a suitable distance
- If your vehicle can still move and no one is badly hurt, move it to the side of the road

**Get help**
If anyone is seriously injured, call 999 for an ambulance immediately. Help only within your training, and do not move an injured person unless they are in danger.

**Record the details**
Once it is safe, exchange and write down:
- Names, phone numbers and licence details
- Vehicle registration numbers
- Insurance details
- The place, date and time

**Report it**
Report the collision to the police, especially if anyone was injured or property was seriously damaged. Inform your insurance company and keep copies of all records.'),
(N'Getting to Know Your Vehicle', N'**What you will learn**
- The main parts of a vehicle
- How different vehicle types differ
- Why knowing your vehicle matters

**The main parts**
- Controls: steering, accelerator, brakes and gear selector
- Lights, mirrors and horn
- Tyres and braking system
- Fluids: fuel, engine oil, coolant and brake fluid
- Electrical system: battery, fuses and wiring

**Different vehicle types**
Petrol, diesel, hybrid and electric vehicles have different systems. An electric vehicle has no engine oil, but it has a high-voltage battery that needs special care. Motorcycles come in different designs too.

**Use the manual**
Your vehicle''s manual tells you which checks apply and how to do them.

**Why it matters**
When you know your vehicle well, you notice changes early, such as a new noise, a warning light or a leak, before they become a danger.'),
(N'Motorcycle Inspection and Maintenance', N'**What you will learn**
- What to check before every ride
- How to look after the drive system
- When to get professional help

**Before every ride**
- Fuel or battery charge
- Engine oil level
- Tyres: correct pressure, enough tread and no cuts
- Brakes: front and rear working properly
- Lights, indicators and horn working
- Mirrors clean and adjusted

**Drive system**
Motorcycles use a chain, belt or shaft drive. For a chain, check the slack (usually 15 to 20 mm) and keep it clean and oiled. Belt and shaft drives need different checks, so follow your manual.

**Controls**
Check that the throttle, clutch and brake levers move freely and return properly.

**Get help when needed**
Do not ride with a known defect, even for a short trip. Have faults repaired by a qualified mechanic.'),
(N'Car Inspection and Maintenance', N'**What you will learn**
- The regular checks for a car
- How to check fluids safely
- Why the vehicle manual matters

**Regular checks**
- Fuel or battery charge
- Engine oil, coolant, brake fluid and power steering fluid
- Windscreen washer fluid
- Lights and indicators
- Tyres: pressure, tread and damage
- Wipers, hoses and belts

**Checking fluids safely**
Check levels with the engine off and cool, on level ground. Never open the radiator cap while the engine is hot.

**Use the right fluid**
Each reservoir needs a specific type of fluid. Check the manual before topping up. The wrong fluid can damage the car.

**Look for warning signs**
Leaks under the car, warning lights, unusual noises or changes in how the car drives all need prompt attention.

Hybrid and electric cars have high-voltage systems that should only be serviced by trained technicians.'),
(N'Building a Maintenance Routine', N'**What you will learn**
- The difference between daily checks and servicing
- Why you should keep records
- How regular maintenance keeps you safe

**Checks before you drive**
Quick checks before a journey help you spot immediate problems such as a flat tyre, a broken light or a leak.

**Regular servicing**
Servicing deals with wear over time, such as changing the engine oil, filters and brake pads. Follow the time and mileage intervals in your manual.

**Keep records**
Record each service, repair and fault you notice. This helps you and your mechanic keep track of the vehicle''s condition.

**Act quickly**
Get problems with tyres, brakes, lights or visibility fixed straight away.

**The benefits**
Good maintenance makes your vehicle safer and more reliable, makes it last longer and reduces the chance of a breakdown on the road.'),
(N'Motorcycle Protective Equipment', N'**What you will learn**
- How to choose and wear a helmet
- The other protective gear you need
- How to make yourself visible

**Helmets**
Wearing a helmet is required by law and can save your life in a crash. A good helmet:
- Meets Malaysian Standard MS 1 or UN Regulation No. 22
- Fits snugly and is fastened securely with the chin strap
- Has a clear, untinted visor

A loose or unfastened helmet can come off in a crash and will not protect you.

**Other protective gear**
- Gloves: protect your hands and improve grip
- Strong, closed shoes or boots: protect your feet and ankles
- A jacket and long trousers: protect your skin
- Rainwear: keeps you dry and comfortable

**Be visible**
Wear bright clothing or a reflective vest, especially at night or in rain. Reflective strips on your helmet or motorcycle help other drivers see you.

Protective gear reduces injuries, but it cannot make unsafe riding safe.'),
(N'Seat Belts and Head Restraints', N'**What you will learn**
- Why seat belts are important
- How to wear a seat belt correctly
- How to adjust the head restraint

**Why wear a seat belt?**
The law requires seat belts for the driver and passengers in the front and back. A seat belt:
- Reduces the risk of serious injury in a crash
- Stops you hitting the inside of the car or being thrown out

**Wearing it correctly**
- The shoulder strap goes across your chest and shoulder, never under your arm
- The lap strap goes low across your hips, not over your stomach
- Make sure the belt is not twisted or loose

**Head restraints**
A correctly adjusted head restraint reduces neck injuries in a rear-end crash.
- The top should be at least level with your ears
- The gap between the back of your head and the restraint should be no more than 3.5 inches, about one fist

**Every occupant**
Make sure every passenger is wearing a seat belt before you move off.'),
(N'Child Passenger Safety', N'**What you will learn**
- How to choose the right child restraint
- How to install it correctly
- What child locks do

**Choose the right restraint**
Adult seat belts do not fit small children properly. Use a restraint that suits the child''s age and weight:
- Baby seat: for infants, usually rear-facing
- Child seat: for young children, forward-facing
- Booster seat: for children aged about 4 to 11 years and weighing 15 to 36 kg, so the adult belt fits across the chest and hips

**Install it correctly**
- Fit the restraint in the back seat, following its instructions and the vehicle manual
- Never put a rear-facing baby seat in front of an active airbag

**Child locks**
Child locks stop the rear doors from being opened from inside. They keep children from opening the door while the car is moving, but they do not protect them in a crash. Children still need the correct restraint.'),
(N'ABS and Airbags', N'**What you will learn**
- What ABS does
- How airbags work
- Why safety systems do not replace safe driving

**Anti-lock Braking System (ABS)**
ABS stops the wheels from locking during hard braking, so you can still steer. In an emergency, press the brake firmly and keep pressing. You may feel the pedal pulse, which is normal.

ABS does not give you extra grip and may not shorten your stopping distance, especially on wet or loose roads. Keep a safe distance.

**Airbags**
Airbags inflate in a serious crash to cushion you. They can be found in the steering wheel, dashboard, seats and side panels. They are a supplementary safety feature that works together with your seat belt, so always wear it. Sit at a proper distance from the steering wheel.

**Technology is not a substitute**
Safety systems help reduce the risk of injury, but they cannot make up for unsafe driving. Keep your speed under control and stay alert.');
    UPDATE l SET Content=c.Content, UpdatedAt=SYSUTCDATETIME()
        FROM dbo.Lessons l INNER JOIN @Content c ON c.Title=l.Title
        WHERE l.Content LIKE N'%**Check your understanding**%' AND l.Content LIKE N'%**Source**%';
    INSERT dbo.CurriculumImports(Version,ImportedAt) VALUES(N'kpp-2025-content-v2',SYSUTCDATETIME());
END
-- Sign, signal and marking lessons are learned from the picture, so show it before the text (once; admin choices kept afterwards).
IF NOT EXISTS (SELECT 1 FROM dbo.CurriculumImports WHERE Version=N'kpp-2025-image-above-v1')
BEGIN
    UPDATE dbo.Lessons SET ImagePosition=N'Above', UpdatedAt=SYSUTCDATETIME()
        WHERE ImagePosition=N'Below' AND ImagePath LIKE N'Content/images/learning/kpp-2025/%'
          AND Title IN (N'How to Read the Road',N'Traffic Lights',N'Police and Road User Hand Signals',N'Lines, Crossings, and Lane Markings',N'Warning Signs: Road Layout and Surface',N'Warning Signs: People, Animals, and Crossings',N'Prohibitory Signs and Restrictions',N'Mandatory Signs',N'Information and Direction Signs',N'Temporary Signs and Roadworks');
    INSERT dbo.CurriculumImports(Version,ImportedAt) VALUES(N'kpp-2025-image-above-v1',SYSUTCDATETIME());
END
COMMIT TRANSACTION;
