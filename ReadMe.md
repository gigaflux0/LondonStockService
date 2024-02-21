# London Stock Service 
#### (Project for Tyl by George Mason)

## About:

This project is a REST API that receives trades and persists them to a NoSql document database as an add only series of records. It partitions on the ticker symbols allowing horizontal scaling of the records where each stock symbol can become its own container stream. It uses optimistic concurrency to further support the scaling but currently deviates conflicts to the caller to decide its own polly policy. A hexagonal architecture combined with CQRS architecture is being used. Although the benefits of CQRS aren't being realised this allows them to be later, such as through either a cache of the latest stock records or a projected read only capture of the current database. Two endpoints have been exposed: POST trades and GET stocks, although these use the same data store they have been named differently as they represent different contextual views of the data. The API is contained in the LondonStockService project, the use cases and stitching are contained in the Application project and the data persistence layer is contained in the Data.CosmosDb layer. The Test folder also contains some integration tests for the api and unit tests for the projects.

## To Run:

This is designed to use the CosmosDB NoSql document database emulator as a data store. To run this project you first have to download the emulator from this page: https://learn.microsoft.com/en-us/azure/cosmos-db/how-to-develop-emulator?tabs=windows%2Ccsharp&pivots=api-nosql#install-the-emulator. The download link is in step 1 of the 'Install the emulator' section, the docker versions should work fine but the local windows version is the easiest to get going. Once installed, run the emulator and wait for it to finish booting up before running this project.

I recommend running all the integration tests, this will quickly create and populate a database with data for you but manual playing with the api will have the same effect.

## Enhancements:

The first enhancement would be before starting to talk to a product owner to get the full context of what is trying to be achieved by this service, too much is left up to interpretation causing some assumptions that may not be true.

This system is scalable as is but it is left up to the caller to reattempt if a writing conflict has happened. The optimistic concurrency approach implemented will get the last record for that stock, validate the request with it, then attempt to push the record up with a version number incremented by 1. If another pod of this service attempted to write to that stock at the exact same time then only 1 will succeed while the other one will get a concurrency conflict. This is due to the version number field being marked as a unique index in the database. 

Depending on the full context of what problem this API is trying to solve, if the order of the trades received is very important and must be persisted, then I would hope whatever is calling this api has already given an order number as that shouldn't be left to the API to figure out. If it is up to this API to determine order, the current approach of rejecting conflicting trades would be best, if the API is suppose to guarantee order based on when they were received, this can't be guaranteed in a scalable system as we can't ever be entirely sure we received the packets in the order intended. If exact order is not important, then I would implement a jitter in polly to retry from the point of validation with the latest record only in the event the version number conflicted when trying to write. This would ensure a much higher chance the trade gets recorded even when writing to this stock in the database has ended up bottlenecked. As it is the database is the main bottleneck, although due to the use of the partition key it could be much worse, and the design makes improvments on that bottleneck simple such as read only replicas of projections.

## Enhancements, more time edition:

If more time was spent: 
- I would add a docker compose script for easy containerisation. 
- I would add Helm charts for easy orchestration in kubernetes. 
- I would add liveness and readiness health endpoints for pod scaling in kubernetes.
- I would add a script to run a swagger openapi script generation tool in the build step.
- I would add more test cases, unit tests to the API project and acceptance tests when the criteria is more refined.
- I would remove the code that auto generates a database and container if they are missing, and replace this with a Terraform infrastructure as code script instead.
- I would add authorisation and authentication to the endpoints, prefferably using a reputable third party IDP like Auth0.
- I would implement a full Result model using a package like FluentResults to remove all the ApplicationExceptions, replacing most of them with errors and leaving only the exceptional situations exceptional.
- I would remove the hard coded connection strings for the emulator, putting them in a Key Vault and a key vault client to access them, with it's details in turn isolated to the AppSettings json.
- I would implement optimisation in the read model either through a cache like Redis, or projection databases.