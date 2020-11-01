# Secure Programming Task 3 #

.Net + React implementation of the Secure Programming course task 3.

![alt text](https://i.ibb.co/LJ9B6VC/Screenshot-2020-11-01-190135.png)

## Table of Contents

- [Prerequisites](#remarks)
- [Installation](#installation)
- [Results](#Results)
- [Implementation](#Implementation)
- [Task](#Thetask)

### Prerequisites

1. Required:
    * [.NET Core 3.1.*](https://dotnet.microsoft.com/download/dotnet-core/3.1) - Hosting boundle runtime for hosting the backend service
    
2. Optional:
    * [IIS Express](https://docs.microsoft.com/en-us/iis/extensions/introduction-to-iis-express/iis-express-overview) - Lightweight, self-contained version of IIS optimized for developers
    * Development IDE
        * [Visual Studio](https://visualstudio.microsoft.com/downloads)
        * [Visual Studio Code](https://code.visualstudio.com/)
    * [Docker](https://www.docker.com/products/docker-desktop) -  A set of platform as a service products that use OS-level virtualization to deliver software in packages called containers.

## Installation 

To run the app either boot up the project with visual studio and run through there or build and run this project using docker. To run using docker:
1. Navigate to the root project directory and run `docker build -t sp3 .`
2. Run the command `docker run -p 80:80 sp3`
3. Navigate to `http://localhost:80` and use the application

## Results

Scanning through all the given documents following results were obtained:
Max prime: 199895387
Min prime: 3
Total files done: 1000

## Implementation

The task was done using a Rest API to control the backend and SignalR to transmit the live data to the frontend. Producer-Consumer pattern was used as the multithreaded workflow pattern.

![alt text](https://i.ibb.co/hC0hs1Z/Untitled-Diagram-22.png)

I decided to use both concurrent queue as well as a channel to try different implementation in one solution.
*Channels are basically a neweer and better version of concurrent queues.

## The task

Task 3. Concurrency collections and patterns

You have 1000 files with random numbers. When using n threads (n should be possible to be adjusted live), filter out only primary numbers when showing real-time statistics:
1. Number of threads
2. Number of files done
3. Max and Min found primary number

In this task you must use concurrent collections and producer - consumer pattern.

Point for the task:
  Producer/Consumer Design Pattern         - 4 points
  Live Threads Adjustment                  - 2 points
  Display Number of Threads                - 1 point
  Display Number of Files Done             - 1 point
  Display Max and Min Found Primary Number - 2 points
                         Total             - 10 points