# Create build environment
FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build-env
WORKDIR /app
COPY . ./
RUN dotnet publish Costco.ECom.API.InventoryAvailability -c Release -o Costco.ECom.API.InventoryAvailability/out

# Create application container
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine

# Update and upgrade
#RUN apt-get update && apt-get upgrade -y \
 #   && rm -rf /var/lib/apt/lists/*
RUN rm -rf /var/lib/apt/lists/*

RUN mkdir -p /tmp/appinsightschannel

# Create a new user group and user to run the workload as non-root
RUN groupadd -r workload && useradd --no-log-init -r -g workload workload
USER workload

WORKDIR /app

# Copy build artifacts from previous stage build-env
COPY --from=build-env /app/Costco.ECom.API.InventoryAvailability/out .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Costco.ECom.API.InventoryAvailability.dll"]