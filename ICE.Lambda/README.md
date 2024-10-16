# ICE.Lambda

## Debugging

### Install the Fake Runtime API

If you haven't done so already, install the `Stackage.Aws.Lambda.FakeRuntime` package as a global tool using the `dotnet tool install --global` command.

```
dotnet tool install --global Stackage.Aws.Lambda.FakeRuntime
```

To update to the latest version of the `Stackage.Aws.Lambda.FakeRuntime` package use the `dotnet new update` command. Be aware that this will attempt to update all dotnet new template packages.

```
dotnet new update
```

### Start the Fake Runtime API

`fake-lambda-runtime`

### Start the Lambda function runtime

The Lambda functions is bootstrapped as a .NET console app so can be easily be started in debug mode in your favourite IDE. Use the `ICE.Lambda` profile in `launchSettings.json` or otherwise ensure the `AWS_LAMBDA_RUNTIME_API` is set to `localhost:9001/ICE.Lambda`.

### Invoke the Lambda function

The Lambda function can be invoked using one of the following methods:

A. cURL

Run the following in your console. If you use a console other than Powershell, you will most likely need to alter the escaping of quotes in the JSON body.

```ps
curl -v -X POST "http://localhost:9001/2015-03-31/functions/ICE.Lambda/invocations" -H "content-type: application/json" -d '{\"name\": \"FOO\"}'
```

B. Postman

Import the `ICE.Lambda.postman_collection.json` file into Postman, select the `Invoke Lambda` request of the `ICE.Lambda` collection and click `Send`.

C. AWS CLI

Run the following in your console. Again, if you use a console other than Powershell, you will most likely need to alter the escaping of quotes in the JSON body.

```ps
aws lambda invoke --endpoint-url http://localhost:9001 --function-name ICE.Lambda --payload '{\"name\": \"FOO\"}' --cli-binary-format raw-in-base64-out response.json
```

To perform an asynchronous invocation and not wait for the response, add `--invocation-type Event` to the command.

## Deploying

From the root of the repository, build the AWS Lambda deployment package using the `build-package` script.

`.\build-package.ps1`

This uses Docker to build the AWS Lambda deployment package `ICE.Lambda.zip` which has been optimised for Linux.

This can then be deployed using the AWS Console as described in [Creating Lambda functions defined as .zip file archives](https://docs.aws.amazon.com/lambda/latest/dg/configuration-function-zip.html).
