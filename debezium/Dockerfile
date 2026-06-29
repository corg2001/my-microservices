# ============================================================
# Debezium SQL Server Connector for Azure Event Hubs (Kafka)
# ============================================================
FROM quay.io/debezium/connect:2.6

USER root

# Write startup script inline to avoid CRLF line ending issues
RUN printf '#!/bin/bash\nset -e\n\nJAAS_CONFIG="org.apache.kafka.common.security.plain.PlainLoginModule required username=\\"\\$ConnectionString\\" password=\\"${EVENTHUBS_CONNECTION_STRING}\\";"\n\nexport CONNECT_BOOTSTRAP_SERVERS="${BOOTSTRAP_SERVERS}"\nexport CONNECT_GROUP_ID="${GROUP_ID}"\nexport CONNECT_CONFIG_STORAGE_TOPIC="${CONFIG_STORAGE_TOPIC}"\nexport CONNECT_OFFSET_STORAGE_TOPIC="${OFFSET_STORAGE_TOPIC}"\nexport CONNECT_STATUS_STORAGE_TOPIC="${STATUS_STORAGE_TOPIC}"\nexport CONNECT_CONFIG_STORAGE_REPLICATION_FACTOR=1\nexport CONNECT_OFFSET_STORAGE_REPLICATION_FACTOR=1\nexport CONNECT_STATUS_STORAGE_REPLICATION_FACTOR=1\nexport CONNECT_SECURITY_PROTOCOL="SASL_SSL"\nexport CONNECT_SASL_MECHANISM="PLAIN"\nexport CONNECT_SASL_JAAS_CONFIG="${JAAS_CONFIG}"\nexport CONNECT_PRODUCER_SECURITY_PROTOCOL="SASL_SSL"\nexport CONNECT_PRODUCER_SASL_MECHANISM="PLAIN"\nexport CONNECT_PRODUCER_SASL_JAAS_CONFIG="${JAAS_CONFIG}"\nexport CONNECT_CONSUMER_SECURITY_PROTOCOL="SASL_SSL"\nexport CONNECT_CONSUMER_SASL_MECHANISM="PLAIN"\nexport CONNECT_CONSUMER_SASL_JAAS_CONFIG="${JAAS_CONFIG}"\nexport CONNECT_REST_PORT=8083\nexport CONNECT_REST_ADVERTISED_HOST_NAME="localhost"\nexport CONNECT_KEY_CONVERTER="org.apache.kafka.connect.json.JsonConverter"\nexport CONNECT_VALUE_CONVERTER="org.apache.kafka.connect.json.JsonConverter"\nexport CONNECT_KEY_CONVERTER_SCHEMAS_ENABLE="false"\nexport CONNECT_VALUE_CONVERTER_SCHEMAS_ENABLE="false"\n\necho "=== Debezium starting ==="\necho "Bootstrap: ${CONNECT_BOOTSTRAP_SERVERS}"\n\nexec /debezium-entrypoint.sh\n' > /custom-entrypoint.sh \
    && chmod +x /custom-entrypoint.sh \
    && cp /docker-entrypoint.sh /debezium-entrypoint.sh \
    && chmod +x /debezium-entrypoint.sh

USER 1001

EXPOSE 8083

ENTRYPOINT ["/custom-entrypoint.sh"]