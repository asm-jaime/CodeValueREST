echo '------------START-----REMOVING-----DOCKER----IMAGES------------------------'
docker rmi $(docker images --format "{{.Repository}}:{{.Tag}}" | findstr codevaluerest) -f
echo '------------DOCKER-----IMAGES------WAS------REMOVED---------------------'
