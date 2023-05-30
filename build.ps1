# Build script
param(
    [string]$dockerfilePath = "Dockerfile",
    [string]$imageName = "discordbotai",
    [string]$tag = "latest"
)

$command = 'docker build -f {0} -t {1}:{2} .' -f $dockerfilePath, $imageName, $tag
Invoke-Expression $command
