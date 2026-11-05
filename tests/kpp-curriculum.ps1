$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$app = Join-Path $repo 'MyKPPLearn'
$expectedPath = [IO.Path]::GetFullPath((Join-Path $app 'App_Data/MyKPPLearn.mdf'))
$manifest = Get-Content (Join-Path $app 'App_Data/KppCurriculum.json') -Raw | ConvertFrom-Json
if ($manifest.chapters.Count -ne 10) { throw 'Expected ten chapters.' }
$lessons = @($manifest.chapters | ForEach-Object { $_.lessons })
if ($lessons.Count -ne 47) { throw 'Expected 47 lessons.' }
foreach ($lesson in $lessons) {
    if (-not (Test-Path (Join-Path $app $lesson.image))) { throw "Missing image: $($lesson.image)" }
    [xml]$svg = Get-Content (Join-Path $app $lesson.image) -Raw
    if (-not $svg.svg.title -or -not $svg.svg.desc) { throw 'Image needs an accessible title and description.' }
    if ($lesson.explanation.Length -lt 250 -or -not $lesson.answer -or -not $lesson.pages) { throw 'Incomplete lesson.' }
}
$connection = New-Object System.Data.SqlClient.SqlConnection 'Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=MyKPPLearn;Integrated Security=True'
$connection.Open()
function Query([string]$sql) {
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = $sql
    $cmd.CommandTimeout = 60
    try { return $cmd.ExecuteScalar() } finally { $cmd.Dispose() }
}
try {
    $actualPath = Query "SELECT physical_name FROM sys.database_files WHERE type=0"
    if ([IO.Path]::GetFullPath($actualPath) -ne $expectedPath) { throw 'Database does not match this workspace.' }
    $activeChapters = Query 'SELECT COUNT(*) FROM LearningChapters WHERE IsActive=1'
    $activeLessons = Query 'SELECT COUNT(*) FROM Lessons l JOIN LearningChapters c ON c.ChapterId=l.ChapterId WHERE l.IsActive=1 AND c.IsActive=1'
    if ($activeChapters -ne 10 -or $activeLessons -ne 47) { throw "Unexpected active curriculum: $activeChapters chapters / $activeLessons lessons." }
    Write-Output "Verified 10 chapters, 47 lessons and valid SVGs."
} finally {
    $connection.Dispose()
}
