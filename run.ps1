$containerName = "discord-bot"
$imageName = "discord-bot"
$tag = "latest"

# Run the container
$command = "docker run -d --name $containerName ${imageName}:${tag}"
Invoke-Expression $command
