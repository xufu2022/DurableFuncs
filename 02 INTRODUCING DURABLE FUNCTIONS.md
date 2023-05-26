# INTRODUCING DURABLE FUNCTIONS

## What Are “Durable Functions”?

- An extension to Azure Functions
- Write “stateful” functions in a “serverless” environment
- Define workflows in code

> Triggers

Timer
Queue Message
HTTP Request

> Languages

C#, F#
JavaScript
Java, Python, PowerShell

> Bindings

Blob Storage
Cosmos DB
SendGrid

## What Is “Serverless”?

- Deploy without having to worry about infrastructure
- Automatic scaling
- Consumption-based pricing model

## Why Durable Functions?

- Function Chaining Workflow
- Fan-out Fan-in Workflow

## Durable Functions Basics

> Define workflows in code

- Parallel execution
- Error handling
- Easily understood “Orchestrator Function”

> Supports many workflow patterns

- Waiting for human interaction

> Solves the state problem

- Tracks workflow progress


1. Define workflows in code
    - Easy to understand the big picture
    - Good separation of concerns
1. Easy to implement complex workflows
    - Fan-out and fan-in
    - Wait for human interaction
1. Consolidate exception handling
1. Check on progress or cancel workflows
1. Manage state for you

## Key Durable Function Concepts

-   Orchestrator functions
    - Define the workflow
    - Triggers “activity” functions
    - Sleeps during activities
-   Activity functions
    - Performs a single step in a workflow
    - Can receive and return data
-   Starting “orchestrations”
    - DurableClient binding

## Durable Functions State Storage

- Durable Functions uses Azure Storage
- Storage Queues
     - Messages to trigger the next function
- Storage Tables
     - Store state of orchestrations
- Event sourcing
     - Never update rows, only append new ones
     - Store full execution history

> You provide the connection string

- Look inside with Azure Storage Explorer

> “Task Hub”

- The storage used by Durable Functions
- You can use multiple task hubs
- They can share a storage account

## Developing Durable Functions

- Azure Portal
    - Great for experimenting

- Command Line
    - Great for cross platform

- Visual Studio
    - Rich development experience

## Azure Storage Emulator

- local.settings.json
    - UseDevelopmentStorage=true
- Automatically started by Visual Studio
- Windows only
- Use a real Storage Account
    - Low cost
