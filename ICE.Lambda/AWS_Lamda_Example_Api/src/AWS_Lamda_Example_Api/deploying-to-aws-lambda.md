## Deploying to AWS Lambda

1. Install the Amazon.Lambda.Tools Global Tool if not already installed:

```dotnet tool install -g Amazon.Lambda.Tools```


2. Deploy the application:
     
  ```cd "AWS_Lamda_Example_Api/src/AWS_Lamda_Example_Api" dotnet lambda deploy-serverless```

  Or, if you want to deploy the application to a specific AWS profile:

  ```cd "AWS_Lamda_Example_Api/src/AWS_Lamda_Example_Api" dotnet lambda deploy-serverless --profile <profile-name>```

  Or, use the AWS Publishing Extension for Visual Studio Code:

    https://marketplace.visualstudio.com/items?itemName=AmazonWebServices.AWSToolkitforVisualStudio2022

