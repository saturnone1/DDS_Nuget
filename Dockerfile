FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY . .

ENV NDDSHOME=/src/rti/connext
ENV PATH="${NDDSHOME}/bin:${PATH}"

RUN chmod +x ./tools/generate-dds.sh
RUN if [ -x "${NDDSHOME}/bin/rtiddsgen" ]; then ./tools/generate-dds.sh; else echo "Skipping rtiddsgen: Linux RTI code generator was not found under rti/connext/bin."; fi
RUN dotnet restore ./DdsAmbassador.DDSClient.sln
RUN dotnet build ./DdsAmbassador.DDSClient.sln -c Release --no-restore
RUN dotnet pack ./src/DdsAmbassador.DDSClient/DdsAmbassador.DDSClient.csproj -c Release --no-build -o ./artifacts/packages
RUN dotnet publish ./src/DdsAmbassador.DDSClient.Cli/DdsAmbassador.DDSClient.Cli.csproj -c Release -r linux-x64 --self-contained false -o ./artifacts/ddsclient
RUN cp -R ./definitions ./artifacts/ddsclient/definitions

FROM scratch AS package
COPY --from=build /src/artifacts/packages /

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime
WORKDIR /app
COPY --from=build /src/artifacts/ddsclient ./
ENV DDS_CLIENT_CONFIG_PATH=/app/definitions/dds_client.asap.xml
ENTRYPOINT ["/app/ddsclient"]
