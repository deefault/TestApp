FROM mcr.microsoft.com/dotnet/core/sdk:2.1 AS build-env
WORKDIR /app

# install System.Drawing native dependencies
RUN apt-get update \
    && apt-get install -y --allow-unauthenticated \
        libc6-dev \
        libgdiplus \
        libx11-dev \
        nodejs \
     && rm -rf /var/lib/apt/lists/*
     
# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore




# Copy everything else and build
COPY . ./
RUN npm install --production
RUN npm install --only=dev
RUN node_modules/.bin/bower install
RUN mkdir -p ./wwwroot/lib
RUN mv ./bower_components/* ./wwwroot/lib/  
RUN dotnet publish -c Release -o out

WORKDIR /app
RUN npm install --production
RUN npm install --only=dev

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:2.1
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "WebApplication3.dll"]