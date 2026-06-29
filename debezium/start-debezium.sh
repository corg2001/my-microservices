#!/bin/bash
# ============================================================
# Custom startup for Debezium on Azure Event Hubs.
# Constructs JAAS config from EVENTHUBS_CONNECTION_STRING
# and sets all required Kafka Connect env vars before
# handing off to the original Debezium entrypoint.
# ============================================================

set -e

JAAS_CONFIG="org.apache.kafka.common.security.plain.PlainLoginModule required username=\"\$ConnectionString\" password=\"${EVENTHUBS_CONNECTION_STRING}\";"

# ── Kafka Connect distributed mode ────────────────────────────
export CONNECT_BOOTSTRAP_SERVERS="${BOOTSTRAP_SERVERS}"
export CONNECT_GROUP_ID="${GROUP_ID}"
export CONNECT_CONFIG_STORAGE_TOPIC="${CONFIG_STORAGE_TOPIC}"
export CONNECT_OFFSET_STORAGE_TOPIC="${OFFSET_STORAGE_TOPIC}"
export CONNECT_STATUS_STORAGE_TOPIC="${STATUS_STORAGE_TOPIC}"
export CONNECT_CONFIG_STORAGE_REPLICATION_FACTOR=1
export CONNECT_OFFSET_STORAGE_REPLICATION_FACTOR=1
export CONNECT_STATUS_STORAGE_REPLICATION_FACTOR=1

# ── SASL/SSL auth for Event Hubs ──────────────────────────────
export CONNECT_SECURITY_PROTOCOL="SASL_SSL"
export CONNECT_SASL_MECHANISM="PLAIN"
export CONNECT_SASL_JAAS_CONFIG="${JAAS_CONFIG}"
export CONNECT_PRODUCER_SECURITY_PROTOCOL="SASL_SSL"
export CONNECT_PRODUCER_SASL_MECHANISM="PLAIN"
export CONNECT_PRODUCER_SASL_JAAS_CONFIG="${JAAS_CONFIG}"
export CONNECT_CONSUMER_SECURITY_PROTOCOL="SASL_SSL"
export CONNECT_CONSUMER_SASL_MECHANISM="PLAIN"
export CONNECT_CONSUMER_SASL_JAAS_CONFIG="${JAAS_CONFIG}"

# ── REST API ──────────────────────────────────────────────────
export CONNECT_REST_PORT=8083
export CONNECT_REST_ADVERTISED_HOST_NAME="localhost"

# ── Converters ────────────────────────────────────────────────
export CONNECT_KEY_CONVERTER="org.apache.kafka.connect.json.JsonConverter"
export CONNECT_VALUE_CONVERTER="org.apache.kafka.connect.json.JsonConverter"
export CONNECT_KEY_CONVERTER_SCHEMAS_ENABLE="false"
export CONNECT_VALUE_CONVERTER_SCHEMAS_ENABLE="false"

echo "=== Debezium starting ==="
echo "Bootstrap servers: ${CONNECT_BOOTSTRAP_SERVERS}"
echo "Group ID: ${CONNECT_GROUP_ID}"
echo "Config topic: ${CONNECT_CONFIG_STORAGE_TOPIC}"

# Hand off to original Debezium entrypoint
exec /debezium-entrypoint.sh