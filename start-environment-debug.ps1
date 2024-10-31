# usage:
# start-development.ps1
# start-development.ps1 -build
# start-development.ps1 -rebuild

param 
(
    [switch]$build = $false,
    [switch]$rebuild = $false
)

if ($rebuild)
{
   $no_cache = '--no-cache'
   $force_recreate = '--force-recreate'
   $build = $true
}

if ($build)
{
    $StartTime = $(get-date)

    docker-compose -f docker-compose.environment.debug.yml build ${no_cache} 

    echo '--------------------------------------------------'
    $elapsedTime = $(get-date) - $StartTime
    $totalTime = "{0:HH:mm:ss}" -f ([datetime]$elapsedTime.Ticks)
    write-host "Build time: $($totalTime)"
    echo '--------------------------------------------------'
}

docker-compose -f docker-compose.environment.debug.yml up ${force_recreate}
docker-compose -f docker-compose.environment.debug.yml rm -fsv
