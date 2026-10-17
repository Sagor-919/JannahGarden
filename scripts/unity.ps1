param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string] $Command,

    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]] $UnityArguments
)

$unityCliPath = Join-Path $env:LOCALAPPDATA 'Unity\bin\unity.exe'
if (-not (Test-Path -LiteralPath $unityCliPath -PathType Leaf)) {
    throw "Unity CLI was not found at '$unityCliPath'. Install it from https://unity.com/blog/meet-the-unity-cli."
}

& $unityCliPath $Command @UnityArguments
exit $LASTEXITCODE
