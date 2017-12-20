# ChannelX: Transient Shared Communication Channels
ChannelX Project for the Software Engineering Course 411E

ChannelX: Transient Shared Communication Channels project aims to provide a secure chat channel between users with user-friendly and performance-wise fast interface.

## Architecture

![ChannelX Architecture](https://github.com/lyzerk/ChannelX/raw/master/docs/channelx_architecture.png "High Level Architecture")

## Screens

![Screens 1](https://github.com/lyzerk/ChannelX/raw/master/docs/intro_low.gif "Screens 1")

![Screens 2](https://github.com/lyzerk/ChannelX/raw/master/docs/chat_low.gif "Screens 2")

## Requirements
⋅⋅* dotnet core (https://www.microsoft.com/net/download/core)
⋅⋅* node & npm (https://nodejs.org/en/)
⋅⋅* [OPTIONAL] redis (https://redis.io)
⋅⋅* [OPTIONAL] email settings (we used mailgun api)

## Installation

Clone the repository
``` 
git clone https://github.com/lyzerk/ChannelX.git
``` 

Install npm packages
``` 
cd ChannelX/src
npm install
``` 

Then project is ready. You can just do

``` 
dotnet run
``` 

We kept the messages on redis server. But it is optional therefore, if you don't want to keep messages on channels, just don't install redis or disable redis on localhost

