# ============================================================
# Debezium SQL Server Connector for Azure Event Hubs (Kafka)
# Uses official Debezium entrypoint with CONNECT_* env vars.
# SASL JAAS config passed via KAFKA_OPTS Java system property.
# ============================================================
FROM quay.io/debezium/connect:2.6

USER root

RUN printf '%s\n' \
    '#!/bin/bash' \
    'set -e' \
    '' \
    'echo "=== Setting up JAAS config ==="' \
    '' \
    'mkdir -p /kafka/config/jaas' \
    'cat > /kafka/config/jaas/kafka-client.conf << EOF' \
    'KafkaClient {' \
    '  org.apache.kafka.common.security.plain.PlainLoginModule required' \
    '  username="$ConnectionString"' \
    '  password="${EVENTHUBS_CONNECTION_STRING}";' \
    '};' \
    'EOF' \
    '' \
    'export KAFKA_OPTS="-Djava.security.auth.login.config=/kafka/config/jaas/kafka-client.conf"' \
    '' \
    'echo "JAAS config written"' \
    'echo "=== Starting Debezium ==="' \
    '' \
    'exec /docker-entrypoint.sh' \
    > /custom-entrypoint.sh \
    && chmod +x /custom-entrypoint.sh

USER 1001

EXPOSE 8083

ENTRYPOINT ["/custom-entrypoint.sh"]