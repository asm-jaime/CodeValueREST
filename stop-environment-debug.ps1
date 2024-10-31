
param 
(
	[switch]$rmi = $false
)

docker-compose -f docker-compose.environment.debug.yml down
docker rmi $(docker images --filter "dangling=true" -q --no-trunc)

if ($rmi)
{
	docker system prune -a -f --volumes
}
