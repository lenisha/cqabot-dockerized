# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:3.1-alpine AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY CQABot/*.csproj ./CQABot/
RUN dotnet restore

# copy everything else and build app
COPY CQABot/. ./CQABot/
WORKDIR /source/CQABot
RUN rm -rf *.botproj
RUN cp language-generation/en-us/common.en-us.lg language-generation/en-us/common.lg
RUN cp language-generation/en-us/CQABot.en-us.lg language-generation/en-us/CQABot.lg
RUN dotnet publish -c release -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:3.1-alpine
WORKDIR /app
COPY --from=build /app ./

# See: https://github.com/dotnet/announcements/issues/20
# Uncomment to enable globalization APIs (or delete)
#ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
#RUN apk add --no-cache icu-libs
#ENV LC_ALL=en_US.UTF-8
#ENV LANG=en_US.UTF-8

EXPOSE 3978
# configure web servers to bind to port 80 when present
ENV ASPNETCORE_URLS=http://*:3978
    # Enable detection of running in a container
ENV DOTNET_RUNNING_IN_CONTAINERS=true 
 
#ENTRYPOINT ["./CQABot"]
ENTRYPOINT ["dotnet", "CQABot.dll", "--urls", "http://*:3978"]
