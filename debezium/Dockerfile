# ============================================================
# Debezium SQL Server Connector for Azure Event Hubs (Kafka)
# Uses standalone mode — simpler, no distributed topic issues
# ============================================================
FROM quay.io/debezium/connect:2.6

USER root

# Write a runtime config generator script
RUN printf '%s\n' \
    '#!/bin/bash' \
    'set -e' \
    '' \
    'echo "=== Generating Kafka Connect config ==="' \
    '' \
    'JAAS_CONFIG="org.apache.kafka.common.security.plain.PlainLoginModule required username=\"\$ConnectionString\" password=\"${EVENTHUBS_CONNECTION_STRING}\";"' \
    '' \
    'cat > /kafka/config/connect-distributed.properties << EOF' \
    'bootstrap.servers=my-microservices-eh.servicebus.windows.net:9093' \
    'group.id=debezium-cdc-group' \
    'config.storage.topic=cdc.configs' \
    'offset.storage.topic=cdc.offsets' \
    'status.storage.topic=cdc.status' \
    'config.storage.replication.factor=1' \
    'offset.storage.replication.factor=1' \
    'status.storage.replication.factor=1' \
    'key.converter=org.apache.kafka.connect.json.JsonConverter' \
    'value.converter=org.apache.kafka.connect.json.JsonConverter' \
    'key.converter.schemas.enable=false' \
    'value.converter.schemas.enable=false' \
    'rest.port=8083' \
    'rest.advertised.host.name=localhost' \
    'security.protocol=SASL_SSL' \
    'sasl.mechanism=PLAIN' \
    "sasl.jaas.config=${JAAS_CONFIG}" \
    'producer.security.protocol=SASL_SSL' \
    'producer.sasl.mechanism=PLAIN' \
    "producer.sasl.jaas.config=${JAAS_CONFIG}" \
    'consumer.security.protocol=SASL_SSL' \
    'consumer.sasl.mechanism=PLAIN' \
    "consumer.sasl.jaas.config=${JAAS_CONFIG}" \
    'EOF' \
    '' \
    'echo "=== Config generated, starting Kafka Connect ==="' \
    'exec /kafka/bin/connect-distributed.sh /kafka/config/connect-distributed.properties' \
    > /custom-entrypoint.sh \
    && chmod +x /custom-entrypoint.sh

USER 1001

EXPOSE 8083

ENTRYPOINT ["/custom-entrypoint.sh"]