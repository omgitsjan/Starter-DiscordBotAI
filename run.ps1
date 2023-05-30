$containerName = "discordbotai"
$imageName = "discordbotai"
$tag = "latest"
$configFilePath = "$PSScriptRoot/DiscordBot/appsettings.json"

# Stop and remove the container if it is already running
$command = "docker stop $containerName"
Invoke-Expression $command
$command = "docker rm $containerName"
Invoke-Expression $command

# Run the container with the configuration file mounted
$command = "docker run -v ${configFilePath}:/app/appsettings.json -d --name $containerName ${imageName}:${tag}"
Invoke-Expression $command
