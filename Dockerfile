FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG SKIP_DDSGEN=false

WORKDIR /src

RUN apt-get update \
    && apt-get install -y --no-install-recommends cmake python3 \
    && rm -rf /var/lib/apt/lists/*

COPY . .

ENV NDDSHOME=/src/rti/connext
ENV PATH="${NDDSHOME}/bin:${PATH}"

RUN chmod +x ./tools/generate-dds.sh ./tools/validate-dds.py
RUN if [ -x "${NDDSHOME}/bin/rtiddsgen" ]; then \
      cmake -S . -B build -DDDS_GENERATE=ON; \
    elif [ "${SKIP_DDSGEN}" = "true" ]; then \
      echo "Skipping rtiddsgen because SKIP_DDSGEN=true."; \
      cmake -S . -B build -DDDS_GENERATE=OFF; \
    else \
      echo "rtiddsgen was not found under rti/connext/bin. Rebuild generated sources first or pass --build-arg SKIP_DDSGEN=true intentionally." >&2; \
      exit 1; \
    fi
RUN cmake --build build --target dds_all
RUN cmake --build build --target dds_publish_cli

FROM scratch AS package
COPY --from=build /src/artifacts/packages /

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime
WORKDIR /app
COPY --from=build /src/artifacts/ddsclient ./
ENV DDS_CLIENT_CONFIG_PATH=/app/definitions/dds_client.asap.xml
ENTRYPOINT ["dotnet", "/app/ddsclient.dll"]
