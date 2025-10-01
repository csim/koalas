
$format = $args -contains "format"

$clean = $args -contains "clean"
$build = $args -contains "build"
$test = $args -contains "test"


$src = "$PSScriptRoot/src"
$solution = "$src/Koalas.sln"

if ($format) {
    dotnet csharpier format $src --log-format Console
}

if ($clean) {
    dotnet clean $solution -v:detailed --nologo
}

if ($build) {
    dotnet build $solution --no-incremental -v:detailed --nologo
}

if ($test) {
    dotnet test $solution -v:detailed --nologo
}
