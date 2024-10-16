docker build lambda --tag ice.lambda --progress=plain

$id = docker create ice.lambda.example
docker cp ${id}:/lambda/ICE.Lambda.Example.zip .
docker rm -v $id
