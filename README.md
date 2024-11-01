# ICE SDK to API 

## What is this project? 

Using a C# .NET 8 AWS Lambda example API project, we’ve developed a solution to jumpstart your ICE SDK-to-API conversion, leveraging the latest tools from Microsoft and C# .NET 8. This project sets up an ASP.NET Core Web API running on AWS Lambda, accessible via Amazon API Gateway. You can clone, fork, or adopt some of the core concepts to expedite your transition.

## Watch the ICE SDK to API Webinar Demo

[![ICE SDK to API Webinar Demo](https://img.youtube.com/vi/p4SVw3v6w4U/maxresdefault.jpg)](https://www.youtube.com/watch?v=p4SVw3v6w4U)

While the codebase addresses important considerations, it was assembled in just two days. It’s intended to serve as a foundation to demonstrate how quickly you can start building your solution, especially if you're on the AWS platform where most of the necessary components are included.

## Key Features
- Authentication: Supports both Lender and ISV authentication.
- V3 Loan Field Reader/Writer: Enables efficient reading and writing of loan fields.
- Exclusive Loan Locking: Ensures loan data integrity through exclusive locking.
- Rate Limit Header: Rate limit information is included in the response object for monitoring.
- Performance Metrics: Execution time metrics are provided in the response object to track performance.
- Postman Collection: Included for easy debugging, testing, and development.

## Important Items:
- Exclusive Loan Locking Strategy: This example demonstrates obtaining an exclusive resource lock. However, you should consider using a work queue, as you won't always be able to secure an exclusive lock on a loan. At this time, we do not recommend using shared locks due to potential version mismatch issues, particularly with the smart client. In the field reader/writer's debug mode, there is code to unlock the loan, but we strongly advise against doing this in a production environment.
- Daily Rate Limit Handling: The implementation in this example is basic and rudimentary. We suggest employing a more sophisticated backoff strategy and recommend against using the approach demonstrated here in production.
- Concurrency: This example does not address concurrency management. Although header values are included in the response object, no strategy has been implemented for handling concurrency.
- Logging: Logging is not currently implemented in this example. We recommend using NLog, a flexible and free open-source logging solution for .NET, which we typically utilize in our solutions.
- Error Handling: Runtime error handling strategies are not implemented here. We recommend adhering to standard C# .NET best practices for error handling.
- Authentication and Loan Data Handling: This example provides a high-level demonstration of how to authenticate and read/write loan-level data while supporting exclusive locking. While it’s meant to add value, it was not developed with production-level code standards in mind.

This project is intended to serve as a quick-start guide to help you begin your SDK-to-API conversion process, but additional refinements and customizations will be necessary for production use.

## Table of Contents

- [Requirements](ICE.Lambda/AWS_Lamda_Example_Api/src/AWS_Lamda_Example_Api/requirements.md)
- [Project Structure](ICE.Lambda/AWS_Lamda_Example_Api/src/AWS_Lamda_Example_Api/project-structure.md)
- [Endpoints](ICE.Lambda/AWS_Lamda_Example_Api/src/AWS_Lamda_Example_Api/endpoints.md)
- [Running the Project](ICE.Lambda/AWS_Lamda_Example_Api/src/AWS_Lamda_Example_Api/running-the-project.md)
- [Testing the API](ICE.Lambda/AWS_Lamda_Example_Api/src/AWS_Lamda_Example_Api/testing-the-api.md)
- [Deploying to AWS Lambda](ICE.Lambda/AWS_Lamda_Example_Api/src/AWS_Lamda_Example_Api/deploying-to-aws-lambda.md)
- [ASP.NET Core Web API Serverless Application Reference Docs](ICE.Lambda/AWS_Lamda_Example_Api/src/AWS_Lamda_Example_Api/aspnet-core-web-api-serverless-application.md)

## What's Next?
- AWS Lambda API project for accepting ICE webhooks

## About us
After years of leadership in the mortgage industry, Chris Mace saw an opportunity to create something bigger than just another mortgage company—he wanted to transform the industry itself. In 2014, he founded Mace Innovations with a mission to revolutionize how mortgage companies operate. Since then, our cutting-edge tools have been adopted industry-wide, helping mortgage companies large and small save time, streamline processes, and significantly reduce costs.

Learn more about how we’re driving innovation in the mortgage space: Mace Innovations(https://maceinnovations.com/).



