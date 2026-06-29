# ============================================================
# Debezium SQL Server Connector for Azure Event Hubs (Kafka)
# ============================================================
FROM quay.io/debezium/connect:2.6

USER root

RUN printf '%s\n' \
    '#!/bin/bash' \
    'set -e' \
    '' \
    'echo "=== Building JAAS config from connection string ==="' \
    'JAAS_CONFIG="org.apache.kafka.common.security.plain.PlainLoginModule required username=\"\$ConnectionString\" password=\"${EVENTHUBS_CONNECTION_STRING}\";"' \
    'echo "JAAS set: $(echo ${JAAS_CONFIG} | cut -c1-50)..."' \
    '' \
    '# Export all CONNECT_* vars — Debezium entrypoint reads these' \
    'export CONNECT_BOOTSTRAP_SERVERS="${BOOTSTRAP_SERVERS}"' \
    'export CONNECT_GROUP_ID="${GROUP_ID}"' \
    'export CONNECT_CONFIG_STORAGE_TOPIC="${CONFIG_STORAGE_TOPIC}"' \
    'export CONNECT_OFFSET_STORAGE_TOPIC="${OFFSET_STORAGE_TOPIC}"' \
    'export CONNECT_STATUS_STORAGE_TOPIC="${STATUS_STORAGE_TOPIC}"' \
    'export CONNECT_CONFIG_STORAGE_REPLICATION_FACTOR=1' \
    'export CONNECT_OFFSET_STORAGE_REPLICATION_FACTOR=1' \
    'export CONNECT_STATUS_STORAGE_REPLICATION_FACTOR=1' \
    'export CONNECT_KEY_CONVERTER="org.apache.kafka.connect.json.JsonConverter"' \
    'export CONNECT_VALUE_CONVERTER="org.apache.kafka.connect.json.JsonConverter"' \
    'export CONNECT_KEY_CONVERTER_SCHEMAS_ENABLE="false"' \
    'export CONNECT_VALUE_CONVERTER_SCHEMAS_ENABLE="false"' \
    'export CONNECT_REST_PORT=8083' \
    'export CONNECT_REST_ADVERTISED_HOST_NAME="localhost"' \
    'export CONNECT_SECURITY_PROTOCOL="SASL_SSL"' \
    'export CONNECT_SASL_MECHANISM="PLAIN"' \
    'export CONNECT_SASL_JAAS_CONFIG="${JAAS_CONFIG}"' \
    'export CONNECT_PRODUCER_SECURITY_PROTOCOL="SASL_SSL"' \
    'export CONNECT_PRODUCER_SASL_MECHANISM="PLAIN"' \
    'export CONNECT_PRODUCER_SASL_JAAS_CONFIG="${JAAS_CONFIG}"' \
    'export CONNECT_CONSUMER_SECURITY_PROTOCOL="SASL_SSL"' \
    'export CONNECT_CONSUMER_SASL_MECHANISM="PLAIN"' \
    'export CONNECT_CONSUMER_SASL_JAAS_CONFIG="${JAAS_CONFIG}"' \
    '' \
    'echo "=== Starting Debezium via original entrypoint ==="' \
    'exec /debezium/run.sh' \
    > /custom-entrypoint.sh \
    && chmod +x /custom-entrypoint.sh

USER 1001

EXPOSE 8083

ENTRYPOINT ["/custom-entrypoint.sh"]