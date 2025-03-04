$volumes = docker volume ls
$confVolumeExists = $volumes | Where-Object { $_ -like "*mongo-conf*" }
if (!$confVolumeExists)
{
    docker volume create mongo-conf
}

$dataVolumeExists = $volumes | Where-Object { $_ -like "*mongo-data*" }
if (!$dataVolumeExists)
{
    docker volume create mongo-data
}

$rootVolumeExists = $volumes | Where-Object { $_ -like "*mongo-root*" }
if (!$rootVolumeExists)
{
    docker volume create mongo-root
}
$networks = docker network ls
$networkExists = $networks | Where-Object { $_ -like "*cs-network*" }
if(!$networkExists )
{
   docker network create cs-network
}
docker run --name mongo-server --network=cs-network --volume=mongo-conf:/etc/mongo  --volume=mongo-data:/data/db --volume=mongo-root:/run/secrets/mongo-root --volume=/data/configdb --volume=/data/db -p 27017:27017 --restart=always -d mongo:latest
$hostsFile = "$env:SystemRoot\System32\drivers\etc\hosts"
$hostname = "mongo-server"
$ipAddress = "127.0.0.1"
$existingEntry = Get-Content $hostsFile | Where-Object { $_ -like "*$hostname*" }
if ($existingEntry) {
    Write-Host "Host entry for $hostname already exists."
} else {
    $newEntry = "$ipAddress`t$hostname"
    Add-Content -Path $hostsFile -Value $newEntry
    Write-Host "Host entry for $hostname added successfully."
}